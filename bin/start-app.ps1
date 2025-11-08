$ErrorActionPreference = "Stop"

function Check-Command {
    param(
        [string]$cmd,
        [string]$friendlyName
    )
    $exists = Get-Command $cmd -ErrorAction SilentlyContinue
    if (-not $exists) {
        Write-Host "ERROR: $friendlyName not found. Please install it before running this script."
        exit 1
    } else {
        Write-Host "Found $friendlyName."
    }
}

Write-Host "Checking dependencies..."
Check-Command "node" "Node.js"
Check-Command "npm" "npm"
Check-Command "dotnet" ".NET SDK"
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Join-Path $scriptDir ".."
$frontend  = Join-Path $repoRoot "frontend"
$backend   = Join-Path $repoRoot "backend"

Write-Host "🚀 Starting frontend..." -ForegroundColor Cyan
$frontendProc = Start-Process `
  -FilePath "cmd.exe" `
  -ArgumentList "/c","npm run dev" `
  -WorkingDirectory $frontend `
  -NoNewWindow `
  -PassThru

Write-Host "🧩 Checking for ASP.NET backend..." -ForegroundColor Cyan

$projFiles = Get-ChildItem -Path $backend -Filter *.csproj -Recurse
if ($projFiles.Count -eq 0) {
  Write-Host "❌ No .csproj file found in backend folder." -ForegroundColor Red
  exit 1
}

$projPath = $projFiles[0].FullName
Write-Host "✅ Found project file: $projPath" -ForegroundColor Green

Write-Host "🚀 Starting ASP.NET backend..." -ForegroundColor Cyan
$backendProc = Start-Process `
  -FilePath "cmd.exe" `
  -ArgumentList "/c","dotnet run --no-launch-profile --project `"$projPath`"" `
  -WorkingDirectory $backend `
  -NoNewWindow `
  -PassThru

Write-Host "`n✅ Both servers are running. Press Ctrl+C to stop them." -ForegroundColor Green

try {
  Wait-Process -Id $frontendProc.Id,$backendProc.Id
} finally {
  Write-Host "`n🛑 Stopping servers..." -ForegroundColor Yellow
  if ($frontendProc -and -not $frontendProc.HasExited) { Stop-Process -Id $frontendProc.Id -Force }
  if ($backendProc  -and -not $backendProc.HasExited)  { Stop-Process -Id $backendProc.Id  -Force }
  Write-Host "✅ All stopped." -ForegroundColor Green
}
