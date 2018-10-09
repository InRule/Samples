[OutputType("System.Int32")]
[CmdletBinding(SupportsShouldProcess = $true)]    
param(
    [string]$catalogUri = $env:InRule:Catalog:Uri,
    [string]$installPath = "$env:catManDir"    
)
function Set-CatalogManagerConfig {    
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

    $cs = New-AppSettingsEntry -parentDoc $cfgXml -key "InRule.Catalog.Uri" -value $catalogUri
    
    $catSvc.AppendChild($cs)    
    $cfgXml.Save($configFilePath)


    write-output "Saved configuration changes."
    return 0;   
}
Set-CatalogManagerConfig $PSBoundParameters