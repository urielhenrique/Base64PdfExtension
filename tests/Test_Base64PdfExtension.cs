using System;
using System.IO;
using System.Text;
using OutSystems.NssBase64PdfExtension;

namespace Base64PdfExtensionTests
{
    /// <summary>
    /// Classe de teste para validar funcionalidades do Base64PdfExtension
    /// </summary>
    public class TestBase64PdfExtension
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Testes Base64PdfExtension ===\n");
            
            CssBase64PdfExtension extension = new CssBase64PdfExtension();
            
            // Teste 1: PDF Base64
            TestPdfBase64(extension);
            
            // Teste 2: PNG Base64
            TestPngBase64(extension);
            
            // Teste 3: JPEG Base64
            TestJpegBase64(extension);
            
            // Teste 4: Binário direto (PNG)
            TestBinaryInput(extension);
            
            // Teste 5: Base64 com prefixo data URI
            TestDataUriBase64(extension);
            
            // Teste 6: Formato não suportado
            TestUnsupportedFormat(extension);
            
            // Teste 7: CAD (DWG) - deve falhar com mensagem informativa
            TestCadFormat(extension);
            
            Console.WriteLine("\n=== Testes concluídos ===");
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
        
        static void TestPdfBase64(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 1: PDF Base64");
            try
            {
                // PDF simples (header %PDF-1.4)
                string pdfBase64 = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes("%PDF-1.4\n%fake pdf content")
                );
                
                extension.MssConvertBase64ToPdf(
                    pdfBase64, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✓ PDF detectado: {mime} | {ext}");
                Console.WriteLine($"  Tamanho: {binary.Length} bytes\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro: {ex.Message}\n");
            }
        }
        
        static void TestPngBase64(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 2: PNG Base64");
            try
            {
                // PNG header mínimo
                byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                string pngBase64 = Convert.ToBase64String(pngBytes);
                
                extension.MssConvertBase64ToPdf(
                    pngBase64, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✓ PNG detectado: {mime} | {ext}");
                Console.WriteLine($"  Tamanho: {binary.Length} bytes\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro: {ex.Message}\n");
            }
        }
        
        static void TestJpegBase64(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 3: JPEG Base64");
            try
            {
                // JPEG header mínimo
                byte[] jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
                string jpegBase64 = Convert.ToBase64String(jpegBytes);
                
                extension.MssConvertBase64ToPdf(
                    jpegBase64, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✓ JPEG detectado: {mime} | {ext}");
                Console.WriteLine($"  Tamanho: {binary.Length} bytes\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro: {ex.Message}\n");
            }
        }
        
        static void TestBinaryInput(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 4: Binário direto (PNG)");
            try
            {
                // PNG header mínimo
                byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                
                extension.MssConvertBinaryToPdf(
                    pngBytes, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✓ PNG detectado (via binário): {mime} | {ext}");
                Console.WriteLine($"  Tamanho: {binary.Length} bytes\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro: {ex.Message}\n");
            }
        }
        
        static void TestDataUriBase64(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 5: Base64 com prefixo data URI");
            try
            {
                // PDF com prefixo data URI
                string pdfContent = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes("%PDF-1.4\n%test content")
                );
                string dataUri = $"data:application/pdf;base64,{pdfContent}";
                
                extension.MssConvertBase64ToPdf(
                    dataUri, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✓ Data URI processado corretamente: {mime} | {ext}");
                Console.WriteLine($"  Tamanho: {binary.Length} bytes\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro: {ex.Message}\n");
            }
        }
        
        static void TestUnsupportedFormat(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 6: Formato não suportado");
            try
            {
                // Bytes aleatórios que não correspondem a nenhum formato
                byte[] randomBytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC };
                string randomBase64 = Convert.ToBase64String(randomBytes);
                
                extension.MssConvertBase64ToPdf(
                    randomBase64, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✗ Deveria ter falhado!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Erro esperado: {ex.Message}\n");
            }
        }
        
        static void TestCadFormat(CssBase64PdfExtension extension)
        {
            Console.WriteLine("Teste 7: Arquivo CAD (DWG)");
            try
            {
                // DWG header simulado (AC1015)
                byte[] dwgBytes = Encoding.ASCII.GetBytes("AC1015DWG_TEST_DATA");
                string dwgBase64 = Convert.ToBase64String(dwgBytes);
                
                extension.MssConvertBase64ToPdf(
                    dwgBase64, 
                    out byte[] binary, 
                    out string mime, 
                    out string ext
                );
                
                Console.WriteLine($"✗ Deveria ter falhado (CAD requer biblioteca)!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Mensagem informativa correta:");
                Console.WriteLine($"  {ex.Message.Substring(0, Math.Min(100, ex.Message.Length))}...\n");
            }
        }
    }
}
