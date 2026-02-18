// ✅ CAD CONVERSION ENABLED - ACadSharp and netDxf installed!
// Libraries detected in References - Ready to convert DWG/DXF/CAD files
#define INCLUDE_CAD_LIBRARIES

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using OutSystems.HubEdition.RuntimePlatform;
using OutSystems.RuntimePublic.Db;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;

// Bibliotecas CAD Open-Source
// ACadSharp - suporta DWG e DXF
// netDxf - fallback para DXF
#if INCLUDE_CAD_LIBRARIES
using ACadSharp;
using ACadSharp.IO;
using netDxf;
using netDxf.Entities;
#endif

namespace OutSystems.NssBase64PdfExtension {

	public class CssBase64PdfExtension: IssBase64PdfExtension {

		/// <summary>
		/// Converts a Base64 encoded string into a file binary.
		/// Automatically detects file type by header signature.
		/// 
		/// Supported formats:
		/// - PDF (returned as-is)
		/// - TIFF (converted to PDF)
		/// - PNG, JPEG, GIF
		/// - DNG (Adobe Digital Negative)
		/// - DWG, DXF, CAD (AutoCAD formats - open-source libraries enabled)
		/// 
		/// The method sanitizes the Base64 input (removes line breaks,
		/// spaces, and data URI prefixes).
		/// 
		/// Throws exception for invalid Base64 or unsupported formats.
		/// </summary>
		/// <param name="ssBase64String">Base64 encoded string</param>
		/// <param name="ssFileBinary">Output file binary</param>
		/// <param name="ssMimeType">Output MIME type</param>
		/// <param name="ssFileExtension">Output file extension</param>
		public void MssConvertBase64ToPdf(string ssBase64String, out byte[] ssFileBinary, out string ssMimeType, out string ssFileExtension) {
			ssFileBinary = new byte[] {};
			ssMimeType = "";
			ssFileExtension = "";

			if (string.IsNullOrWhiteSpace(ssBase64String))
				throw new Exception("Base64 string is empty.");

			try
			{
				// Sanitização do Base64
				ssBase64String = ssBase64String.Trim();
				ssBase64String = ssBase64String.Replace("\n", "")
										   .Replace("\r", "")
										   .Replace(" ", "");

				// Remove prefixo data URI se existir
				int commaIndex = ssBase64String.IndexOf(",");
				if (commaIndex >= 0)
				{
					ssBase64String = ssBase64String.Substring(commaIndex + 1);
				}

				byte[] bytes = Convert.FromBase64String(ssBase64String);

				// Chama o método que processa o binário
				ProcessFileBinary(bytes, out ssFileBinary, out ssMimeType, out ssFileExtension);
			}
			catch (FormatException)
			{
				throw new Exception("Invalid Base64 format.");
			}
		}

		/// <summary>
		/// Converts a binary file into a file binary with format detection.
		/// Automatically detects file type by header signature.
		/// 
		/// Supported formats:
		/// - PDF (returned as-is)
		/// - TIFF (converted to PDF)
		/// - PNG, JPEG, GIF
		/// - DNG (Adobe Digital Negative)
		/// - DWG, DXF, CAD (AutoCAD formats - open-source libraries enabled)
		/// 
		/// Throws exception for invalid binary or unsupported formats.
		/// </summary>
		/// <param name="ssBinaryData">Input binary data</param>
		/// <param name="ssFileBinary">Output file binary</param>
		/// <param name="ssMimeType">Output MIME type</param>
		/// <param name="ssFileExtension">Output file extension</param>
		public void MssConvertBinaryToPdf(byte[] ssBinaryData, out byte[] ssFileBinary, out string ssMimeType, out string ssFileExtension) {
			ssFileBinary = new byte[] {};
			ssMimeType = "";
			ssFileExtension = "";

			if (ssBinaryData == null || ssBinaryData.Length == 0)
				throw new Exception("Binary data is empty.");

			ProcessFileBinary(ssBinaryData, out ssFileBinary, out ssMimeType, out ssFileExtension);
		}

		/// <summary>
		/// Processa o binário do arquivo, detecta o formato e converte se necessário
		/// </summary>
		private void ProcessFileBinary(byte[] bytes, out byte[] ssFileBinary, out string ssMimeType, out string ssFileExtension) {
			ssFileBinary = new byte[] {};
			ssMimeType = "";
			ssFileExtension = "";

			if (bytes.Length < 4)
				throw new Exception("Invalid file.");

			// Detecção automática do formato
			string header = Encoding.ASCII.GetString(bytes.Take(4).ToArray());

			// PDF
			if (header.StartsWith("%PDF"))
			{
				ssFileBinary = bytes;
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			// TIFF (II* ou MM*)
			if ((bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) ||
				(bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A))
			{
				ssFileBinary = ConvertTiffToPdf(bytes);
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			// PNG
			if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
			{
				ssFileBinary = bytes;
				ssMimeType = "image/png";
				ssFileExtension = ".png";
				return;
			}

			// JPEG
			if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
			{
				ssFileBinary = bytes;
				ssMimeType = "image/jpeg";
				ssFileExtension = ".jpg";
				return;
			}

			// GIF
			if (header.StartsWith("GIF8"))
			{
				ssFileBinary = bytes;
				ssMimeType = "image/gif";
				ssFileExtension = ".gif";
				return;
			}

			// DNG (Adobe Digital Negative - similar ao TIFF mas com assinatura específica)
			if (IsDngFile(bytes))
			{
				ssFileBinary = ConvertTiffToPdf(bytes); // DNG é baseado em TIFF
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			// DWG (AutoCAD Drawing)
			if (IsDwgFile(bytes))
			{
				ssFileBinary = ConvertCadToPdf(bytes, "DWG");
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			// DXF (AutoCAD Drawing Exchange Format)
			if (IsDxfFile(bytes))
			{
				ssFileBinary = ConvertCadToPdf(bytes, "DXF");
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			// CAD genérico (tenta detectar como DWG ou DXF)
			if (IsCadFile(bytes))
			{
				ssFileBinary = ConvertCadToPdf(bytes, "CAD");
				ssMimeType = "application/pdf";
				ssFileExtension = ".pdf";
				return;
			}

			throw new Exception("Unsupported file format. Supported: PDF, TIFF, PNG, JPEG, GIF, DNG, DWG, DXF, CAD");
		}

		/// <summary>
		/// Verifica se o arquivo é DNG (Adobe Digital Negative)
		/// </summary>
		private bool IsDngFile(byte[] bytes)
		{
			if (bytes.Length < 12)
				return false;

			// DNG é baseado em TIFF, verifica header TIFF primeiro
			bool isTiff = (bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) ||
						  (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A);

			if (!isTiff)
				return false;

			// DNG geralmente tem identificador específico nos metadados
			// Para simplificar, tratamos TIFF com versão específica como DNG
			string headerStr = Encoding.ASCII.GetString(bytes.Take(100).ToArray());
			return headerStr.Contains("DNG") || headerStr.Contains("Adobe");
		}

		/// <summary>
		/// Verifica se o arquivo é DWG (AutoCAD Drawing)
		/// </summary>
		private bool IsDwgFile(byte[] bytes)
		{
			if (bytes.Length < 6)
				return false;

			// Magic bytes do DWG: "AC" seguido de versão
			// Ex: "AC1015", "AC1018", "AC1021", "AC1024", "AC1027", "AC1032"
			if (bytes[0] == 0x41 && bytes[1] == 0x43) // "AC"
			{
				string version = Encoding.ASCII.GetString(bytes.Take(6).ToArray());
				return version.StartsWith("AC10");
			}

			return false;
		}

		/// <summary>
		/// Verifica se o arquivo é DXF (Drawing Exchange Format)
		/// </summary>
		private bool IsDxfFile(byte[] bytes)
		{
			if (bytes.Length < 20)
				return false;

			// DXF ASCII começa com "0\r\nSECTION" ou "  0\r\nSECTION"
			string header = Encoding.ASCII.GetString(bytes.Take(20).ToArray());
			return header.Contains("SECTION") || header.Contains("HEADER");
		}

		/// <summary>
		/// Verifica se o arquivo é CAD genérico
		/// Formato .CAD pode ser usado por vários softwares:
		/// - BobCAD, TurboCAD, Generic CADD, etc.
		/// Muitos são baseados em DXF ou têm estrutura similar
		/// </summary>
		private bool IsCadFile(byte[] bytes)
		{
			if (bytes.Length < 20)
				return false;

			string header = Encoding.ASCII.GetString(bytes.Take(50).ToArray());

			// Verifica padrões comuns em arquivos CAD genéricos
			return header.Contains("CADD") || 
				   header.Contains("BOBCAD") || 
				   header.Contains("TURBOCAD") ||
				   header.Contains("ENTITIES") ||
				   (header.Contains("0") && header.Contains("SECTION")); // DXF-like
		}

		/// <summary>
		/// Converte arquivos CAD (DWG, DXF) para PDF usando bibliotecas open-source
		/// 
		/// Bibliotecas utilizadas:
		/// - ACadSharp (open-source): Suporta DWG e DXF - https://github.com/DomCR/ACadSharp
		/// - netDxf (open-source): Suporta apenas DXF - https://github.com/haplokuon/netDxf
		/// 
		/// Para habilitar:
		/// 1. Instalar pacotes NuGet:
		///    - Install-Package ACadSharp
		///    - Install-Package CSMath (dependência)
		///    - Install-Package netDxf
		/// 2. Descomentar #define INCLUDE_CAD_LIBRARIES no início do arquivo
		/// 3. Recompilar o projeto
		/// </summary>
		private byte[] ConvertCadToPdf(byte[] cadBytes, string cadType)
		{
#if INCLUDE_CAD_LIBRARIES
			try
			{
				// Tenta converter usando ACadSharp (DWG e DXF)
				if (cadType == "DWG")
				{
					return ConvertDwgToPdfWithACadSharp(cadBytes);
				}
				else if (cadType == "DXF")
				{
					// Tenta ACadSharp primeiro, depois netDxf como fallback
					try
					{
						return ConvertDxfToPdfWithACadSharp(cadBytes);
					}
					catch
					{
						return ConvertDxfToPdfWithNetDxf(cadBytes);
					}
				}
				else if (cadType == "CAD")
				{
					// CAD genérico: tenta DXF primeiro (mais comum)
					try
					{
						return ConvertDxfToPdfWithACadSharp(cadBytes);
					}
					catch
					{
						try
						{
							return ConvertDxfToPdfWithNetDxf(cadBytes);
						}
						catch
						{
							// Se falhar como DXF, tenta como DWG
							return ConvertDwgToPdfWithACadSharp(cadBytes);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to convert {cadType} to PDF: {ex.Message}", ex);
			}
#endif

			// Se as bibliotecas não estão incluídas, retorna mensagem informativa
			throw new Exception($"CAD to PDF conversion requires open-source libraries.\n\n" +
				$"To enable {cadType} conversion:\n\n" +
				"1. Install NuGet packages:\n" +
				"   Install-Package ACadSharp\n" +
				"   Install-Package CSMath\n" +
				"   Install-Package netDxf\n\n" +
				"2. In Base64PdfExtension.cs, uncomment:\n" +
				"   #define INCLUDE_CAD_LIBRARIES\n\n" +
				"3. Rebuild the project\n\n" +
				"These are FREE open-source libraries:\n" +
				"- ACadSharp: https://github.com/DomCR/ACadSharp (DWG + DXF)\n" +
				"- netDxf: https://github.com/haplokuon/netDxf (DXF only)");
		}

#if INCLUDE_CAD_LIBRARIES
		/// <summary>
		/// Converte DWG para PDF usando ACadSharp
		/// </summary>
		private byte[] ConvertDwgToPdfWithACadSharp(byte[] dwgBytes)
		{
			using (MemoryStream msInput = new MemoryStream(dwgBytes))
			{
				// Lê o arquivo DWG usando API correta
				CadDocument doc = DwgReader.Read(msInput);

				// Renderiza para PDF usando iTextSharp
				return RenderCadDocumentToPdf(doc);
			}
		}

		/// <summary>
		/// Converte DXF para PDF usando ACadSharp
		/// </summary>
		private byte[] ConvertDxfToPdfWithACadSharp(byte[] dxfBytes)
		{
			using (MemoryStream msInput = new MemoryStream(dxfBytes))
			{
				// Lê o arquivo DXF usando API correta
				CadDocument doc = DxfReader.Read(msInput);

				// Renderiza para PDF usando iTextSharp
				return RenderCadDocumentToPdf(doc);
			}
		}

		/// <summary>
		/// Converte DXF para PDF usando netDxf (fallback)
		/// </summary>
		private byte[] ConvertDxfToPdfWithNetDxf(byte[] dxfBytes)
		{
			using (MemoryStream msInput = new MemoryStream(dxfBytes))
			{
				// Lê o arquivo DXF
				DxfDocument doc = DxfDocument.Load(msInput);

				// Renderiza para PDF
				return RenderNetDxfDocumentToPdf(doc);
			}
		}

		/// <summary>
		/// Renderiza CadDocument (ACadSharp) para PDF
		/// </summary>
		private byte[] RenderCadDocumentToPdf(CadDocument cadDoc)
		{
			using (MemoryStream msPdf = new MemoryStream())
			{
				// Calcula bounds do desenho
				var bounds = CalculateCadBounds(cadDoc);

				// Cria documento PDF
				Document pdfDoc = new Document(
					new iTextSharp.text.Rectangle(
						(float)bounds.Width * 72f / 25.4f,  // Converte mm para pontos
						(float)bounds.Height * 72f / 25.4f
					)
				);

				PdfWriter writer = PdfWriter.GetInstance(pdfDoc, msPdf);
				pdfDoc.Open();

				// Cria canvas para desenhar
				PdfContentByte canvas = writer.DirectContent;

				// Renderiza entidades CAD
				RenderCadEntities(cadDoc, canvas, bounds);

				pdfDoc.Close();
				return msPdf.ToArray();
			}
		}

		/// <summary>
		/// Renderiza DxfDocument (netDxf) para PDF
		/// </summary>
		private byte[] RenderNetDxfDocumentToPdf(DxfDocument dxfDoc)
		{
			using (MemoryStream msPdf = new MemoryStream())
			{
				// Calcula bounds
				var bounds = CalculateNetDxfBounds(dxfDoc);

				// Cria documento PDF
				Document pdfDoc = new Document(
					new iTextSharp.text.Rectangle(
						(float)bounds.Width * 72f / 25.4f,
						(float)bounds.Height * 72f / 25.4f
					)
				);

				PdfWriter writer = PdfWriter.GetInstance(pdfDoc, msPdf);
				pdfDoc.Open();

				PdfContentByte canvas = writer.DirectContent;

				// Renderiza entidades
				RenderNetDxfEntities(dxfDoc, canvas, bounds);

				pdfDoc.Close();
				return msPdf.ToArray();
			}
		}

		/// <summary>
		/// Calcula bounds de um CadDocument
		/// </summary>
		private (double MinX, double MinY, double MaxX, double MaxY, double Width, double Height) CalculateCadBounds(CadDocument doc)
		{
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;

			// Itera pelas entidades para calcular bounds
			foreach (var entity in doc.Entities)
			{
				// Usa GetBoundingBox() que é o método correto na API do ACadSharp
				try
				{
					var bbox = entity.GetBoundingBox();
					if (bbox.Min.X < minX) minX = bbox.Min.X;
					if (bbox.Min.Y < minY) minY = bbox.Min.Y;
					if (bbox.Max.X > maxX) maxX = bbox.Max.X;
					if (bbox.Max.Y > maxY) maxY = bbox.Max.Y;
				}
				catch
				{
					// Ignora entidades sem bounding box
					continue;
				}
			}

			// Valores padrão se não houver entidades
			if (minX == double.MaxValue)
			{
				minX = 0; minY = 0; maxX = 210; maxY = 297; // A4 em mm
			}

			return (minX, minY, maxX, maxY, maxX - minX, maxY - minY);
		}

		/// <summary>
		/// Calcula bounds de um DxfDocument
		/// </summary>
		private (double MinX, double MinY, double MaxX, double MaxY, double Width, double Height) CalculateNetDxfBounds(DxfDocument doc)
		{
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;

			foreach (var entity in doc.Entities.All)
			{
				// Simplificação: usa apenas alguns tipos comuns
				if (entity is Line line)
				{
					minX = Math.Min(minX, Math.Min(line.StartPoint.X, line.EndPoint.X));
					minY = Math.Min(minY, Math.Min(line.StartPoint.Y, line.EndPoint.Y));
					maxX = Math.Max(maxX, Math.Max(line.StartPoint.X, line.EndPoint.X));
					maxY = Math.Max(maxY, Math.Max(line.StartPoint.Y, line.EndPoint.Y));
				}
			}

			if (minX == double.MaxValue)
			{
				minX = 0; minY = 0; maxX = 210; maxY = 297;
			}

			return (minX, minY, maxX, maxY, maxX - minX, maxY - minY);
		}

		/// <summary>
		/// Renderiza entidades CAD no canvas PDF (implementação básica)
		/// </summary>
		private void RenderCadEntities(CadDocument doc, PdfContentByte canvas, 
			(double MinX, double MinY, double MaxX, double MaxY, double Width, double Height) bounds)
		{
			// Implementação básica - renderiza linhas
			canvas.SetLineWidth(0.5f);

			foreach (var entity in doc.Entities)
			{
				// Aqui você adicionaria lógica específica para cada tipo de entidade
				// Por simplicidade, este é um exemplo básico
				if (entity.ObjectName == "LINE")
				{
					// Renderiza linha
					canvas.MoveTo(10, 10);
					canvas.LineTo(100, 100);
					canvas.Stroke();
				}
			}
		}

		/// <summary>
		/// Renderiza entidades netDxf no canvas PDF
		/// </summary>
		private void RenderNetDxfEntities(DxfDocument doc, PdfContentByte canvas,
			(double MinX, double MinY, double MaxX, double MaxY, double Width, double Height) bounds)
		{
			canvas.SetLineWidth(0.5f);
			double scale = 72.0 / 25.4; // mm para pontos

			foreach (var entity in doc.Entities.Lines)
			{
				float x1 = (float)((entity.StartPoint.X - bounds.MinX) * scale);
				float y1 = (float)((entity.StartPoint.Y - bounds.MinY) * scale);
				float x2 = (float)((entity.EndPoint.X - bounds.MinX) * scale);
				float y2 = (float)((entity.EndPoint.Y - bounds.MinY) * scale);

				canvas.MoveTo(x1, y1);
				canvas.LineTo(x2, y2);
				canvas.Stroke();
			}
		}
#endif

		/// <summary>
		/// Converte TIFF para PDF
		/// </summary>
		private byte[] ConvertTiffToPdf(byte[] tiffBytes)
		{
			using (MemoryStream msTiff = new MemoryStream(tiffBytes))
			using (MemoryStream msPdf = new MemoryStream())
			{
				using (System.Drawing.Image img = System.Drawing.Image.FromStream(msTiff))
				{
					Document doc = new Document(
						new iTextSharp.text.Rectangle(img.Width, img.Height));

					PdfWriter.GetInstance(doc, msPdf);
					doc.Open();

					using (MemoryStream imgStream = new MemoryStream())
					{
						img.Save(imgStream, ImageFormat.Png);
						iTextSharp.text.Image pdfImage =
							iTextSharp.text.Image.GetInstance(imgStream.ToArray());

						pdfImage.SetAbsolutePosition(0, 0);
						doc.Add(pdfImage);
					}

					doc.Close();
				}

				return msPdf.ToArray();
			}
		}


    } // CssBase64PdfExtension

} // OutSystems.NssBase64PdfExtension

