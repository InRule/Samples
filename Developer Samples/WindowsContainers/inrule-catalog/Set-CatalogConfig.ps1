[OutputType("System.Int32")]
[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string]$connectionString = $env:inrule:repository:service:connectionString,
    [string]$installPath = "$env:ContainerDir",
    [string]$logLevel = "$env:irLogLevel",
    [string]$wcfCfgFilePath = "$env:ContainerDir\\configfragments.xml"    
)
function Set-CatalogConfig {    
    function New-AppSettingsEntry {
        param([XML]$parentDoc, [string]$key, [string]$value)
    
        $appSetting = $parentDoc.CreateElement("add")
        $appSetting.SetAttribute("key", $key)
        $appSetting.SetAttribute("value", $value)
        return $appSetting
    }

    function Configure-SslCert {    
        $pwSecString = ConvertTo-SecureString -String $env:PfxPassword -AsPlainText -Force
        $certPath = (gci $installPath\*.pfx -recurse)[0]
        $cert = Import-PfxCertificate -FilePath $certPath.FullName -Password $pwSecString -CertStoreLocation cert:\LocalMachine\My
        $certHashString = $cert.GetCertHashString()
        Write-Verbose "Imported PFX $certHashString to 'cert:\LocalMachine\My'"
        (Get-WebBinding -Protocol "https").AddSslCertificate($certHashString, "My")
        Write-Verbose "Added SSL certificate with thumbprint $certHashString to HTTPS binding"
    }

    function Replace-WcfBindings {
        param([XML]$cfgXml)
        $newSvcModelCfg = [xml]$(get-content $wcfCfgFilePath)
        $cfgXml.configuration.RemoveChild($cfgXml.configuration.'system.serviceModel')
        $newNode = $cfgXml.ImportNode($newSvcModelCfg.'system.serviceModel', $true)
        $cfgXml.configuration.AppendChild($newNode)
        
    }

    write-output "Replacing supplied configuration values in web.config..."
    $configFilePath = resolve-Path -Path (Join-Path -Path $installPath -ChildPath "Web.config")

    if ((test-path $installPath) -eq $false) {
        throw "cannot find path to service configuration file.";
        return -1;
    }

    $cfgXml = new-object Xml
    $cfgXml.Load($configFilePath)
    $cfgXml.configuration.'system.web'.compilation.setAttribute("targetFramework", "4.8")
    $cfgXml.configuration.'system.web'.httpRuntime.setAttribute("targetFramework", "4.8")

    $catSvc = $cfgXml.configuration["appSettings"]
    if ($null -eq $catSvc) {
         
        $catSvc = $cfgXml.CreateElement("appSettings")        
        $cfgXml.configuration.appendChild($catSvc)
        write-verbose "Added appSettings node to 'inrule.repository.service'"
    }
    
    #Install-NugetAndPackages $cfgXml
    if ((gci $installPath\*.pfx -recurse).Count -gt 0) {
        Configure-SslCert
    }
    #
    Replace-WcfBindings $cfgXml
    

    $cs = New-AppSettingsEntry -parentDoc $cfgXml -key "inrule:repository:service:connectionString" -value $connectionString
    $logLevel = New-AppSettingsEntry -parentDoc $cfgXml -key 'inrule:logging:level' -value $logLevel

    $catSvc.AppendChild($cs)
    $catSvc.AppendChild($logLevel)
   

    $inruleLogSection = $cfgXml.configuration.'inrule.logging'    
    $inruleLogSection.group.logger.option.SetAttribute("value", "InRule") # ensure logs are sourced to the InRule eventSource

    $logGroup = $cfgXml.CreateElement("group") # add an additional logging group for the console logger
    $logGroup.SetAttribute("typeName", "InRule.Repository.Logging.Loggers.LoggerGroup, InRule.Repository")
    #controlled by global appsetting but still needed to properly work
    $logGroup.SetAttribute("level", $logLevel)
    $inruleLogSection.AppendChild($logGroup)

    $conLogger = $cfgXml.CreateElement("logger")
    $conLogger.SetAttribute("typeName", "InRule.Repository.Logging.Loggers.ConsoleLogger, InRule.Repository")
    $logGroup.AppendChild($conLogger)

    $cfgXml.Save($configFilePath)
    write-output "Saved configuration changes."
    return 0;   
}
Set-CatalogConfig $PSBoundParameters