using System;
using System.IO;
using OutSystems.NssBase64PdfExtension;

namespace Base64PdfExtensionTests
{
    /// <summary>
    /// Script de teste para conversão CAD com arquivos reais
    /// </summary>
    class TestCadConversion
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  🚀 TESTE DE CONVERSÃO CAD → PDF (ARQUIVOS REAIS)        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var extension = new CssBase64PdfExtension();

            // Teste 1: Converter arquivo especificado pelo usuário
            if (args.Length > 0)
            {
                string filePath = args[0];
                TestFileConversion(extension, filePath);
            }
            else
            {
                // Teste 2: Buscar arquivos CAD automaticamente
                Console.WriteLine("📂 Buscando arquivos CAD no sistema...");
                Console.WriteLine();

                string[] searchPaths = new string[]
                {
                    @"C:\",
                    @"C:\Temp",
                    @"C:\Users\" + Environment.UserName + @"\Downloads",
                    @"C:\Users\" + Environment.UserName + @"\Documents",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Directory.GetCurrentDirectory()
                };

                bool foundFiles = false;

                foreach (var searchPath in searchPaths)
                {
                    if (!Directory.Exists(searchPath))
                        continue;

                    try
                    {
                        Console.WriteLine($"   Verificando: {searchPath}");

                        string[] extensions = { "*.dwg", "*.dxf", "*.cad" };
                        
                        foreach (var ext in extensions)
                        {
                            var files = Directory.GetFiles(searchPath, ext, SearchOption.TopDirectoryOnly);
                            
                            if (files.Length > 0)
                            {
                                foundFiles = true;
                                Console.WriteLine($"   ✅ Encontrado: {files[0]}");
                                Console.WriteLine();
                                
                                TestFileConversion(extension, files[0]);
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // Ignora erros de permissão
                        continue;
                    }
                }

                if (!foundFiles)
                {
                    Console.WriteLine("⚠️  Nenhum arquivo CAD encontrado automaticamente.");
                    Console.WriteLine();
                    ShowManualTestInstructions();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static void TestFileConversion(CssBase64PdfExtension extension, string inputPath)
        {
            try
            {
                Console.WriteLine("════════════════════════════════════════════════════════════");
                Console.WriteLine($"📄 Arquivo: {Path.GetFileName(inputPath)}");
                Console.WriteLine($"📁 Caminho: {inputPath}");
                Console.WriteLine("════════════════════════════════════════════════════════════");
                Console.WriteLine();

                // Verifica se arquivo existe
                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"❌ Erro: Arquivo não encontrado!");
                    return;
                }

                FileInfo fileInfo = new FileInfo(inputPath);
                Console.WriteLine($"📊 Tamanho: {fileInfo.Length / 1024.0:F2} KB");
                Console.WriteLine();

                // Lê o arquivo
                Console.WriteLine("📖 Lendo arquivo...");
                byte[] fileBytes = File.ReadAllBytes(inputPath);
                Console.WriteLine($"   ✅ {fileBytes.Length} bytes lidos");
                Console.WriteLine();

                // Detecta o formato
                Console.WriteLine("🔍 Detectando formato...");
                string detectedFormat = DetectFormat(fileBytes);
                Console.WriteLine($"   ✅ Formato detectado: {detectedFormat}");
                Console.WriteLine();

                // Converte para PDF
                Console.WriteLine("🔄 Convertendo para PDF...");
                DateTime startTime = DateTime.Now;

                extension.MssConvertBinaryToPdf(
                    fileBytes,
                    out byte[] pdfBytes,
                    out string mimeType,
                    out string fileExtension
                );

                TimeSpan elapsed = DateTime.Now - startTime;
                Console.WriteLine($"   ✅ Conversão concluída em {elapsed.TotalSeconds:F2}s");
                Console.WriteLine();

                // Informações do resultado
                Console.WriteLine("📝 Resultado:");
                Console.WriteLine($"   Mime Type: {mimeType}");
                Console.WriteLine($"   Extensão: {fileExtension}");
                Console.WriteLine($"   Tamanho PDF: {pdfBytes.Length / 1024.0:F2} KB");
                Console.WriteLine();

                // Salva o PDF
                string outputPath = Path.ChangeExtension(inputPath, ".pdf");
                string outputDir = Path.GetDirectoryName(inputPath);
                string fileName = Path.GetFileNameWithoutExtension(inputPath);
                outputPath = Path.Combine(outputDir, fileName + "_converted.pdf");

                File.WriteAllBytes(outputPath, pdfBytes);
                Console.WriteLine($"💾 PDF salvo em:");
                Console.WriteLine($"   {outputPath}");
                Console.WriteLine();

                // Abre o PDF (opcional)
                Console.Write("❓ Deseja abrir o PDF? (S/N): ");
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key == ConsoleKey.S)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(outputPath);
                        Console.WriteLine("   ✅ PDF aberto!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️  Não foi possível abrir: {ex.Message}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ✅ CONVERSÃO BEM-SUCEDIDA!                               ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ❌ ERRO NA CONVERSÃO                                     ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine($"Mensagem: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack Trace:");
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }

        static string DetectFormat(byte[] bytes)
        {
            if (bytes.Length < 4)
                return "Unknown";

            // PDF
            if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                return "PDF";

            // DWG
            if (bytes[0] == 0x41 && bytes[1] == 0x43) // "AC"
            {
                string version = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(6, bytes.Length));
                return $"DWG ({version})";
            }

            // DXF
            string header = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(20, bytes.Length));
            if (header.Contains("SECTION") || header.Contains("HEADER"))
                return "DXF (ASCII)";

            // TIFF
            if ((bytes[0] == 0x49 && bytes[1] == 0x49) || (bytes[0] == 0x4D && bytes[1] == 0x4D))
                return "TIFF";

            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50)
                return "PNG";

            // JPEG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
                return "JPEG";

            return "Unknown";
        }

        static void ShowManualTestInstructions()
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("📖 INSTRUÇÕES DE TESTE MANUAL");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Opção 1 - Via Linha de Comando:");
            Console.WriteLine("   TestCadConversion.exe \"C:\\caminho\\arquivo.dwg\"");
            Console.WriteLine();
            Console.WriteLine("Opção 2 - Via Código C#:");
            Console.WriteLine();
            Console.WriteLine("   var extension = new CssBase64PdfExtension();");
            Console.WriteLine("   byte[] dwgBytes = File.ReadAllBytes(@\"C:\\arquivo.dwg\");");
            Console.WriteLine("   extension.MssConvertBinaryToPdf(");
            Console.WriteLine("       dwgBytes,");
            Console.WriteLine("       out byte[] pdfBytes,");
            Console.WriteLine("       out string mimeType,");
            Console.WriteLine("       out string fileExtension");
            Console.WriteLine("   );");
            Console.WriteLine("   File.WriteAllBytes(@\"C:\\output.pdf\", pdfBytes);");
            Console.WriteLine();
            Console.WriteLine("Opção 3 - Teste com Base64:");
            Console.WriteLine();
            Console.WriteLine("   byte[] fileBytes = File.ReadAllBytes(@\"C:\\arquivo.dwg\");");
            Console.WriteLine("   string base64 = Convert.ToBase64String(fileBytes);");
            Console.WriteLine("   extension.MssConvertBase64ToPdf(");
            Console.WriteLine("       base64,");
            Console.WriteLine("       out byte[] pdfBytes,");
            Console.WriteLine("       out _,");
            Console.WriteLine("       out _");
            Console.WriteLine("   );");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("💡 DICA: Você pode baixar arquivos CAD de exemplo em:");
            Console.WriteLine("   - https://knowledge.autodesk.com/support/autocad/downloads");
            Console.WriteLine("   - https://www.cad-blocks.net (DWG grátis)");
            Console.WriteLine("   - https://www.freecadfiles.com");
            Console.WriteLine();
        }
    }
}
