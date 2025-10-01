using System;
using System.IO;
using System.Text;
using PdfiumViewer;
using Tesseract;

namespace KuittiParseri.Services
{
    /// <summary>
    /// Vastaa PDF-sivujen tekstin poiminnasta OCR:lla (PdfiumViewer + Tesseract).
    /// </summary>
    public class OcrTextExtractor
    {
        private readonly string _tessDataPath;
        private readonly string _language;

        /// <param name="tessDataPath">Polku tessdata-kansioon (esim. "./tessdata")</param>
        /// <param name="language">Kielikoodi (esim. "fin" suomi, "eng" englanti)</param>
        public OcrTextExtractor(string tessDataPath = "./tessdata", string language = "fin")
        {
            _tessDataPath = tessDataPath;
            _language = language;
        }

        /// <summary>
        /// Lukee PDF:n ja palauttaa OCR:lla tunnistetun tekstin.
        /// </summary>
        public string ExtractTextFromPdf(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            var sb = new StringBuilder();

            using var document = PdfDocument.Load(filePath);
            // using var engine = new TesseractEngine(_tessDataPath, _language, EngineMode.Default);
            // ⬇️ Tässä kohtaa alustetaan Tesseract
            using var engine = new TesseractEngine(@"./tessdata", "fin", EngineMode.Default);


            for (int i = 0; i < document.PageCount; i++)
            {
                // Renderöidään PDF-sivu kuvaksi (Image)
                using var image = document.Render(i, 300, 300, true);

                // Muutetaan Image → Bitmap
                using var bitmap = new System.Drawing.Bitmap(image);

                // Tallennetaan bitmap muistiin PNG-muodossa
                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                // Luetaan Pix suoraan muistista
                using var pix = Pix.LoadFromMemory(ms.ToArray());

                // OCR
                using var result = engine.Process(pix);
                sb.AppendLine(result.GetText());
            }

            return sb.ToString();
        }
    }
}