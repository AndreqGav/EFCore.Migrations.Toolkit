$ProjectDir = Join-Path $PSScriptRoot "..\.."

$providers = @(
    @{
        Name       = "SqlServer"
        Context    = "SqlServerMigrationDbContext"
        OutputDir  = "MigrationTests/SqlServer/Migrations"
        Folder     = Join-Path $PSScriptRoot "SqlServer\Migrations"
    },
    @{
        Name       = "PostgreSQL"
        Context    = "PostgreSqlMigrationDbContext"
        OutputDir  = "MigrationTests/PostgreSQL/Migrations"
        Folder     = Join-Path $PSScriptRoot "PostgreSQL\Migrations"
    }
)

Set-Location -Path $ProjectDir

foreach ($p in $providers) {
    Write-Host ""
    Write-Host "=== $($p.Name): сброс и создание миграции ===" -ForegroundColor Cyan

    if (Test-Path $p.Folder) {
        Write-Host "[1/2] Удаляем старую папку Migrations..." -ForegroundColor Yellow
        Remove-Item -Path $p.Folder -Recurse -Force
    } else {
        Write-Host "[1/2] Папка Migrations не найдена, пропускаем." -ForegroundColor DarkGray
    }

    Write-Host "[2/2] Создаем новую миграцию Initial..." -ForegroundColor Yellow
    dotnet ef migrations add Initial `
        --framework net10.0 `
        --project EFCore.Migrations.Toolkit.Tests `
        --context $p.Context `
        --output-dir $p.OutputDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ОШИБКА: $($p.Name) завершился с кодом $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Write-Host "=== $($p.Name): готово ===" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Все провайдеры обновлены ===" -ForegroundColor Green
