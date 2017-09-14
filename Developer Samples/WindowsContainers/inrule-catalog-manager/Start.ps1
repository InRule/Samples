$last = Get-Date
while ($true) {
    start-sleep -seconds 2;
    Get-WinEvent -FilterHashtable @{ StartTime = $last; } -ErrorAction SilentlyContinue | format-list
    $last = Get-Date
}