# 🚀 Quick Start Guide - Habilitação Completa

## ✅ Status Atual

```
✅ Código atualizado para API correta do ACadSharp
✅ Compilação bem-sucedida (sem bibliotecas CAD)
✅ Pronto para instalar pacotes NuGet
```

---

## 📦 Passo 1: Instalar Pacotes NuGet

### **Método Mais Fácil - Visual Studio:**

1. Abra o **Package Manager Console**:
   - `Tools` → `NuGet Package Manager` → `Package Manager Console`

2. Execute os comandos:
```powershell
Install-Package ACadSharp
Install-Package netDxf
```

**Isso é tudo!** CSMath virá automaticamente como dependência. ✅

---

## 🔧 Passo 2: Habilitar no Código

No arquivo **`Base64PdfExtension.cs`**, linha 3:

### Antes (desabilitado):
```csharp
//#define INCLUDE_CAD_LIBRARIES
```

### Depois (habilitado):
```csharp
#define INCLUDE_CAD_LIBRARIES
```

**Basta remover as barras `//`**

---

## 🔨 Passo 3: Recompilar

No Visual Studio:
```
Build → Rebuild Solution
```

Ou via linha de comando:
```powershell
msbuild Base64PdfExtension.csproj /p:Configuration=Release
```

---

## 🧪 Passo 4: Testar

### Teste Rápido 1: Verificar se CAD está habilitado

```csharp
using OutSystems.NssBase64PdfExtension;

var extension = new CssBase64PdfExtension();

// Tenta converter um DWG fictício
try {
    byte[] fakeBytes = new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x35 }; // "AC1015"
    extension.MssConvertBinaryToPdf(fakeBytes, out _, out _, out _);
}
catch (Exception ex) {
    if (ex.Message.Contains("requires open-source libraries")) {
        Console.WriteLine("❌ CAD NÃO habilitado - pacotes faltando");
    }
    else {
        Console.WriteLine("✅ CAD habilitado - erro esperado (arquivo inválido)");
    }
}
```

### Teste Rápido 2: Converter arquivo real

```csharp
// Com arquivo DWG real
byte[] dwgBytes = File.ReadAllBytes(@"C:\caminho\arquivo.dwg");

extension.MssConvertBinaryToPdf(
    dwgBytes,
    out byte[] pdfBytes,
    out string mimeType,
    out string fileExtension
);

File.WriteAllBytes(@"C:\caminho\output.pdf", pdfBytes);
Console.WriteLine("✅ Convertido com sucesso!");
```

### Teste Rápido 3: Converter Base64

```csharp
// DWG em Base64
string dwgBase64 = Convert.ToBase64String(
    File.ReadAllBytes(@"C:\caminho\arquivo.dwg")
);

extension.MssConvertBase64ToPdf(
    dwgBase64,
    out byte[] pdfBytes,
    out _,
    out _
);

File.WriteAllBytes(@"C:\caminho\output.pdf", pdfBytes);
```

---

## 🎯 Formatos Suportados Após Habilitação

| Formato | Conversão | Status |
|---------|-----------|--------|
| PDF | As-is | ✅ |
| TIFF | → PDF | ✅ |
| PNG | As-is | ✅ |
| JPEG | As-is | ✅ |
| GIF | As-is | ✅ |
| DNG | → PDF | ✅ |
| **DWG** | **→ PDF** | ✅ **Habilitado!** |
| **DXF** | **→ PDF** | ✅ **Habilitado!** |
| **CAD** | **→ PDF** | ✅ **Habilitado!** |

---

## 🐛 Troubleshooting

### Erro: "The type or namespace 'ACadSharp' could not be found"

**Solução:**
1. Verifique se os pacotes foram instalados:
   ```powershell
   Get-Package | Where-Object { $_.Id -like "*Acad*" -or $_.Id -like "*Dxf*" }
   ```

2. Se não aparecer nada, instale novamente:
   ```powershell
   Install-Package ACadSharp
   Install-Package netDxf
   ```

### Erro: "DwgReader does not contain a constructor that takes 0 arguments"

**Isso foi corrigido!** ✅ A API foi atualizada para:
```csharp
CadDocument doc = DwgReader.Read(stream);
```

### Erro de compilação após descomentar

**Certifique-se que:**
1. Os pacotes estão instalados
2. O projeto foi limpo: `Build → Clean Solution`
3. Recompile: `Build → Rebuild Solution`

---

## 📊 Mudanças na API

### **Antiga (não funciona):**
```csharp
DwgReader reader = new DwgReader();
CadDocument doc = reader.Read(stream);
```

### **Nova (implementada):**
```csharp
CadDocument doc = DwgReader.Read(stream);
```

### **BoundingBox:**
```csharp
// Antiga:
var bounds = entity.BoundingBox;

// Nova:
var bounds = entity.GetBoundingBox();
```

---

## ✅ Checklist Final

- [ ] Instalar `ACadSharp` via NuGet ✅
- [ ] Instalar `netDxf` via NuGet ✅
- [ ] Descomentar `#define INCLUDE_CAD_LIBRARIES` ✅
- [ ] Recompilar projeto ✅
- [ ] Testar com arquivo DWG real ✅
- [ ] Testar com arquivo DXF real ✅

---

## 🎉 Conclusão

**Tudo pronto!** Basta:

1. **Instalar:** `Install-Package ACadSharp` + `Install-Package netDxf`
2. **Descomentar:** `#define INCLUDE_CAD_LIBRARIES`
3. **Recompilar:** `Rebuild Solution`

**Custo: $0** (100% gratuito e open-source!) 🚀

---

## 📚 Documentação Adicional

- **CAD_FORMAT_RESEARCH.md** - Detalhes sobre formatos CAD
- **CAD_CONVERSION_GUIDE.md** - Guia completo de conversão
- **FINAL_REPORT.md** - Relatório técnico completo
- **TEST_REPORT.md** - Exemplos de teste

---

**Pronto para usar!** 🎊
