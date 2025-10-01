namespace PekkasParseri.Services
{
    public class PdfKindDetector
    {
        private readonly PdfTextExtractor _pdfText;

        public PdfKindDetector(PdfTextExtractor pdfText) => _pdfText = pdfText;

        public bool LooksLikeTextPdf(string pdfPath)
        {
            var text = _pdfText.ExtractText(pdfPath);
            return !string.IsNullOrWhiteSpace(text) && text.Trim().Length > 100;
        }
    }
}