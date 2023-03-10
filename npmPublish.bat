xcopy .\README.md .\Assets\_PackageRoot\README.md /y
xcopy .\README.md .\Assets\_PackageRoot\Documentation~\README.md /y
cd Assets\_PackageRoot
npm publish
pause