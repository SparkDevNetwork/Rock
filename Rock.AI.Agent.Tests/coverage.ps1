# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

param (
    [string]$tests = "Rock.AI.Agent.Tests",
    [string]$include = "Rock.AI.Agent"
)

coverlet bin\Debug\net472\Rock.AI.Agent.Tests.dll `
    --target dotnet `
    --targetargs "vstest bin\Debug\net472\Rock.AI.Agent.Tests.dll /Tests:$tests"`
    --include "[Rock.AI.Agent]$include.*" `
    --include "[Rock]$include.*" `
    --skipautoprops `
    --format opencover `
    --exclude-assemblies-without-sources=none `
    --output bin\Debug\net472\opencover.xml

reportgenerator -reports:bin\Debug\net472\opencover.xml `
    -targetdir:.\bin\Debug\net472\coverage `
    -sourcedirs:..

# Invoke-Expression bin\Debug\net472\coverage\index.html
