function Start-RulesEnvironment {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$InRuleVersion = "latest", 
        [string[]]$ComposeFilePath = ".\docker-compose.yml",
        [string]$Name = "inrule",
        [bool]$deleteOnStop = $true
        )

    write-verbose "Using a service name of $Name"
    write-warning "Names containing invalid characters will be changed, e.g. 'service-name' becomes 'servicename'"

    $ErrorActionPreference = 'Stop'
    #$composePath = Resolve-Path -Path $ComposeFilePath
    #write-verbose "using path $composePath"
    $composeFileArgs = $ComposeFilePath | foreach { "-f " + (Resolve-Path $_) }
    $passDeleteArg = ""
    if ($deleteOnStop -eq $true) {
        $passDeleteArg = "-d"
    }
    Invoke-Expression "docker-compose $composeFileArgs --project-name $Name up $passDeleteArg" | write-verbose
    
    #$svcIds = docker-compose ps -q
    write-debug "using alternate method of retrieving containers for this environment"
    $cons = (Invoke-Expression "docker-compose -p $Name ps -q")
    
    $cons | ForEach-Object {
            write-verbose "Setting HOST file info for $_" 
            Set-ContainerHostName -ContainerNameOrId $_ -Verbose 
        }
    return $svcIds
}

function Stop-RulesEnvironment {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(        
        [string]$EnvironmentName = "inrule"
    )
    write-verbose "Stopping rule environment $EnvironmentName"

    $svcIds = invoke-expression "docker-compose -p $EnvironmentName ps -q"
    if ($svcIds -eq $null -or $svcIds.Length -lt 1) {
        Write-warning "No containers running, therefore there is nothing to stop."
        return
    }
    if ($PSCmdlet.ShouldProcess("Remove HOSTS entries")) {
        $svcIds | ForEach-Object {         
            Clear-ContainerHostName -ContainerNameOrId $_ -Verbose
            write-verbose "Cleared $_ from HOSTS"
        }
    }    
    
    write-verbose "Stopping services $svcIds"

    
    if ($PSCmdlet.ShouldProcess("docker-compose down")) {
        write-verbose "Calling docker-compose down"
        invoke-expression "docker-compose -p $EnvironmentName down"
    }
    write-verbose "Stopped services."
}

Function Get-ContainerIpAddress {
    param([string]$ContainerNameOrId)
    return docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $ContainerNameOrId
}

Function Get-ContainerName {
    param([string]$ContainerNameOrId)
    # note that this function returns unity if the $ContainerNameOrId is a Name rather than an Id
    return (docker inspect -f '{{.Name}}' $ContainerNameOrId).TrimStart('/')
}

function Set-ContainerHostName {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$ContainerNameOrId,
        [string]$ContainerIp,
        [switch]$SkipContainerLookup = $false)

$ErrorActionPreference = "Stop"

    if ($SkipContainerLookup -eq $true) {
        write-verbose "Skipping container lookup. Provided IpAddress: $ContainerIp"
        if ($ContainerIp -eq '') {
            throw "When container lookup is skipped, ContainerIp must be specified." 
            return;
        }
        $hostEntry = $ContainerNameOrId + " " + $containerIP
    }
    else {
        $hostEntry = (Get-ContainerIpAddress -ContainerNameOrId $ContainerNameOrId) + " " + (Get-ContainerName -ContainerNameOrId $ContainerNameOrId) 
    }
    
    write-verbose "Constructed host entry $hostEntry"
    if ($hostEntry -eq $null -or [string]::IsNullOrWhiteSpace($hostEntry)) {
        write-verbose "Container $ContainerNameOrId not found. No entry will be written."
        return $hostEntry
    }
    $hostFile = resolve-Path "$env:windir\System32\drivers\etc\hosts"
    Write-Verbose "HOSTS file resolved to $hostFile"

    $entryExists = select-string -Path $hostFile -Pattern $hostEntry
    write-verbose "Checking for existing host entry..."
    if ($entryExists -ne $null) {
        write-verbose "Host entry already present in file. Exiting."
        return $hostEntry
    }
    if ($PSCmdlet.ShouldProcess($hostFile,"Add host entry $hostEntry")) {
        try {
            Add-Content -Path $hostFile -Value $hostEntry -ErrorAction 'stop'
            Write-Verbose "Added host file entry for $ContainerNameOrId"
        }
        catch [UnauthorizedAccessException] {
            Write-Host "The executing process has insufficient permissions to modify the HOSTS file. Please run this script again using elevated credentials"
            return;
        }
        
    }
    return $hostEntry
}

function Clear-ContainerHostName {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$ContainerNameOrId,
        [switch]$SkipContainerLookup = $false
    )
    $ErrorActionPreference = 'Stop'
    if ($SkipContainerLookup -eq $true) {
        $hostEntry = $ContainerNameOrId
    }
    else {
        $hostEntry = (Get-ContainerIpAddress -ContainerNameOrId $ContainerNameOrId) + " " + (Get-ContainerName -ContainerNameOrId $ContainerNameOrId) 
    }
    if ($SkipContainerLookup -eq $false -and ($hostEntry -eq $null -or [string]::IsNullOrWhiteSpace($hostEntry))) {
        write-verbose "Container $ContainerNameOrId not found. No entry will be removed."
        return $hostEntry
    }
    write-verbose "Constructed host entry $hostEntry"
    $hostFile = resolve-Path "$env:windir\System32\drivers\etc\hosts"
    Write-Verbose "HOSTS file resolved to $hostFile"
    $entryExists = select-string -SimpleMatch -Path $hostFile -Pattern $hostEntry
    write-verbose "Checking for existing host entry..."
    if ($entryExists -eq $null) {
        write-verbose "Host entry not present in file. Exiting."
        return $hostEntry
    }
    if ($PSCmdlet.ShouldProcess($hostFile,"Remove host entry $hostEntry")) {
        try {
            $backupName = "$hostFile.bak"
            Get-Content $hostFile | Set-Content -Path $backupName -ErrorAction 'stop' -force
            write-verbose "Created backup HOSTS file at $backupName"
            Get-Content $backupName | Where-Object { $_ -notmatch $hostEntry } | Set-Content -Path $hostFile -force
            Write-Verbose "Removed host file entry for $ContainerNameOrId"
        }
        catch [UnauthorizedAccessException] {
            Write-Host "The executing process has insufficient permissions to modify the HOSTS file. Please run this script again using elevated credentials"
            return;
        }        
    }

    return $hostEntry
}

function Start-RulesEnvironment {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$InRuleVersion = "latest",
        [string]$ComposeFilePath = ".\docker-compose.yml",
        [string]$Name = "inrule" # TODO: perform xform to abide by docker service name restrictions - lower case, no '-', etc
        )

    write-verbose "Using a service name of $Name"
    write-warning "Names containing invalid characters will be changed, e.g. 'service-name' becomes 'servicename'"

    $ErrorActionPreference = 'Stop'
    $composePath = Resolve-Path -Path $ComposeFilePath
    write-verbose "using path $composePath"
    
    Invoke-Expression "docker-compose -f $composePath --project-name $Name up -d" | write-verbose
    
    #$svcIds = docker-compose ps -q
    write-debug "using alternate method of retrieving containers for this environment"
    $cons = (Invoke-Expression "docker-compose -p $Name ps -q")
    
    $cons | ForEach-Object {
            write-verbose "Setting HOST file info for $_" 
            Set-ContainerHostName -ContainerNameOrId $_ -Verbose 
        }
    return $svcIds
}

function Stop-RulesEnvironment {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(        
        [string]$EnvironmentName = "inrule"
    )
    write-verbose "Stopping rule environment $EnvironmentName"

    $svcIds = invoke-expression "docker-compose -p $EnvironmentName ps -q"
    if ($svcIds -eq $null -or $svcIds.Length -lt 1) {
        Write-warning "No containers running, therefore there is nothing to stop."
        return
    }
    if ($PSCmdlet.ShouldProcess("Remove HOSTS entries")) {
        $svcIds | ForEach-Object {         
            Clear-ContainerHostName -ContainerNameOrId $_ -Verbose
            write-verbose "Cleared $_ from HOSTS"
        }
    }    
    
    write-verbose "Stopping services $svcIds"

    
    if ($PSCmdlet.ShouldProcess("docker-compose down")) {
        write-verbose "Calling docker-compose down"
        invoke-expression "docker-compose -p $EnvironmentName down"
    }
    write-verbose "Stopped services."
}