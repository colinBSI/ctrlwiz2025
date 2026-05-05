$msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'
$sln = 'C:\Users\cemch-wei\Documents\GitHub\ctrlwiz2025\CtrlWiz.sln'
& $msbuild $sln /p:Configuration="Debug NW25" /p:Platform=x64 /t:CtrlWizNW /m /nologo /verbosity:minimal 2>&1
