param(
  [string]$BaseUrl = "http://127.0.0.1:5080"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================="
Write-Host " Starting SeatHold API on $BaseUrl"
Write-Host "============================================="
Write-Host ""

$api = Start-Process dotnet -ArgumentList "run --project SeatHold.Api --urls $BaseUrl" -PassThru -WindowStyle Hidden

# Newman will export the environment (with seatId/holdId) here:
$exportedEnvPath = Join-Path $PSScriptRoot "SeatHoldApi.local.exported.postman_environment.json"

try {
  Write-Host "Waiting for API to become ready..."

  $ready = $false
  for ($i = 0; $i -lt 40; $i++) {
    try {
      Invoke-WebRequest -Uri "$BaseUrl/swagger/index.html" -UseBasicParsing | Out-Null
      $ready = $true
      break
    }
    catch { Start-Sleep -Milliseconds 250 }
  }

  if (-not $ready) { throw "API did not become ready in time." }

  Write-Host "API is ready."
  Write-Host ""
  Write-Host "============================================="
  Write-Host " Running Newman collection"
  Write-Host "============================================="
  Write-Host ""

  # Run newman; export environment so we can read generated variables after
  newman run `
    (Join-Path $PSScriptRoot "SeatHoldApi.postman_collection.json") `
    -e (Join-Path $PSScriptRoot "SeatHoldApi.local.postman_environment.json") `
    --export-environment $exportedEnvPath `
    --reporters cli

  if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Newman failed."
    exit $LASTEXITCODE
  }

  # Read exported env and print key variables
  $envJson = Get-Content $exportedEnvPath -Raw | ConvertFrom-Json
  $seatId = ($envJson.values | Where-Object { $_.key -eq "seatId" }).value
  $holdId = ($envJson.values | Where-Object { $_.key -eq "holdId" }).value

  Write-Host ""
  Write-Host "============================================="
  Write-Host " Newman run completed successfully"
  Write-Host "============================================="
  Write-Host "Generated seatId: $seatId"
  Write-Host "Generated holdId: $holdId"
}
finally {
  if ($api -and -not $api.HasExited) {
    Write-Host ""
    Write-Host "Stopping API..."
    Stop-Process -Id $api.Id -Force
  }
}
