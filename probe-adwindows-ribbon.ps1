$dll = 'C:\Program Files\Autodesk\Navisworks Manage 2025\AdWindows.dll'
if (-not (Test-Path $dll)) {
    $dll = 'C:\Program Files\Autodesk\Navisworks Manage 2022\AdWindows.dll'
}
$asm = [System.Reflection.Assembly]::LoadFile($dll)
Write-Host "Loaded: $dll"
$types = $asm.GetTypes()
$btn = $types | Where-Object { $_.Name -eq 'RibbonButton' }
if ($btn) {
    Write-Host "Found: $($btn.FullName)"
    Write-Host "=== RibbonButton properties ==="
    $btn.GetProperties() | Select-Object -ExpandProperty Name | Sort-Object
} else {
    Write-Host "RibbonButton not found"
    Write-Host "=== Ribbon* types in AdWindows ==="
    $types | Where-Object { $_.Name -like 'Ribbon*' } | Select-Object -ExpandProperty FullName | Sort-Object
}
