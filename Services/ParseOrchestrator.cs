using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KuittiParseri.Models;
using iTextSharp;

namespace KuittiParseri.Services
{
    public class ParseOrchestrator
    {
        private readonly ITextTextExtractor _iTextExtractor;
        private readonly OcrTextExtractor _ocrExtractor;
        private readonly PdfTextParser _parser;

        public ParseOrchestrator()
        {
            _iTextExtractor = new ITextTextExtractor();
            _ocrExtractor = new OcrTextExtractor("./tessdata", "fin");
            _parser = new PdfTextParser();
        }

        public FileResult ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Tiedostoa ei löydy", filePath);

            string text = string.Empty;
            string mode = "unknown";

            // 1. Yritetään ensin iTextSharpilla (natiivi PDF)
            try
            {
                text = _iTextExtractor.ExtractTextFromPdf(filePath);
                if (!string.IsNullOrWhiteSpace(text))
                    mode = "iTextSharp";
            }
            catch
            {
                text = string.Empty;
            }

            // 2. Jos tekstiä ei löytynyt, käytetään OCR:ää
            if (string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    text = _ocrExtractor.ExtractTextFromPdf(filePath);
                    mode = "OCR";
                }
                catch (Exception ex)
                {
                    return new FileResult
                    {
                        FileName = Path.GetFileName(filePath),
                        Error = $"OCR epäonnistui: {ex.Message}",
                        WorkCodes = new List<(string Code, string Description, double Hours)>()
                    };
                }
            }

            // Debug-tiedostot
            try
            {
                File.WriteAllText("debug_pdf.txt", text);
                File.WriteAllText("debug_mode.txt", $"Käytetty polku: {mode}");
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                File.WriteAllLines("debug_lines.txt", lines);
            }
            catch { /* ei kaadeta debug-virheistä */ }

            // ... jatkuu kuten ennen

            // Parsitaan tiedot
            var workCodes = _parser.ExtractWorkCodeEntriesFromLines(text)
                            ?? new List<(string Code, string Description, double Hours)>();

            string summary = string.Join("; ", workCodes.Select(w => $"{w.Code}:{w.Hours:0.##}"));

            return new FileResult
            {
                FileName = Path.GetFileName(filePath),
                Code = TryExtractCodeFallback(text),
                Name = _parser.ExtractName(text),
                Hours = _parser.ExtractHours(text),
                PayPeriod = _parser.ExtractPayPeriod(text),
                Error = string.IsNullOrWhiteSpace(text) ? "Ei tekstiä" : string.Empty,
                WorkCodesSummary = summary,
                WorkCodes = workCodes
            };
        }

        private string TryExtractCodeFallback(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var tokens = text.Split(new[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (t.Length >= 3 && t.Any(char.IsLetter) && t.Any(char.IsDigit))
                {
                    return t;
                }
            }
            return string.Empty;
        }
    }
}