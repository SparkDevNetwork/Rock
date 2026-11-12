# Points the integration test harness at a Windows runner's LocalDB, in a database the harness may
# delete and rebuild. The harness defaults to a Linux SQL Server container, which a Windows runner
# cannot start. Run from the repository root; both workflows that run the integration tests call it,
# so their database setup cannot drift apart.

$ErrorActionPreference = 'Stop'

$project = 'Rock.Tests.Integration'
$database = 'RockChatIntegration'

# The LocalDB path reads the sample data from a file, not from the assembly as the container path
# does. The reset name matching the connection string's database is what permits the harness to
# replace it, and the creator key must be set before the harness will delete a database it left
# behind, so one failed class would otherwise fail every class after it.
$sample = (Resolve-Path "$project\TestData\SampleData\sampledata_1_14_1.xml").Path
(Get-Content "$project\app.TestSettings.config.example") `
    -replace '"DatabaseHost" value="docker"', '"DatabaseHost" value="localdb"' `
    -replace '"SampleDataUrl" value="embedded"', "`"SampleDataUrl`" value=`"$sample`"" `
    -replace '</appSettings>', "  <add key=`"LocalDbResetDatabaseName`" value=`"$database`" />`n  <add key=`"DatabaseCreatorKey`" value=`"RockIntegrationTestProject`" />`n</appSettings>" |
    Set-Content "$project\app.TestSettings.config"

# The harness asks whether the database exists before it creates it, and that failed login would
# otherwise make the pool block its next attempt, the one just after the create, for seconds.
@"
<connectionStrings>
  <add name="RockContext" connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=$database;Integrated Security=True;MultipleActiveResultSets=True;PoolBlockingPeriod=NeverBlock" providerName="System.Data.SqlClient" />
</connectionStrings>
"@ | Set-Content "$project\app.ConnectionStrings.config"

# Each replace above must have matched, or the run would quietly use the container or the
# embedded sample data and fail somewhere far from here.
$settings = Get-Content "$project\app.TestSettings.config" -Raw
foreach ($expected in '"DatabaseHost" value="localdb"', "`"SampleDataUrl`" value=`"$sample`"", "<add key=`"LocalDbResetDatabaseName`" value=`"$database`" />", '<add key="DatabaseCreatorKey" value="RockIntegrationTestProject" />') {
    if (-not $settings.Contains($expected)) {
        throw "The test settings did not take $expected; the example file has changed shape."
    }
}

# The harness restores an image by writing its files into a Data folder beside the test assembly and
# attaching them. Files the test process writes there inherit the workspace's permissions, which do
# not let LocalDB write or delete them, so it attaches them read-only and cannot drop them for the
# next class. The folder grants full control to everyone, inherited by every file written into it.
$data = "$project\bin\Debug\net472\Data"
New-Item -ItemType Directory -Force $data | Out-Null
icacls $data /grant '*S-1-1-0:(OI)(CI)F' | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "The test database folder $data could not be opened to LocalDB."
}

# A runner's automatic instance sometimes cannot be created on its first start, and sqllocaldb
# reports that in its output while still exiting zero, so the instance's own state is what is read.
# Once it fails, it is deleted and created again, which builds it fresh.
function Test-Running {
    return [bool]((sqllocaldb info MSSQLLocalDB 2>&1 | Out-String) -match 'State:\s+Running')
}

sqllocaldb start MSSQLLocalDB
if (-not (Test-Running)) {
    sqllocaldb stop MSSQLLocalDB -k
    sqllocaldb delete MSSQLLocalDB
    sqllocaldb create MSSQLLocalDB -s
}
if (-not (Test-Running)) {
    sqllocaldb info MSSQLLocalDB
    throw 'LocalDB did not start.'
}
