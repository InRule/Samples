[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string]$tag,
    [string]$registryRootRepos = "inrule",
    [switch]$setLatestTag = $false,
    [switch]$skipServerBuild = $false,
    [string]$defaultInRuleInstallFolder = "C:\Program Files (x86)\InRule\"
)
Push-Location $PSScriptRoot

$version = $null
Write-Host "Attempting to determine the version number for assets located at $defaultInRuleInstallFolder"
$version = (Get-ChildItem $defaultInRuleInstallFolder\*\InRule.Common.dll -recurse)[0].VersionInfo.ProductVersion
Write-Host "Using InRule version $version"

if (([String]::IsNullOrWhiteSpace($tag) -and ($null -ne $version))) {
    Write-Host "No value supplied for image tag. Using $version for tagging images."
    $tag = $version
}
Write-Host "Images will be tagged $tag. Also tag them as latest? $setLatestTag"

$ErrorActionPreference = "Stop"
if ($skipServerBuild -eq $false) { 
    write-host "Building inrule-server base image."
    set-location "$PSScriptRoot\inrule-server" 
    if ($PSCmdlet.ShouldProcess("Building inrule-server base image")) {
        docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-server:$tag . 
    }
    if ($setLatestTag -eq $true -and $PSCmdlet.ShouldProcess("Setting image $version to latest...")) {
        docker image tag ${registryRootRepos}/inrule-server:$tag ${registryRootRepos}/inrule-server:latest    
    }
}

$hostCatalogPath = resolve-path "$defaultInRuleInstallFolder\irServer\RepositoryService\IisService\"
set-location "$PSScriptRoot\inrule-catalog"
if ($PSCmdlet.ShouldProcess("Building inrule-catalog image using assets at $hostCatalogPath")) {
    Copy-Item $hostCatalogPath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-catalog\irCatalog\"
    docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog:$tag .
}

$hostRuntimePath = resolve-path $defaultInRuleInstallFolder\irServer\RuleEngineService\IisService
set-location "$PSScriptRoot\inrule-runtime"
if ($PSCmdlet.ShouldProcess("Building inrule-runtime image using assets at $hostRuntimePath")) {
    Copy-Item -Path $hostRuntimePath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-runtime\irServer"
    docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-runtime:$tag .
}

$hostCatManPath = resolve-path $defaultInRuleInstallFolder\irServer\CatalogManagerWeb
set-location "$PSScriptRoot\inrule-catalog-manager"
if ($PSCmdlet.ShouldProcess("Building inrule-catalog-manager image using assets at $hostCatManPath")) {
    Copy-Item -Path $hostCatManPath\** -Recurse -Force -Destination "$PSScriptRoot\inrule-catalog-manager\CatalogManagerWeb"
    docker build --label "com.inrule.version=$version" -t ${registryRootRepos}/inrule-catalog-manager:$tag .
}

if ($setLatestTag -eq $true -and $PSCmdlet.ShouldProcess("Setting image $tag to latest...") ) {
    docker image tag ${registryRootRepos}/inrule-catalog:$tag ${registryRootRepos}/inrule-catalog:latest

    docker image tag ${registryRootRepos}/inrule-runtime:$tag ${registryRootRepos}/inrule-runtime:latest

    docker image tag ${registryRootRepos}/inrule-catalog-manager:$tag ${registryRootRepos}/inrule-catalog-manager:latest
}
Pop-Location

