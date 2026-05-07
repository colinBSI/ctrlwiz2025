$dll = 'C:\Program Files\Autodesk\Navisworks Manage 2025\navisworks.gui.roamer.dll'
try {
    $asm = [System.Reflection.Assembly]::LoadFile($dll)
    $types = $asm.GetTypes()
    Write-Host "=== navisworks.gui.roamer.dll Ribbon types ==="
    $types | Where-Object { $_.Name -like '*Ribbon*' } | Select-Object -ExpandProperty FullName | Sort-Object
} catch [System.Reflection.ReflectionTypeLoadException] {
    Write-Host "=== navisworks.gui.roamer.dll Ribbon types (partial load) ==="
    $_.Exception.Types | Where-Object { $_ -ne $null -and $_.Name -like '*Ribbon*' } | Select-Object -ExpandProperty FullName | Sort-Object
} catch {
    Write-Host ("ERROR: " + $_.Exception.Message)
}
