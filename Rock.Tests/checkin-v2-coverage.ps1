# dotnet tool install -g dotnet-reportgenerator-globaltool
# dotnet tool install -g coverlet.console

coverlet bin\Debug\net472\Rock.Tests.dll `
    --target dotnet `
    --targetargs "vstest bin\Debug\net472\Rock.Tests.dll /Tests:Rock.Tests.CheckIn.v2" `
    --include "[Rock]Rock.CheckIn.v2.*" `
    --skipautoprops `
    --format opencover `
    --exclude-assemblies-without-sources=none `
    --output bin\Debug\net472\opencover.checkin.xml

reportgenerator -reports:bin\Debug\net472\opencover.checkin.xml `
    -targetdir:.\bin\Debug\net472\coverage `
    -sourcedirs:..

# Invoke-Expression bin\Debug\net472\coverage\index.html
