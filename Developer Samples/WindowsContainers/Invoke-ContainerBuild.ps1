[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string][Parameter(Mandatory = $true)] $inruleReleaseTag,
    [string[]][Parameter(Mandatory = $false)] $imageTags = @(""),    
    [string]$registry = "inrule",
    [string[]]$windowsServiceReleasesToUse = @("ltsc2016", "ltsc2019")
)
$ErrorActionPreference = "Stop"

$baseImageTags = @()
$catalogTags = @()
$runtimeTags = @()
$catManTags = @()

function Invoke-ContainerBuild {
    
    
    function buildServerImage {
        param($folder)
        if ($null -eq $folder) {
            return $null
        }
        try {
            Push-Location $folder
            $tag = "$registry/inrule-server:" + $folder.Name
            $cmd = "build --rm --no-cache -t $tag -f `"$folder\DOCKERFILE`" `"$folder`""
            runDockerBuild $cmd

            write-verbose "Built $tag"
        }
        finally {
            Pop-Location      
        
        }
        return $tag
    }

    function buildCatalogImage {
        param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-catalog:" + "$inruleReleaseTag-$baseImageTag"
        
        
        $imageTags | foreach {
            $t = $tagTmpl
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t += "-$_"
            }
                        
            $script:catalogTags += $t
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag -t $($catalogTags -join ' -t ') --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        runDockerBuild $cmd
        write-verbose "Built $catalogTags"
        
        
    }   

    function buildRuntimeImage {
        param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-runtime:" + "$inruleReleaseTag-$baseImageTag"        
        
        $imageTags | foreach {
            $t = $tagTmpl 
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t += "-$_"
            }            
            $script:runtimeTags += $t
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag -t $($runtimeTags -join ' -t ') --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        write-verbose $cmd

        runDockerBuild $cmd
        write-verbose "Built $runtimeTags"       
        
    }

    
    function buildCatManImage {
        param([string]$baseImageTag)
        $tagTmpl = "$registry/inrule-catalog-manager:" + "$inruleReleaseTag-$baseImageTag"
        
        $imageTags | foreach {
            $t = $tagTmpl
            if ([string]::IsNullOrWhiteSpace($_) -eq $false) {
                $t += "-$_"
            }
            $script:catManTags += $t
        }
        $cmd = "build --rm --no-cache --label com.inrule.version=$inruleReleaseTag --label com.inrule.windowsrelease=$baseImageTag -t $($catManTags -join ' -t ') --build-arg imageRepos=$registry --build-arg baseImageTag=$baseImageTag --build-arg reposTag=$inruleReleaseTag -f .\DOCKERFILE ."

        runDockerBuild $cmd
        write-verbose "Built $catManTags"       
        
    }

    function runDockerBuild {
        param([string]$command)
        write-verbose $cmd
       
        if ($PSCmdlet.ShouldProcess($cmd) -and ($null -ne $cmd)) {
            start-process "docker.exe" -ArgumentList $cmd  -NoNewWindow -Wait -Verbose
            if ($LASTEXITCODE -ne 0) {
                throw $Error[$Error.Count - 1]
            }
        }
    }

    # we assume that all subfolders of the ..\WindowsContainers\inrule-server\**\* are build dirs
    Get-ChildItem -Path .\inrule-server\ -Directory -Include $windowsServiceReleasesToUse -Recurse | foreach {
        $baseTag = buildServerImage $_
        if ($null -ne $baseTag) {
            $script:baseImageTags += $baseTag
        }                
    }

    Write-Verbose "Base image tags: $baseImageTags"
    try {
        
        $baseImageTags | foreach {
            Push-Location .\inrule-catalog\
            buildCatalogImage $_
            Pop-Location
            Push-Location .\inrule-runtime\
            buildRuntimeImage $_
            Pop-Location
            Push-Location .\inrule-catalog-manager\
            buildCatManImage $_
            Pop-Location

        }
        $allTags = $baseImageTags + $catalogTags + $catManTags + $runtimeTags 
        $allTags | Tee-Object .\tags-built.txt
    }
    finally {
        Pop-Location
    }

    
}


Invoke-ContainerBuild $PSBoundParameters