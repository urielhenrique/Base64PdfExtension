using System;
using System.Collections;
using System.Data;
using OutSystems.HubEdition.RuntimePlatform;

namespace OutSystems.NssBase64PdfExtension {

	public interface IssBase64PdfExtension {

		/// <summary>
		/// Converts a Base64 string into a file binary with automatic format detection.
		/// 
		/// This action receives a Base64 encoded string and automatically detects the file type based on its binary signature.
		/// 
		/// Supported formats:
		/// 
		/// PDF (returned as-is)
		/// 
		/// TIFF (converted to PDF)
		/// 
		/// PNG
		/// 
		/// JPEG
		/// 
		/// GIF
		/// 
		/// If the file is a TIFF image, it will be converted into a PDF document.
		/// 
		/// The action also sanitizes the input Base64 string by:
		/// 
		/// Removing line breaks
		/// 
		/// Removing spaces
		/// 
		/// Removing data URI prefixes (e.g. data:application/pdf;base64,)
		/// 
		/// Throws an exception if:
		/// 
		/// Base64 is empty
		/// 
		/// Base64 format is invalid
		/// 
		/// File format is unsupported
		/// Performance Notes
		/// 
		/// Memory-based processing
		/// 
		/// No temporary files created
		/// 
		/// Suitable for server-side document workflows
		/// </summary>
		/// <param name="ssBase64String">Base64 encoded string representing the file.
		/// The string may contain data URI prefixes and line breaks — they will be automatically sanitized.</param>
		/// <param name="ssFileBinary">Output file binary ready for download or storage.</param>
		/// <param name="ssMimeType">Detected MIME type of the file (e.g. application/pdf, image/png).</param>
		/// <param name="ssFileExtension">Suggested file extension based on detected file format (.pdf, .png, .jpg, .gif).</param>
		void MssConvertBase64ToPdf(string ssBase64String, out byte[] ssFileBinary, out string ssMimeType, out string ssFileExtension);

	} // IssBase64PdfExtension

} // OutSystems.NssBase64PdfExtension
