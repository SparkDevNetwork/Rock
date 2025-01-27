# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

coverlet bin\Debug\Rock.Tests.UnitTests.dll `
    --target dotnet `
    --targetargs "vstest bin\Debug\Rock.Tests.UnitTests.dll" `
    --include "[Rock]Rock.*" `
    --exclude "[Rock]Rock.Model.*" `
    --exclude "[Rock]Rock.Web.UI.*" `
    --skipautoprops `
    --format opencover `
    --output bin\Debug\opencover.checkin.xml

reportgenerator -reports:bin\Debug\opencover.checkin.xml `
    -targetdir:.\bin\Debug\coverage `
    -sourcedirs:..

# Invoke-Expression bin\Debug\coverage\index.html
