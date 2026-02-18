# ============================================================================
# SCRIPT DE INSTALAÇÃO AUTOMÁTICA - Bibliotecas CAD Open-Source
# ============================================================================
# 
# Este script instala automaticamente as bibliotecas necessárias para
# conversão de arquivos CAD (DWG, DXF, .CAD) para PDF.
#
# Bibliotecas instaladas:
# - ACadSharp (DWG + DXF)
# - CSMath (dependência)
# - netDxf (DXF)
#
# ============================================================================

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  CAD Libraries Installation Script" -ForegroundColor Cyan
Write-Host "  Open-Source & FREE" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Encontra o diretório do projeto
$projectPath = Get-ChildItem -Recurse -Filter "Base64PdfExtension.csproj" | Select-Object -First 1 -ExpandProperty DirectoryName

if (-not $projectPath) {
    Write-Host "❌ Erro: Arquivo Base64PdfExtension.csproj não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Diretório do projeto: $projectPath" -ForegroundColor Yellow
Write-Host ""

cd $projectPath

# ============================================================================
# MÉTODO 1: Tentar com NuGet CLI
# ============================================================================

Write-Host "🔍 Verificando NuGet CLI..." -ForegroundColor Cyan

$nugetPath = Get-Command nuget.exe -ErrorAction SilentlyContinue

if ($nugetPath) {
    Write-Host "✅ NuGet CLI encontrado: $($nugetPath.Source)" -ForegroundColor Green
    Write-Host ""
    Write-Host "📦 Instalando pacotes..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        Write-Host "   [1/3] Instalando ACadSharp..." -ForegroundColor Yellow
        & nuget.exe install ACadSharp -OutputDirectory packages -Framework net48
        
        Write-Host "   [2/3] Instalando CSMath..." -ForegroundColor Yellow
        & nuget.exe install CSMath -OutputDirectory packages -Framework net48
        
        Write-Host "   [3/3] Instalando netDxf..." -ForegroundColor Yellow
        & nuget.exe install netDxf -OutputDirectory packages -Framework net48
        
        Write-Host ""
        Write-Host "✅ Pacotes instalados com sucesso!" -ForegroundColor Green
        $installSuccess = $true
    }
    catch {
        Write-Host "❌ Erro ao instalar pacotes via NuGet CLI" -ForegroundColor Red
        $installSuccess = $false
    }
}
else {
    Write-Host "⚠️  NuGet CLI não encontrado no PATH" -ForegroundColor Yellow
    $installSuccess = $false
}

# ============================================================================
# MÉTODO 2: Tentar com dotnet CLI
# ============================================================================

if (-not $installSuccess) {
    Write-Host ""
    Write-Host "🔍 Tentando com dotnet CLI..." -ForegroundColor Cyan
    
    $dotnetPath = Get-Command dotnet.exe -ErrorAction SilentlyContinue
    
    if ($dotnetPath) {
        Write-Host "✅ dotnet CLI encontrado" -ForegroundColor Green
        Write-Host ""
        Write-Host "📦 Instalando pacotes..." -ForegroundColor Cyan
        Write-Host ""
        
        try {
            Write-Host "   [1/3] Instalando ACadSharp..." -ForegroundColor Yellow
            dotnet add package ACadSharp --version 2.1.0
            
            Write-Host "   [2/3] Instalando CSMath..." -ForegroundColor Yellow
            dotnet add package CSMath --version 2.0.0
            
            Write-Host "   [3/3] Instalando netDxf..." -ForegroundColor Yellow
            dotnet add package netDxf --version 3.0.0
            
            Write-Host ""
            Write-Host "✅ Pacotes instalados com sucesso!" -ForegroundColor Green
            $installSuccess = $true
        }
        catch {
            Write-Host "❌ Erro ao instalar pacotes via dotnet CLI" -ForegroundColor Red
            Write-Host "   Nota: Este projeto usa formato antigo (.NET Framework)" -ForegroundColor Gray
            $installSuccess = $false
        }
    }
    else {
        Write-Host "⚠️  dotnet CLI não encontrado" -ForegroundColor Yellow
        $installSuccess = $false
    }
}

# ============================================================================
# MÉTODO 3: Download Manual (Fallback)
# ============================================================================

if (-not $installSuccess) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Red
    Write-Host "  INSTALAÇÃO MANUAL NECESSÁRIA" -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "NuGet e dotnet CLI não estão disponíveis." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📋 OPÇÕES:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1️⃣  VISUAL STUDIO (Recomendado):" -ForegroundColor Green
    Write-Host "   - Abrir Base64PdfExtension.sln no Visual Studio"
    Write-Host "   - Tools > NuGet Package Manager > Package Manager Console"
    Write-Host "   - Executar:"
    Write-Host "     Install-Package ACadSharp" -ForegroundColor White
    Write-Host "     Install-Package CSMath" -ForegroundColor White
    Write-Host "     Install-Package netDxf" -ForegroundColor White
    Write-Host ""
    Write-Host "2️⃣  NUGET.EXE (Download):" -ForegroundColor Green
    Write-Host "   - Baixar de: https://www.nuget.org/downloads"
    Write-Host "   - Adicionar ao PATH"
    Write-Host "   - Executar novamente este script"
    Write-Host ""
    Write-Host "3️⃣  DOWNLOAD MANUAL:" -ForegroundColor Green
    Write-Host "   - ACadSharp: https://www.nuget.org/packages/ACadSharp"
    Write-Host "   - CSMath: https://www.nuget.org/packages/CSMath"
    Write-Host "   - netDxf: https://www.nuget.org/packages/netDxf"
    Write-Host "   - Extrair para: $projectPath\packages"
    Write-Host ""
    
    exit 1
}

# ============================================================================
# PÓS-INSTALAÇÃO
# ============================================================================

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  PRÓXIMOS PASSOS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "1️⃣  HABILITAR CONVERSÃO CAD:" -ForegroundColor Green
Write-Host "   📝 Editar: Base64PdfExtension.cs" -ForegroundColor White
Write-Host "   🔧 Descomentar linha ~3:" -ForegroundColor White
Write-Host "      //#define INCLUDE_CAD_LIBRARIES  ← REMOVER //" -ForegroundColor Yellow
Write-Host "      #define INCLUDE_CAD_LIBRARIES   ← DEVE FICAR ASSIM" -ForegroundColor Green
Write-Host ""

Write-Host "2️⃣  RECOMPILAR PROJETO:" -ForegroundColor Green
Write-Host "   msbuild Base64PdfExtension.csproj /p:Configuration=Release" -ForegroundColor White
Write-Host ""

Write-Host "3️⃣  TESTAR:" -ForegroundColor Green
Write-Host "   - Criar arquivo de teste com DWG/DXF" -ForegroundColor White
Write-Host "   - Executar MssConvertBinaryToPdf()" -ForegroundColor White
Write-Host "   - Verificar PDF gerado" -ForegroundColor White
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "📚 DOCUMENTAÇÃO:" -ForegroundColor Cyan
Write-Host "   - CAD_FORMAT_RESEARCH.md" -ForegroundColor White
Write-Host "   - CAD_CONVERSION_GUIDE.md" -ForegroundColor White
Write-Host "   - TEST_REPORT.md" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "✅ Instalação concluída com sucesso!" -ForegroundColor Green
Write-Host ""

# Verificar se packages.config existe
$packagesConfigPath = Join-Path $projectPath "packages.config"
if (Test-Path $packagesConfigPath) {
    Write-Host "✅ packages.config verificado" -ForegroundColor Green
    
    $packagesConfig = Get-Content $packagesConfigPath -Raw
    if ($packagesConfig -match "ACadSharp") {
        Write-Host "✅ ACadSharp listado em packages.config" -ForegroundColor Green
    }
    if ($packagesConfig -match "netDxf") {
        Write-Host "✅ netDxf listado em packages.config" -ForegroundColor Green
    }
    if ($packagesConfig -match "CSMath") {
        Write-Host "✅ CSMath listado em packages.config" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Pressione qualquer tecla para sair..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
