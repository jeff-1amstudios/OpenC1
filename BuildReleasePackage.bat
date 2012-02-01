rmdir /S /Q ReleasePackage
mkdir ReleasePackage
xcopy /S OpenC1\bin\x86\Release\* ReleasePackage
copy Engine\Lib\Microsoft.DirectX.* ReleasePackage
cd ReleasePackage
mkdir tmp
c:\progra~2\Microsoft\ilmerge\ilmerge.exe /out:tmp\OpenC1.exe OpenC1.exe 1amstudiosEngine.dll Ionic.Zip.Reduced.dll
del OpenC1.*
del 1amstudiosEngine.*
del Ionic.Zip.Reduced.dll
copy tmp\* .
rmdir /S /Q tmp
rmdir /S /Q Lib
copy ..\license.txt .
copy ..\readme.txt .
pause