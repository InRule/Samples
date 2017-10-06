
[CmdletBinding()]
param (
    [string]$catalogUri,
    [string]$cfgBasePath = 'c:\inetpub\wwwroot\'
)

$cfgPath = resolve-path $cfgBasePath\web.config
write-host "resolved $cfgPath"
$cfg = [xml](gc $cfgPath);

$cfg.configuration.appSettings.SelectSingleNode("add[@key='InRule.Catalog.Uri']").value = "$catalogUri";

$cfg.Save($cfgPath);
write-host "Updated $cfgPath appsetting to point at catalog $catalogUri"