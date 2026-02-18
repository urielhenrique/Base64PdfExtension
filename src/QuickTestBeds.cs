using System;
using System.IO;
using OutSystems.NssBase64PdfExtension;

// TESTE RÁPIDO - Cole isso em um arquivo .cs e execute
class QuickTest
{
    static void Main()
    {
        var extension = new CssBase64PdfExtension();
        string folder = @"C:\Users\uriel\Downloads\01-01-cad-blocks-net-beds";
        
        Console.WriteLine("🔍 Buscando arquivos CAD...\n");
        
        string[] files = Directory.GetFiles(folder, "*.dwg");
        
        if (files.Length == 0)
        {
            files = Directory.GetFiles(folder, "*.dxf");
        }
        
        if (files.Length == 0)
        {
            Console.WriteLine("❌ Nenhum arquivo DWG ou DXF encontrado!");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"✅ Encontrados {files.Length} arquivo(s)\n");
        
        foreach (string file in files)
        {
            try
            {
                Console.WriteLine($"📄 {Path.GetFileName(file)}");
                Console.Write("   Convertendo... ");
                
                byte[] cadBytes = File.ReadAllBytes(file);
                
                extension.MssConvertBinaryToPdf(
                    cadBytes,
                    out byte[] pdfBytes,
                    out _,
                    out _
                );
                
                string pdfPath = Path.ChangeExtension(file, ".pdf");
                File.WriteAllBytes(pdfPath, pdfBytes);
                
                Console.WriteLine($"✅ OK ({pdfBytes.Length/1024}KB)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO: {ex.Message}");
            }
        }
        
        Console.WriteLine($"\n💾 PDFs salvos em:\n{folder}\n");
        Console.ReadKey();
    }
}
