using System;
using System.IO;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PekkasParseri.Services
{
    public class PdfTextExtractor
    {
        public string ExtractText(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
                throw new FileNotFoundException("Tiedostoa ei löytynyt", pdfPath);

            var sb = new StringBuilder();

            try
            {
                using (var doc = PdfDocument.Open(pdfPath))
                {
                    foreach (var page in doc.GetPages())
                    {
                        // page.Text toimii suoraan, kun Content-nimialue on mukana
                        sb.AppendLine(page.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                // Voit halutessasi logittaa tämän; nyt palautetaan selkeä virheviesti
                return $"[PdfTextExtractor] Virhe PDF:n luvussa: {ex.Message}";
            }

            return sb.ToString();
        }
    }
}