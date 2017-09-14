[OutputType("System.Int32")]
[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string]$catalogUser,
    [string]$catalogPass,
    [string]$installPath = "C:\Program Files (x86)\InRule\irServer\RuleEngineService\IisService\web.config",
    [string]$restRuleApplication = "C:\RuleApps",
    [string]$catalogServiceUri = "http://localhost/InRuleCatalogService/Service.svc",
    [string]$endpointAssemblyPath = "EndpointAssemblies"
)

write-output "Replacing supplied configuration values in web.config..."
$configFilePath = resolve-Path -Path (Join-Path -Path $installPath -ChildPath "Web.config")

if ((test-path $installPath) -eq $false) {
    throw "cannot find path to service configuration file.";
    return -1;
}

$cfgXml = new-object Xml
$cfgXml.LoadXml((Get-Content $configFilePath))
$catSvc = $cfgXml.configuration.'inrule.runtime.service'.catalog

if ($null -eq $catSvc) {
    write-error "Could not locate configuration section inrule.repository.service.connectionString. irCatalog is unlikely to function in this state."
    $catSvc = $cfgXml.CreateElement("catalog")
  
    $cfgXml.configuration.'inrule.runtime.service'.appendChild($catSvc)
    write-verbose "Added connectionString node to 'inrule.repository.service'"
}

$catSvc.SetAttribute("catalogServiceUri", $catalogServiceUri)
$catSvc.SetAttribute("userName", "")
$catSvc.SetAttribute("password", "")

if ($endpointAssemblyPath -ne $null -and $endpointAssemblyPath -ne "") {
    write-verbose "Setting endpointAssemblyPath to $endpointAssemblyPath in config"
    $cfgXml.configuration.'inrule.repository'.endPoints.assemblyEndpoint.SetAttribute("endPointAssemblyPath", $endpointAssemblyPath)
}

if ($PSCmdlet.ShouldProcess("Save configuration string changes to config")) {
    $cfgXml.Save($configFilePath)
}

write-output "Saved configuration changes."

return 0;