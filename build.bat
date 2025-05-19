@echo off
setlocal

dotnet build CS2-CSkins-QR.csproj

set "source_dir=F:\cs2dev\CS2_dev_CSSharp_dev\0_My_plugins\CS2-CSkins-QR\bin\Debug\net8.0\"
set "target_dir=F:\csgoserver_win\cs2\game\csgo\addons\counterstrikesharp\plugins\CS2-CSkins-QR\"

set "files=CS2-CSkins-QR.dll CS2-CSkins-QR.pdb CS2-CSkins-QR.deps.json"

for %%f in (%files%) do (
    if exist "%source_dir%%%f" (
        echo Copying %%~nxf...
        copy /Y "%source_dir%%%f" "%target_dir%"
    ) else (
        echo Error: File not found - "%source_dir%%%f"
    )
)

echo All files copied.
@REM pause