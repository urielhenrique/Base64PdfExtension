using System;
using System.IO;
using System.Diagnostics;
using OutSystems.NssBase64PdfExtension;

class TestBedsCad
{
    static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  🛏️  TESTE: Conversão CAD de Camas (Beds)                   ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        string cadDirectory = @"C:\Users\uriel\Downloads\01-01-cad-blocks-net-beds";
        
        if (!Directory.Exists(cadDirectory))
        {
            Console.WriteLine($"❌ Diretório não encontrado: {cadDirectory}");
            Console.ReadKey();
            return;
        }

        var extension = new CssBase64PdfExtension();
        
        // Busca todos os arquivos CAD
        string[] extensions = { "*.dwg", "*.dxf", "*.cad" };
        int totalFiles = 0;
        int successCount = 0;
        int errorCount = 0;

        Console.WriteLine($"📁 Processando: {cadDirectory}");
        Console.WriteLine();

        foreach (var ext in extensions)
        {
            string[] files = Directory.GetFiles(cadDirectory, ext, SearchOption.AllDirectories);
            
            if (files.Length == 0)
                continue;

            Console.WriteLine($"🔍 Encontrados {files.Length} arquivo(s) {ext}");
            Console.WriteLine();

            foreach (string filePath in files)
            {
                totalFiles++;
                Console.WriteLine($"────────────────────────────────────────────────────────");
                Console.WriteLine($"📄 [{totalFiles}] {Path.GetFileName(filePath)}");
                Console.WriteLine($"────────────────────────────────────────────────────────");

                try
                {
                    // Lê o arquivo
                    FileInfo fileInfo = new FileInfo(filePath);
                    Console.WriteLine($"   📊 Tamanho: {fileInfo.Length / 1024.0:F2} KB");
                    
                    byte[] cadBytes = File.ReadAllBytes(filePath);
                    
                    // Detecta formato
                    string format = DetectFormat(cadBytes);
                    Console.WriteLine($"   🔍 Formato: {format}");
                    
                    // Converte
                    Console.Write("   🔄 Convertendo... ");
                    Stopwatch sw = Stopwatch.StartNew();
                    
                    extension.MssConvertBinaryToPdf(
                        cadBytes,
                        out byte[] pdfBytes,
                        out string mimeType,
                        out string fileExtension
                    );
                    
                    sw.Stop();
                    Console.WriteLine($"OK ({sw.ElapsedMilliseconds}ms)");
                    
                    // Salva PDF
                    string outputPath = Path.Combine(
                        Path.GetDirectoryName(filePath),
                        Path.GetFileNameWithoutExtension(filePath) + "_converted.pdf"
                    );
                    
                    File.WriteAllBytes(outputPath, pdfBytes);
                    
                    Console.WriteLine($"   ✅ SUCESSO!");
                    Console.WriteLine($"   💾 PDF: {pdfBytes.Length / 1024.0:F2} KB");
                    Console.WriteLine($"   📁 Salvo: {Path.GetFileName(outputPath)}");
                    
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ ERRO: {ex.Message}");
                    errorCount++;
                }
                
                Console.WriteLine();
            }
        }

        // Resumo final
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                        RESUMO FINAL");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"📊 Total processado: {totalFiles} arquivo(s)");
        Console.WriteLine($"✅ Sucessos: {successCount}");
        Console.WriteLine($"❌ Erros: {errorCount}");
        Console.WriteLine($"📈 Taxa sucesso: {(totalFiles > 0 ? (successCount * 100.0 / totalFiles) : 0):F1}%");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        if (successCount > 0)
        {
            Console.WriteLine($"💡 PDFs salvos em: {cadDirectory}");
            Console.WriteLine();
            Console.Write("❓ Abrir pasta? (S/N): ");
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                Console.WriteLine();
                Process.Start("explorer.exe", cadDirectory);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    static string DetectFormat(byte[] bytes)
    {
        if (bytes.Length < 4) return "Unknown";
        
        // DWG
        if (bytes[0] == 0x41 && bytes[1] == 0x43)
        {
            string version = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(6, bytes.Length));
            return $"DWG ({version})";
        }
        
        // DXF
        string header = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(20, bytes.Length));
        if (header.Contains("SECTION") || header.Contains("HEADER"))
            return "DXF";
        
        // TIFF
        if ((bytes[0] == 0x49 && bytes[1] == 0x49) || (bytes[0] == 0x4D && bytes[1] == 0x4D))
            return "TIFF";
        
        return "Unknown";
    }
}
