param(
    [switch]$Force,
    [switch]$RandomSecret
)

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToBase64String($bytes)
}

$backendDir = "backend"
if (-not (Test-Path -Path $backendDir -PathType Container)) {
    New-Item -ItemType Directory -Path $backendDir | Out-Null
    Write-Host "Created backend directory."
}

$envPath = Join-Path -Path $backendDir -ChildPath ".env"

if ((Test-Path $envPath) -and -not $Force) {
    Write-Host "Warning: $envPath already exists. Use -Force to overwrite."
    exit 0
}

$secret = if ($RandomSecret) { New-RandomSecret } else { "change_me_dev_secret" }

$envContent = @"
##############################################
# Server
##############################################

FRONTEND_CLIENT="http://localhost:3090"
PORT=8090

##############################################
# Databases
##############################################

DB_CONNECTION_STRING="Server=localhost;Port=3306;Database=database;User=root;Password=password"
REDIS_CONNECTION="redis://localhost:6379/0"
MONGO_URL="mongodb://localhost:27017/app"

##############################################
# CORS Configuration
##############################################
# Allowed origins for cross-origin requests (JSON array as string)
# Typical dev front-end address:
#   ["http://localhost:3080"]
CORS_ALLOWED_REGION=["http://localhost:3080"]

##############################################
# Security / JWT
##############################################
JWT_SECRET_KEY=your_super_secret_key_here_12345

##############################################
# Email (SMTP credentials)
##############################################
EMAIL="example@email.com"
PASSWORD="your_email_password_here"

##############################################
# Google OAuth2
##############################################
GOOGLE_CLIENT_ID="your-google-client-id.apps.googleusercontent.com"

##############################################
# Microsoft OAuth2
##############################################
MS_TENANT_ID="your-microsoft-tenant-id"
MS_CLIENT_ID="your-microsoft-client-id"
"@

$envContent | Set-Content -Path $envPath -Encoding UTF8 -NoNewline
Write-Host "Created .env at $envPath"
