using System;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace KuittiParseri.Services
{
    /// <summary>
    /// Vastaa natiivien PDF-tiedostojen tekstin poiminnasta iTextSharpilla.
    /// </summary>
    public class ITextTextExtractor
    {
        public string ExtractTextFromPdf(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            var sb = new StringBuilder();

            using (var reader = new PdfReader(filePath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);

                    // Varmistetaan UTF-8
                    pageText = Encoding.UTF8.GetString(
                        Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(pageText))
                    );

                    sb.AppendLine(pageText);
                }
            }

            return sb.ToString();
        }
    }
}