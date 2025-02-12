@echo off

set MOD_NAME="Trashbin"
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"
set SR_TRASHBIN_INPUTDIR=".\bin\Debug\net6.0\publish"

echo "Building dev configuration"
python.exe SRModCore\build.py --clean -n "%MOD_NAME%" -c Debug -i %SR_TRASHBIN_INPUTDIR% -o build\localdev localdev build_files.txt || goto :ERROR

echo "Copying to SR directory..."
@REM Building spits out raw file structure in build/localdev/raw
copy build\localdev\Mods\* %SYNTHRIDERS_MODS_DIR% || goto :ERROR

echo "Done"
goto :EOF

:ERROR
echo "Error occurred in build script! Error code: %errorlevel%"
