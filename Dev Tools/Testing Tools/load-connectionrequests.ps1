# load-connectionrequests.ps1
# Usage:
#   $env:AUTH_TOKEN="**************"
#   .\load-connectionrequests.ps1
#   # or:
#   .\load-connectionrequests.ps1 -Users 30 -PostsPerUser 20 -PersonAliasId 15 -Url "http://localhost:6229/api/ConnectionRequests"

#"C:\Portable\bombardier.exe"
param(
  [string]$Bombardier = "C:\Portable\bombardier-windows-386.exe",
  [string]$Url = "http://localhost:6229/api/ConnectionRequests",
  [int]$Users = 2,
  [int]$PostsPerUser = 5,
  [int]$PersonAliasId = 1,
  [string]$AuthToken = $env:AUTH_TOKEN
)

if (-not (Test-Path $Bombardier)) { throw "bombardier.exe not found at: $Bombardier" }
if ([string]::IsNullOrWhiteSpace($AuthToken)) { throw "Set AUTH_TOKEN env var or pass -AuthToken" }

$total = $Users * $PostsPerUser

# Write body to a temp file (bombardier uses -f for request body file)
$bodyPath = Join-Path $env:TEMP "connectionrequest-body.json"
@"
{
  "ConnectionOpportunityId": 1,
  "PersonAliasId": $PersonAliasId,
  "ConnectionStatusId": 1,
  "ConnectionState": 0,
  "ConnectionTypeId": 1,
  "Order": 0,
  "WasCompletedOnTime": false,
  "ModifiedAuditValuesAlreadyUpdated": false
}
"@ | Set-Content -Path $bodyPath -Encoding UTF8

& $Bombardier `
  -m POST `
  -c $Users `
  -n $total `
  -H "Content-Type: application/json" `
  -H "Accept: application/json" `
  -H "Authorization-Token: $AuthToken" `
  -H "Connection: keep-alive" `
  -f $bodyPath `
  $Url

Remove-Item -Force $bodyPath -ErrorAction SilentlyContinue