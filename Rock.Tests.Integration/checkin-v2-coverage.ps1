# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

coverlet bin\Debug\Rock.Tests.Integration.dll --target dotnet --targetargs "vstest bin\Debug\Rock.Tests.Integration.dll /Tests:Rock.Tests.Integration.Modules.CheckIn.v2" --include "[Rock]Rock.CheckIn.v2.*" --skipautoprops --format opencover --output bin\Debug\opencover.checkin.xml
reportgenerator -reports:bin\Debug\opencover.checkin.xml -targetdir:.\bin\Debug\coverage -sourcedirs:..

# Invoke-Expression bin\Debug\coverage\index.html
