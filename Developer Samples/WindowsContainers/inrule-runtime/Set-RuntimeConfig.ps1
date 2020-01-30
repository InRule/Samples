[OutputType("System.Int32")]
[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string]$catalogUser = "",
    [string]$catalogPass = "",
    [string]$installPath = "$env:ContainerDir",
    [string]$restRuleApplication = "C:\RuleApps",
    [string]$catalogServiceUri = "${env:inrule:runtime:service:catalog:catalogServiceUri}",
    [string]$endpointAssemblyPath = "EndpointAssemblies",
    [string]$logLevel = "$env:irLogLevel"
)
function Set-RuntimeConfig {    
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

    $cUri = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:runtime:service:catalog:catalogServiceUri' -value $catalogServiceUri
    $cUser = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:runtime:service:catalog:userName' -value $catalogUser
    $cPass = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:runtime:service:catalog:password' -value $catalogPass
    $logLevel = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:logging:level' -value $logLevel
    $catSvc.AppendChild($cUri)
    $catSvc.AppendChild($cUser)
    $catSvc.AppendChild($cPass)
    $catSvc.AppendChild($logLevel)

    $inruleLogSection = $cfgXml.configuration.'inrule.logging'
    
    $inruleLogSection.group.logger.option.SetAttribute("value", "InRule") # ensure logs are sourced to the InRule eventSource

    #$logGroup = $cfgXml.CreateElement("group") # add an additional logging group for the console logger
    #$logGroup.SetAttribute("typeName", "InRule.Repository.Logging.Loggers.LoggerGroup, InRule.Repository")
    # controlled by global appsetting but still needed to properly work
    #$logGroup.SetAttribute("level", $logLevel)
    #$inruleLogSection.AppendChild($logGroup)

    #$conLogger = $cfgXml.CreateElement("logger")
    #$conLogger.SetAttribute("typeName", "InRule.Repository.Logging.Loggers.ConsoleLogger, InRule.Repository")
    #$logGroup.AppendChild($conLogger)

    $cfgXml.configuration.'inrule.runtime.service'.restRuleApplication.SetAttribute("path", $restRuleApplication)
    $cfgXml.Save($configFilePath)

    write-output "Saved configuration changes."
    return 0;   
}
Set-RuntimeConfig $PSBoundParameters