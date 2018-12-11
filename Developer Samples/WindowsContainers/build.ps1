[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string][Parameter(Mandatory= $true)] $tag,
    [string]$registryRootRepos = "inrule",
    [switch]$setLatestTag = $false,
    [switch]$skipServerBuild = $false
)
$ErrorActionPreference = "Stop"
Write-Host "Images will be tagged $tag. Also tag them as latest? $setLatestTag"

$ErrorActionPreference = "Stop"
if ($skipServerBuild -eq $false) { 
    write-host "Building inrule-server base image."    
    if ($PSCmdlet.ShouldProcess("Building inrule-server base image")) {
        docker build --label "com.inrule.version=$tag" -t ${registryRootRepos}/inrule-server:$tag  -f "$PWD\inrule-server\DOCKERFILE" "$PWD\inrule-server"

        if ($LASTEXITCODE -ne 0) {
            throw 'non-zero return from docker build. aborting.'
            
        }
    }
    if ($setLatestTag -eq $true -and $PSCmdlet.ShouldProcess("Setting image $tag to latest...")) {
        docker image tag ${registryRootRepos}/inrule-server:$tag ${registryRootRepos}/inrule-server:latest    
    }
}

if ($PSCmdlet.ShouldProcess("Building inrule-catalog image")) {
   
    docker build --build-arg reposTag=$tag --label "com.inrule.version=$tag" -t ${registryRootRepos}/inrule-catalog:$tag -f $PWD\inrule-catalog\DOCKERFILE $PWD\inrule-catalog

    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
        
    }
}

if ($PSCmdlet.ShouldProcess("Building inrule-runtime image")) {   
    docker build --build-arg reposTag=$tag --label "com.inrule.version=$tag" -t ${registryRootRepos}/inrule-runtime:$tag -f $PWD\inrule-runtime\DOCKERFILE $PWD\inrule-runtime

    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
        
    }
}

if ($PSCmdlet.ShouldProcess("Building inrule-catalog-manager image")) {    
    docker build --build-arg reposTag=$tag --label "com.inrule.version=$tag" -t ${registryRootRepos}/inrule-catalog-manager:$tag -f $PWD\inrule-catalog-manager\DOCKERFILE $PWD\inrule-catalog-manager
    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
        
    }
}

if ($setLatestTag -and $PSCmdlet.ShouldProcess("Setting image $tag to latest...") ) {
    docker image tag ${registryRootRepos}/inrule-catalog:$tag ${registryRootRepos}/inrule-catalog:latest

    docker image tag ${registryRootRepos}/inrule-runtime:$tag ${registryRootRepos}/inrule-runtime:latest

    docker image tag ${registryRootRepos}/inrule-catalog-manager:$tag ${registryRootRepos}/inrule-catalog-manager:latest
}
Pop-Location

