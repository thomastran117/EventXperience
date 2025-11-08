$ErrorActionPreference = "Stop"

$binDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $binDir "..") | ForEach-Object { $_.Path }
$backend  = Join-Path $repoRoot "backend"

Write-Host "Project root: $repoRoot"
Write-Host "Backend path: $backend"
Write-Host ""

function Check-Command {
    param([string]$cmd, [string]$friendlyName)
    $exists = Get-Command $cmd -ErrorAction SilentlyContinue
    if (-not $exists) {
        Write-Host "ERROR: $friendlyName not found. Please install it before running this script."
        exit 1
    } else {
        Write-Host "Found $friendlyName."
    }
}

Write-Host "Checking dependencies..."
Check-Command "docker" "Docker CLI"
Check-Command "docker-compose" "Docker Compose"
Check-Command "dotnet" ".NET SDK"
Write-Host ""

if (Test-Path $backend) {
    Write-Host "Applying EF Core migrations before starting containers..."
    Push-Location $backend

    try {
        dotnet ef database update
        Write-Host "EF Core migrations applied successfully."
    } catch {
        Write-Host "ERROR: EF Core migration failed. Check your connection string or EF setup."
        Pop-Location
        exit 1
    }

    Pop-Location
} else {
    Write-Host "WARNING: Backend folder not found at $backend"
}

if (Test-Path (Join-Path $repoRoot "docker-compose.yml")) {
    Write-Host ""
    Write-Host "Starting Docker containers..."
    Push-Location $repoRoot
    docker-compose up --build
    Pop-Location
} else {
    Write-Host "ERROR: docker-compose.yml not found at $repoRoot"
    exit 1
}

Write-Host ""
Write-Host "Docker containers started successfully."
