param(
    [string]$tag,
    [string]$registryRootRepos = "server",
    [string]$baseBinariesPath = ".",
    [switch]$setLatestTag = $false
)
Push-Location $PSScriptRoot

$version = $null
if ($tag -ne "latest") {    
    $version = $tag
}
$ErrorActionPreference = "Stop"
write-host "Building inrule-server base image."
set-location "$PSScriptRoot\inrule-server" 
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-server:$tag .

if ($setLatestTag -eq $true ) {
    write-host "Setting image $version to latest..."
    docker image tag ${registryRootRepos}/inrule-server:$tag ${registryRootRepos}/inrule-server:latest
}

Write-Host "Building inrule-catalog image."
set-location "$PSScriptRoot\inrule-catalog"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog:$tag .

write-host "Building inrule-runtime image."
set-location "$PSScriptRoot\inrule-runtime"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-runtime:$tag .

write-host "Building inrule-catalog-manager image."
set-location "$PSScriptRoot\inrule-catalog-manager"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog-manager:$tag .

if ($setLatestTag -eq $true ) {
    write-host "Setting image $version to latest..."
    docker image tag ${registryRootRepos}/inrule-catalog:$tag ${registryRootRepos}/inrule-catalog:latest

    docker image tag ${registryRootRepos}/inrule-runtime:$tag ${registryRootRepos}/inrule-runtime:latest

    docker image tag ${registryRootRepos}/inrule-catalog-manager:$tag ${registryRootRepos}/inrule-catalog-manager:latest
}
Pop-Location

