[CmdletBinding()]
param(
    [string]$catalogDbHostName = "localhost",
    [string]$catalogUserName,
    [string]$catalogPassword= "_", # TODO: use SecureString
    [string]$catalogInstallPath = "C:\Program Files (x86)\InRule\irServer\RepositoryService\IisService",
    [string]$catalogDbName = "InRuleCatalog"
    )
    
$ErrorActionPreference = 'Stop'

if ($catalogPassword -eq "_" -or $catalogPassword -eq $null) {
    Write-Verbose 'Invalid DB connection string information supplied to setup script. Ensure the catalogUserName and catalogPassword parameters are suppleid. NOTE: Integration Windows Authentication is not currently supported by this script.'
    return -1
}

if ((test-path $catalogInstallPath) -eq $false) {
    Write-Verbose "cannot find path to irCatalog configuration file.";
    return -1;
}
$catConString = "Server=tcp:$catalogDbHostName,1433;User ID=$catalogUserName;Password=$catalogPassword;Initial Catalog=$catalogDbName;"
write-Verbose "Replacing supplied configuration values in web.config..."
$configFilePath = resolve-Path -Path (Join-Path -Path $catalogInstallPath -ChildPath "Web.config")

$cfgXml = new-object Xml
$cfgXml.Load($configFilePath)
$cstring = $cfgXml.configuration.'inrule.repository.service'.connectionString
if ($cstring -eq $null) {
    write-error "Could not locate configuration section inrule.repository.service.connectionString. irCatalog is unlikely to function in this state."
    $cstring = $cfgXml.CreateElement("connectionString")
    
    $cfgXml.configuration.'inrule.repository.service'.appendChild($cstring)
    write-verbose "Added connectionString node to 'inrule.repository.service'"
}
write-verbose "Replacing constring $cstring with $catConString"

$cfgXml.configuration.'inrule.repository.service'.connectionString = $catConString
if ($PSCmdlet.ShouldProcess("Save configuration string changes to config")) {
    $cfgXml.Save($configFilePath)
}
write-Verbose "Saved configuration changes."
return 0;