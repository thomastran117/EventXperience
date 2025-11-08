param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$Command
)

function Show-Help {
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Cyan
    Write-Host "  .\app.ps1 <command> [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Description:" -ForegroundColor Cyan
    Write-Host "  Main entry point for managing and running project tasks." 
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    
    "{0,-10} {1}" -f "docker", "Run start-docker.ps1   → Starts Docker Compose services"  | Write-Host
    "{0,-10} {1}" -f "app",    "Run start-app.ps1      → Launches the app locally"       | Write-Host
    "{0,-10} {1}" -f "env",    "Run create-env.ps1   → Generates a new .env file"      | Write-Host
    "{0,-10} {1}" -f "setup",  "Run setup.ps1        → Installs local dependencies"    | Write-Host
    "{0,-10} {1}" -f "--help", "Show this help message"                                | Write-Host
    
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\app.ps1 docker" -ForegroundColor Yellow
    Write-Host "  .\app.ps1 app"
    Write-Host "  .\app.ps1 env"
    Write-Host "  .\app.ps1 setup"
    Write-Host ""
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

switch ($Command) {
    "docker" { & "$ScriptDir\bin\start-docker.ps1"; break }
    "app"    { & "$ScriptDir\bin\start-app.ps1"; break }
    "env"    { & "$ScriptDir\bin\create-env.ps1"; break }
    "setup"  { & "$ScriptDir\bin\setup.ps1"; break }
    "--help" { Show-Help; break }
    default {
        Write-Host "`nUnknown or missing command: $Command`n" -ForegroundColor Red
        Show-Help
        exit 1
    }
}
