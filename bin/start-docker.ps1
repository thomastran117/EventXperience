$ErrorActionPreference = "Stop"

$binDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $binDir "..") | ForEach-Object { $_.Path }

Write-Host "Project root: $repoRoot"
Write-Host ""

function Check-Command {
    param([string]$cmd, [string]$friendlyName)
    $exists = Get-Command $cmd -ErrorAction SilentlyContinue
    if (-not $exists) {
        Write-Host "ERROR: $friendlyName not found. Please install it."
        exit 1
    } else {
        Write-Host "Found $friendlyName."
    }
}

Write-Host "Checking dependencies..."
Check-Command "docker" "Docker CLI"
Check-Command "docker-compose" "Docker Compose"
Write-Host ""

Write-Host "Running EF Core migrations inside backend container..."
Push-Location $repoRoot

docker-compose run --rm backend dotnet ef database update

Pop-Location
Write-Host "EF Core migrations applied successfully inside container."

Write-Host ""
Write-Host "Starting Docker containers..."
Push-Location $repoRoot
docker-compose up --build
Pop-Location

Write-Host ""
Write-Host "Docker containers started successfully."
