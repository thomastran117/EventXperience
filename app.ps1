param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$Command = "--help"
)

$ErrorActionPreference = "Stop"

$ScriptDir =
    if ($PSScriptRoot) {
        $PSScriptRoot
    } else {
        Split-Path -Parent $MyInvocation.MyCommand.Definition
    }

$RepoRoot = Resolve-Path $ScriptDir
$ScriptsDir = Join-Path $RepoRoot "bin"

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host $Text -ForegroundColor Cyan
}

function Write-Command {
    param(
        [string]$Name,
        [string]$Description
    )
    "{0,-12} {1}" -f $Name, $Description | Write-Host
}

function Invoke-Script {
    param([string]$ScriptName)

    $Path = Join-Path $ScriptsDir $ScriptName

    if (-not (Test-Path $Path)) {
        Write-Host "`nScript not found: $ScriptName" -ForegroundColor Red
        Write-Host "Expected location: $Path`n" -ForegroundColor DarkGray
        exit 1
    }

    & $Path
}

function Show-Help {
    Write-Header "Usage"
    Write-Host "  .\app.ps1 <command>" -ForegroundColor Yellow

    Write-Header "Description"
    Write-Host "  Main entry point for managing and running project tasks."

    Write-Header "Commands"
    Write-Command "docker"  "Start Docker Compose services"
    Write-Command "k8"      "Start Kubernetes services"
    Write-Command "local"   "Run the application locally"
    Write-Command "env"     "Generate a new .env file"
    Write-Command "setup"   "Install local dependencies"
    Write-Command "format"  "Run code formatting scripts"
    Write-Command "--help"  "Show this help message"

    Write-Header "Examples"
    Write-Host "  .\app.ps1 docker" -ForegroundColor Yellow
    Write-Host "  .\app.ps1 local"
    Write-Host "  .\app.ps1 env"
    Write-Host "  .\app.ps1 setup"
    Write-Host ""
}

switch ($Command.ToLower()) {
    "docker" { Invoke-Script "docker.ps1" }
    "k8" { Invoke-Script "k8.ps1" }
    "local"  { Invoke-Script "local.ps1" }
    "env"    { Invoke-Script "env.ps1" }
    "setup"  { Invoke-Script "setup.ps1" }
    "format" { Invoke-Script "format.ps1" }
    "--help" { Show-Help }
    default {
        Write-Host "`nUnknown command: $Command`n" -ForegroundColor Red
        Show-Help
        exit 1
    }
}
