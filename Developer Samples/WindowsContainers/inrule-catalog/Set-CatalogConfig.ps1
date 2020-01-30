[OutputType("System.Int32")]
[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string]$connectionString = $env:inrule:repository:service:connectionString,
    [string]$installPath = "$env:ContainerDir",
    [string]$logLevel = "$env:irLogLevel"    
)
function Set-CatalogConfig {    
    function New-AppSettingsEntry {
        param([XML]$parentDoc, [string]$key, [string]$value)
    
        $appSetting = $parentDoc.CreateElement("add")
        $appSetting.SetAttribute("key", $key)
        $appSetting.SetAttribute("value", $value)
        return $appSetting
    }

    write-output "Replacing supplied configuration values in web.config..."
    $configFilePath = resolve-Path -Path (Join-Path -Path $installPath -ChildPath "Web.config")

    if ((test-path $installPath) -eq $false) {
        throw "cannot find path to service configuration file.";
        return -1;
    }

    $cfgXml = new-object Xml
    $cfgXml.Load($configFilePath)
    $catSvc = $cfgXml.configuration["appSettings"]
    if ($null -eq $catSvc) {
         
        $catSvc = $cfgXml.CreateElement("appSettings")        
        $cfgXml.configuration.appendChild($catSvc)
        write-verbose "Added appSettings node to 'inrule.repository.service'"
    }

    $cs = New-AppSettingsEntry -parentDoc $cfgXml -key "inrule:repository:service:connectionString" -value $connectionString
    $logLevel = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:logging:level' -value $logLevel

    $catSvc.AppendChild($cs)
    $catSvc.AppendChild($logLevel)
   

    $inruleLogSection = $cfgXml.configuration.'inrule.logging'
    
    $inruleLogSection.group.logger.option.SetAttribute("value", "InRule") # ensure logs are sourced to the InRule eventSource

    $cfgXml.Save($configFilePath)
    write-output "Saved configuration changes."
    return 0;   
}
Set-CatalogConfig $PSBoundParameters