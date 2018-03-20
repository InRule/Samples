param(
    [string]$tag,
    [string]$registryRootRepos = "server",
    [switch]$setLatestTag = $false,
    [string]$defaultInRuleInstallFolder = "C:\Program Files (x86)\InRule\"
)
Push-Location $PSScriptRoot

$version = $null
if ($tag -ne "latest") {    
    $version = $tag
}
$ErrorActionPreference = "Stop"
write-host "Building inrule-server base image."
set-location "$PSScriptRoot\inrule-server" 
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-server:latest .

$hostCatalogPath = resolve-path "$defaultInRuleInstallFolder\irServer\RepositoryService\IisService\"
Write-Host "Building inrule-catalog image using assets at $hostCatalogPath"
Copy-Item $hostCatalogPath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-catalog\irCatalog\" -Verbose
set-location "$PSScriptRoot\inrule-catalog"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog:$tag .

$hostRuntimePath = resolve-path $defaultInRuleInstallFolder\irServer\RuleEngineService\IisService
write-host "Building inrule-runtime image."
Copy-Item -Path $hostRuntimePath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-runtime\irServer" -Verbose
set-location "$PSScriptRoot\inrule-runtime"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-runtime:$tag .

$hostCatManPath = resolve-path $defaultInRuleInstallFolder\irServer\CatalogManagerWeb
write-host "Building inrule-catalog-manager image."
Copy-Item -Path $hostCatManPath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-catalog-manager\CatalogManagerWeb" -Verbose
set-location "$PSScriptRoot\inrule-catalog-manager"
docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog-manager:$tag .

if ($setLatestTag -eq $true ) {
    write-host "Setting image $version to latest..."
    docker image tag ${registryRootRepos}/inrule-catalog:$tag ${registryRootRepos}/inrule-catalog:latest

    docker image tag ${registryRootRepos}/inrule-runtime:$tag ${registryRootRepos}/inrule-runtime:latest

    docker image tag ${registryRootRepos}/inrule-catalog-manager:$tag ${registryRootRepos}/inrule-catalog-manager:latest
}
Pop-Location

