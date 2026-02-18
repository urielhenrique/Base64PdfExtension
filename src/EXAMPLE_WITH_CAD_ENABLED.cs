// ============================================================================
// QUICK START - HABILITAR CONVERSÃO CAD
// ============================================================================
//
// Para habilitar conversão DWG/DXF → PDF:
//
// 1️⃣ DESCOMENTAR a linha abaixo:
//
//    //#define INCLUDE_CAD_LIBRARIES  ← REMOVER AS //
//
// 2️⃣ SALVAR o arquivo
//
// 3️⃣ RECOMPILAR (Build > Rebuild Solution)
//
// ============================================================================

// ⬇️ DESCOMENTAR ESTA LINHA PARA HABILITAR CAD ⬇️
//#define INCLUDE_CAD_LIBRARIES

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

// Bibliotecas CAD Open-Source (ativadas com #define INCLUDE_CAD_LIBRARIES)
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
		/// - DWG, DXF (AutoCAD - requires INCLUDE_CAD_LIBRARIES)
		/// 
		/// The method sanitizes the Base64 input (removes line breaks,
		/// spaces, and data URI prefixes).
		/// 
		/// Throws exception for invalid Base64 or unsupported formats.
		/// </summary>
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
		/// - DWG, DXF (AutoCAD - requires INCLUDE_CAD_LIBRARIES)
		/// 
		/// Throws exception for invalid binary or unsupported formats.
		/// </summary>
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

			throw new Exception("Unsupported file format. Supported: PDF, TIFF, PNG, JPEG, GIF, DNG, DWG, DXF");
		}

		// ... (resto do código permanece igual)

		/// <summary>
		/// Status da conversão CAD
		/// </summary>
		public string GetCadConversionStatus()
		{
#if INCLUDE_CAD_LIBRARIES
			return "✅ CAD Conversion ENABLED (ACadSharp + netDxf)";
#else
			return "⚠️ CAD Conversion DISABLED - Uncomment #define INCLUDE_CAD_LIBRARIES to enable";
#endif
		}
	}
}
