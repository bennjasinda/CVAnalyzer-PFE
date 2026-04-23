# Database Migration Script for Enhanced CV Matching System
# This script creates the required tables for the enhanced scoring system

$server = "localhost"
$database = "Cvparsing"
$scriptPath = "C:\Users\bennj\Downloads\CVAnalyzer-PFE\Candidat\Scripts\AddCvExperiencesAndDiplomes.sql"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Migration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server: $server" -ForegroundColor Yellow
Write-Host "Database: $database" -ForegroundColor Yellow
Write-Host "Script: $scriptPath" -ForegroundColor Yellow
Write-Host ""

# Check if sqlcmd is available
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: sqlcmd not found. Please install SQL Server Command Line Utilities." -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Open the SQL file in SQL Server Management Studio and execute it manually." -ForegroundColor Yellow
    Write-Host "File location: $scriptPath" -ForegroundColor Yellow
    pause
    exit 1
}

# Check if the script file exists
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: Migration script not found at: $scriptPath" -ForegroundColor Red
    pause
    exit 1
}

Write-Host "Executing migration script..." -ForegroundColor Green
Write-Host ""

# Execute the SQL script
try {
    $output = sqlcmd -S $server -d $database -i $scriptPath -b 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Migration completed successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Created tables:" -ForegroundColor Cyan
        Write-Host "  - CvExperiences" -ForegroundColor White
        Write-Host "  - CvDiplomes" -ForegroundColor White
        Write-Host ""
        Write-Host "Added columns to Match table:" -ForegroundColor Cyan
        Write-Host "  - BonusScore" -ForegroundColor White
        Write-Host "  - SkillsBonusScore" -ForegroundColor White
        Write-Host "  - EducationBonusScore" -ForegroundColor White
        Write-Host ""
        Write-Host "You can now restart your application." -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "Migration failed!" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Output:" -ForegroundColor Yellow
        Write-Host $output -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please check the error messages above." -ForegroundColor Red
    }
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error executing migration!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
