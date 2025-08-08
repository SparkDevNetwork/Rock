# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

param (
    [string]$tests = "Rock.Tests",
    [string]$include = "Rock"
)

coverlet bin\Debug\net472\Rock.Tests.dll `
    --target dotnet `
    --targetargs "vstest bin\Debug\net472\Rock.Tests.dll /Tests:$tests"`
    --include "[Rock]$include.*" `
    --exclude "[Rock]Rock.Model.*" `
    --exclude "[Rock]Rock.Web.UI.*" `
    --skipautoprops `
    --format opencover `
    --exclude-assemblies-without-sources=none `
    --output bin\Debug\opencover.checkin.xml

reportgenerator -reports:bin\Debug\net472\opencover.checkin.xml `
    -targetdir:.\bin\Debug\net472\coverage `
    -sourcedirs:..

# Invoke-Expression bin\Debug\net472\coverage\index.html
