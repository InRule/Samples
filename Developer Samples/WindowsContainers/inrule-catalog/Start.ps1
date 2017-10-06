write-host "Container IP address: $(get-netipaddress)"
$last = Get-Date
while ($true) {
    start-sleep -seconds 5;
    (Get-WinEvent -FilterHashtable @{ StartTime = $last; ProviderName = "InRule*" } -ErrorAction SilentlyContinue) | format-list
    $last = Get-Date
}