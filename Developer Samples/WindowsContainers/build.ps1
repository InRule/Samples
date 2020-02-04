[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string][Parameter(Mandatory= $true)] $tag,
    [string]$registryRootRepos = "inrule",
    [switch]$setLatestTag = $false,
    [switch]$skipServerBuild = $false,
    [string][Parameter(Mandatory= $true)] $reposTag,
    [switch]$requiresHyperVIsolation = $false,
    [string]$baseServerTagName = "latest"
)
$ErrorActionPreference = "Stop"
Write-Verbose "Images will be tagged $tag. Assets will be pulled from $reposTag. Also tag them as latest? $setLatestTag"

$hyperVIsolationParameter = ""
if ($requiresHyperVIsolation -eq $true) {
    $hyperVIsolationParameter = "--isolation=hyperv"
}

$imagesBuild = @()

if ($skipServerBuild -eq $false) { 
    write-host "Building inrule-server base image."    
    if ($PSCmdlet.ShouldProcess("Building inrule-server base image")) {
        docker build $hyperVIsolationParameter -t ${registryRootRepos}/inrule-server:$baseServerTagName  -f "$PWD\inrule-server\DOCKERFILE" "$PWD\inrule-server"
        
        if ($LASTEXITCODE -ne 0) {
            throw 'non-zero return from docker build. aborting.'
            
        }
        $imagesBuild += "${registryRootRepos}/inrule-server:${baseServerTagName}"
        if ($setLatestTag -eq $true -and $baseServerTagName -ne "latest") {
            docker image tag ${registryRootRepos}/inrule-server:$tag ${registryRootRepos}/inrule-server:latest
            $imagesBuild += "${registryRootRepos}/inrule-server:latest"    
        }
        
    }    
}

$baseImageBuildArg = "baseImageTag=$baseServerTagName"
$reposTagBuildArg = "reposTag=$reposTag"
$baseImageBuildArg
$reposTagBuildArg
$versionLabelArg

if ($PSCmdlet.ShouldProcess("Building inrule-catalog-db image")) {
   
    docker build $hyperVIsolationParameter --build-arg $reposTagBuildArg --build-arg $baseImageBuildArg -t ${registryRootRepos}/inrule-catalog-db:$tag -f $PWD\inrule-catalog-db\DOCKERFILE $PWD\inrule-catalog-db
    
    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
    }
    $imagesBuild += "${registryRootRepos}/inrule-catalog-db:${tag}"
}

if ($PSCmdlet.ShouldProcess("Building inrule-catalog image")) {
   
    docker build $hyperVIsolationParameter --build-arg $reposTagBuildArg --build-arg $baseImageBuildArg -t ${registryRootRepos}/inrule-catalog:$tag -f $PWD\inrule-catalog\DOCKERFILE $PWD\inrule-catalog
    
    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
    }
    $imagesBuild += "${registryRootRepos}/inrule-catalog:${tag}"
}

if ($PSCmdlet.ShouldProcess("Building inrule-runtime image")) {   
    docker build $hyperVIsolationParameter --build-arg $reposTagBuildArg --build-arg $baseImageBuildArg -t ${registryRootRepos}/inrule-runtime:$tag -f $PWD\inrule-runtime\DOCKERFILE $PWD\inrule-runtime

    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
    }
    $imagesBuild += "${registryRootRepos}/inrule-runtime:${tag}"
}

if ($PSCmdlet.ShouldProcess("Building inrule-catalog-manager image")) {    
    docker build $hyperVIsolationParameter --build-arg $reposTagBuildArg --build-arg $baseImageBuildArg -t ${registryRootRepos}/inrule-catalog-manager:$tag -f $PWD\inrule-catalog-manager\DOCKERFILE $PWD\inrule-catalog-manager
    if ($LASTEXITCODE -ne 0) {
        throw 'non-zero return from docker build. aborting.'
    }
    $imagesBuild += "${registryRootRepos}/inrule-catalog-manager:${tag}"
}

if ($setLatestTag -and $PSCmdlet.ShouldProcess("Setting image $tag to latest...") ) {
    docker image tag ${registryRootRepos}/inrule-catalog:$tag ${registryRootRepos}/inrule-catalog:latest
    $imagesBuild += "${registryRootRepos}/inrule-catalog:latest"
    docker image tag ${registryRootRepos}/inrule-runtime:$tag ${registryRootRepos}/inrule-runtime:latest
    $imagesBuild += "${registryRootRepos}/inrule-runtime:latest"
    docker image tag ${registryRootRepos}/inrule-catalog-manager:$tag ${registryRootRepos}/inrule-catalog-manager:latest
    $imagesBuild += "${registryRootRepos}/inrule-catalog-manager:latest"
}
if ($PSCmdlet.ShouldProcess("Write file with images and tags")) {
    $imagesBuild | set-content -Path "$PWD\docker-build" -Force
}

Pop-Location