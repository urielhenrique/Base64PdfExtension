# 🧪 TESTE RÁPIDO - Conversão CAD → PDF

## ✅ PRÉ-REQUISITOS
- ACadSharp e netDxf instalados ✅
- #define INCLUDE_CAD_LIBRARIES habilitado ✅
- Projeto compilado ✅

---

## 🚀 Teste Simples (Copiar e Colar)

### **Opção 1: Console App Simples**

```csharp
using System;
using System.IO;
using OutSystems.NssBase64PdfExtension;

class Program
{
    static void Main()
    {
        var extension = new CssBase64PdfExtension();
        
        // ⬇️ ALTERE O CAMINHO PARA SEU ARQUIVO CAD ⬇️
        string inputFile = @"C:\caminho\seu_arquivo.dwg";
        
        try
        {
            Console.WriteLine($"📖 Lendo: {inputFile}");
            byte[] cadBytes = File.ReadAllBytes(inputFile);
            Console.WriteLine($"✅ Arquivo lido: {cadBytes.Length} bytes");
            
            Console.WriteLine("🔄 Convertendo para PDF...");
            extension.MssConvertBinaryToPdf(
                cadBytes,
                out byte[] pdfBytes,
                out string mimeType,
                out string fileExtension
            );
            
            string outputFile = Path.ChangeExtension(inputFile, ".pdf");
            File.WriteAllBytes(outputFile, pdfBytes);
            
            Console.WriteLine($"✅ SUCESSO! PDF salvo em:");
            Console.WriteLine($"   {outputFile}");
            Console.WriteLine($"   Tamanho: {pdfBytes.Length / 1024} KB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO: {ex.Message}");
        }
        
        Console.ReadKey();
    }
}
```

---

## 📝 Teste via OutSystems (Server Action)

### **Action: ConvertCadToPdf**

**Input Parameters:**
- `FileContent` (Binary Data)

**Output Parameters:**
- `PdfContent` (Binary Data)
- `Success` (Boolean)
- `ErrorMessage` (Text)

```csharp
public void MssConvertCadToPdf(byte[] ssFileContent, out byte[] ssPdfContent, out bool ssSuccess, out string ssErrorMessage)
{
    ssPdfContent = new byte[] {};
    ssSuccess = false;
    ssErrorMessage = "";
    
    try
    {
        var extension = new CssBase64PdfExtension();
        
        extension.MssConvertBinaryToPdf(
            ssFileContent,
            out ssPdfContent,
            out string mimeType,
            out string fileExtension
        );
        
        ssSuccess = true;
    }
    catch (Exception ex)
    {
        ssErrorMessage = ex.Message;
        ssSuccess = false;
    }
}
```

---

## 🧪 Teste Completo com Logging

```csharp
using System;
using System.IO;
using System.Diagnostics;
using OutSystems.NssBase64PdfExtension;

class CadTester
{
    static void Main()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║  🧪 TESTE CONVERSÃO CAD → PDF        ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();
        
        // Arquivo de teste
        Console.Write("📁 Digite o caminho do arquivo CAD: ");
        string filePath = Console.ReadLine();
        
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("❌ Arquivo não encontrado!");
            Console.ReadKey();
            return;
        }
        
        TestConversion(filePath);
        
        Console.WriteLine();
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
    
    static void TestConversion(string inputPath)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var extension = new CssBase64PdfExtension();
            
            // Lê arquivo
            Console.WriteLine("📖 Lendo arquivo...");
            byte[] cadBytes = File.ReadAllBytes(inputPath);
            Console.WriteLine($"   ✅ {cadBytes.Length:N0} bytes");
            Console.WriteLine();
            
            // Detecta formato
            string format = DetectFormat(cadBytes);
            Console.WriteLine($"🔍 Formato detectado: {format}");
            Console.WriteLine();
            
            // Converte
            Console.WriteLine("🔄 Convertendo para PDF...");
            extension.MssConvertBinaryToPdf(
                cadBytes,
                out byte[] pdfBytes,
                out string mimeType,
                out string fileExtension
            );
            
            stopwatch.Stop();
            
            // Salva resultado
            string outputPath = Path.ChangeExtension(inputPath, ".pdf");
            File.WriteAllBytes(outputPath, pdfBytes);
            
            // Relatório
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  ✅ CONVERSÃO BEM-SUCEDIDA!           ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"⏱️  Tempo: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"📄 Entrada: {cadBytes.Length / 1024.0:F2} KB");
            Console.WriteLine($"📄 Saída: {pdfBytes.Length / 1024.0:F2} KB");
            Console.WriteLine($"🎯 Mime: {mimeType}");
            Console.WriteLine($"📁 PDF: {outputPath}");
            Console.WriteLine();
            
            // Abrir PDF
            Console.Write("❓ Abrir PDF? (S/N): ");
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                Console.WriteLine();
                Process.Start(outputPath);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  ❌ ERRO NA CONVERSÃO                 ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Mensagem: {ex.Message}");
            Console.WriteLine();
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Detalhes: {ex.InnerException.Message}");
            }
        }
    }
    
    static string DetectFormat(byte[] bytes)
    {
        if (bytes.Length < 4) return "Unknown";
        
        // DWG
        if (bytes[0] == 0x41 && bytes[1] == 0x43)
            return $"DWG ({System.Text.Encoding.ASCII.GetString(bytes, 0, 6)})";
        
        // DXF
        string header = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(20, bytes.Length));
        if (header.Contains("SECTION"))
            return "DXF";
        
        // PDF
        if (bytes[0] == 0x25 && bytes[1] == 0x50)
            return "PDF";
        
        return "Unknown";
    }
}
```

---

## 🌐 Sites com Arquivos CAD Grátis para Teste

1. **Autodesk Samples**
   - https://knowledge.autodesk.com/support/autocad/downloads
   - Arquivos oficiais AutoCAD

2. **CAD-Blocks.net**
   - https://www.cad-blocks.net
   - DWG gratuitos

3. **FreecadFiles.com**
   - https://www.freecadfiles.com
   - Biblioteca gratuita

4. **GrabCAD**
   - https://grabcad.com/library
   - Comunidade CAD

---

## 🔧 Se Algo Der Errado

### Erro: "Failed to convert DWG to PDF"
**Possíveis causas:**
- Versão DWG muito antiga (< R13)
- Arquivo corrompido
- Versão muito nova (> 2018)

**Solução:**
- Abrir no AutoCAD e "Save As" versão 2018 ou anterior
- Testar com arquivo DXF

### Erro: "The type or namespace 'ACadSharp' could not be found"
**Solução:**
- Verificar se #define INCLUDE_CAD_LIBRARIES está descomentado
- Rebuild Solution

### PDF Vazio ou Incorreto
**Causa:**
- Renderização básica implementada (apenas linhas)

**Solução:**
- Expandir métodos `RenderCadEntities` e `RenderNetDxfEntities`
- Adicionar suporte a círculos, arcos, textos

---

## 📞 Resultado Esperado

```
╔════════════════════════════════════════╗
║  ✅ CONVERSÃO BEM-SUCEDIDA!           ║
╚════════════════════════════════════════╝

⏱️  Tempo: 1234ms
📄 Entrada: 156.78 KB
📄 Saída: 89.45 KB
🎯 Mime: application/pdf
📁 PDF: C:\caminho\arquivo_converted.pdf
```

---

## ✅ Checklist de Teste

- [ ] Testar com arquivo DWG
- [ ] Testar com arquivo DXF
- [ ] Testar com arquivo .CAD
- [ ] Verificar se PDF foi gerado
- [ ] Abrir PDF e validar conteúdo
- [ ] Testar com arquivos grandes (> 5MB)
- [ ] Testar com Base64 input

---

**🎉 Pronto para testar!**

Cole o código em um Console App ou use o arquivo `TestCadConversion.cs` que criei.
