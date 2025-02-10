@echo off

IF %1.==. goto :Usage

set MOD_NAME="Trashbin"
set VERSION=%1
set SR_TRASHBIN_INPUTDIR=".\bin\Release\net6.0\publish"

echo "Building release..."
python.exe SRModCore\build.py --clean --tag -n "%MOD_NAME%" -c Release -i %SR_TRASHBIN_INPUTDIR% %VERSION% build_files.txt || goto :ERROR

echo "Done"
goto :EOF

:ERROR
echo "Error occurred in build script! Error code: %errorlevel%"

:Usage
echo "Usage: ./build_release.bat <version>"
