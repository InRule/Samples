[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string][Parameter(Mandatory = $true)] $inruleReleaseTag,
    [string[]][Parameter(Mandatory = $false)] $imageTags = @(""),    
    [string]$registry = "inrule"
)
$ErrorActionPreference = "Stop"

$baseImageTags = @()
$catalogTags = @()
$runtimeTags = @()
$catManTags = @()

function Invoke-ContainerBuild {
    
    
    function buildServerImage {
        param($folder)
        
        try {
            Push-Location $folder
            $tag = "$registry/inrule-server:" + $folder.Name
            $cmd = "build --rm --no-cache -t $tag -f `"$folder\DOCKERFILE`" `"$folder`""
            write-verbose $cmd
            if ($PSCmdlet.ShouldProcess("Building Base image in $folder")) {
                start-process "docker.exe" -ArgumentList $cmd  -NoNewWindow -Wait -Verbose 
            }
            write-verbose "Built $tag"
        }
        finally {
            Pop-Location      
        
        }
        return $tag
    }

    function buildCatalogImage {
        param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-catalog:"
        
        
        $imageTags | foreach {
            $t = ""
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t = $_ + "-$inruleReleaseTag-$baseImageTag"
            }
            else {
                $t = $_ + "$inruleReleaseTag-$baseImageTag"
            }
            
            $catalogTags += ("-t $tagTmpl" + $t) 
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag $catalogTags --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        write-verbose $cmd

        if ($PSCmdlet.ShouldProcess("Building catalog image for $baseImageTag")) {
             start-process "docker.exe" -ArgumentList $cmd  -NoNewWindow -Wait -Verbose 
        }
        write-verbose "Built $catalogTags"
        
        
    }   

    function buildRuntimeImage {
         param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-runtime:"
        
        
        $imageTags | foreach {
            $t = ""
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t = $_ + "-$inruleReleaseTag-$baseImageTag"
            }
            else {
                $t = $_ + "$inruleReleaseTag-$baseImageTag"
            }
            
            $runtimeTags += ("-t $tagTmpl" + $t) 
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag $runtimeTags --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        write-verbose $cmd

        if ($PSCmdlet.ShouldProcess("Building catalog image for $baseImageTag")) {
             start-process "docker.exe" -ArgumentList $cmd  -NoNewWindow -Wait -Verbose 
        }
        write-verbose "Built $runtimeTags"       
        
    }

    
    function buildCatManImage {
         param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-catalog-manager:"
        
        
        $imageTags | foreach {
            $t = ""
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t = $_ + "-$inruleReleaseTag-$baseImageTag"
            }
            else {
                $t = $_ + "$inruleReleaseTag-$baseImageTag"
            }
            
            $catManTags += ("-t $tagTmpl" + $t) 
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag $catManTags --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        write-verbose $cmd

        if ($PSCmdlet.ShouldProcess("Building catalog manager image for $baseImageTag")) {
             start-process "docker.exe" -ArgumentList $cmd  -NoNewWindow -Wait -Verbose 
        }
        write-verbose "Built $catManTags"       
        
    }

    # we assume that all subfolders of the ..\WindowsContainers\inrule-server\**\* are build dirs
    Get-ChildItem -Path .\inrule-server\ -Directory -Recurse | foreach {
        $baseTag = buildServerImage $_        
        $baseImageTags += $_.Name
    }

    Write-Verbose "Base image tags: $baseImageTags"
    try {
        Push-Location .\inrule-catalog\
        $baseImageTags | foreach {
            buildCatalogImage $_
            buildRuntimeImage $_
            buildCatManImage $_
        }
    }
    finally {
        Pop-Location
    }

    
}


Invoke-ContainerBuild $PSBoundParameters