# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

param (
    [string]$tests = "Rock.Tests.Integration",
    [string]$include = "Rock"
)

coverlet bin\Debug\Rock.Tests.Integration.dll `
    --target dotnet `
    --targetargs "vstest bin\Debug\Rock.Tests.Integration.dll /Tests:$tests"`
    --include "[Rock]$include.*" `
    --exclude "[Rock]Rock.Model.*" `
    --exclude "[Rock]Rock.Web.UI.*" `
    --skipautoprops `
    --format opencover `
    --output bin\Debug\opencover.xml

reportgenerator -reports:bin\Debug\opencover.xml `
    -targetdir:.\bin\Debug\coverage `
    -sourcedirs:..

# Invoke-Expression bin\Debug\coverage\index.html