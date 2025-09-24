cd ..\Assets\_PackageRoot
@echo off
echo ----------------------------------------------------
echo Executing "npm pkg get version"
FOR /F "tokens=* USEBACKQ" %%F IN (`npm pkg get version`) DO (
SET RawVersion=%%F
)
echo Version of current package is extracted: %RawVersion%
SET CleanVersion=%RawVersion:~1,-1%
echo Current version: %CleanVersion%

git push -u origin HEAD

echo ----------------------------------------------------
cd ..\..\
echo Creating GitHub release with tag=%CleanVersion%
@echo on
gh release create %CleanVersion% --draft --generate-notes --title %CleanVersion%
gh repo view --web
@echo off
echo ----------------------------------------------------

pause
