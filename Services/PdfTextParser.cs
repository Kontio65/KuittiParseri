using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KuittiParseri.Services
{
    /// <summary>
    /// Vastaa OCR-tekstin jäsentämisestä rakenteiseksi dataksi.
    /// </summary>
    public class PdfTextParser
    {
        // Palkkajakso: esim. 18102021-31102021
        private static readonly Regex PayPeriodRegex = new(@"\b\d{6,8}-\d{6,8}\b");

        // Nimi: yksinkertainen heuristiikka (iso alkukirjain + sukunimi)
        private static readonly Regex NameRegex = new(@"\b[A-ZÅÄÖ][a-zåäö]+ [A-ZÅÄÖ][a-zåäö]+\b");

        // Työ- ja vähennyskoodit: esim. 100, 200, 1700 jne.
        private static readonly Regex WorkCodeRegex = new(@"(?<code>\d{2,4})\s+(?<desc>[A-Za-zÅÄÖåäö\s\-]+)\s+(?<hours>\d+[.,]?\d*)");

        public string ExtractPayPeriod(string text)
        {
            var match = PayPeriodRegex.Match(text);
            return match.Success ? match.Value : string.Empty;
        }

        public string ExtractName(string text)
        {
            var match = NameRegex.Match(text);
            return match.Success ? match.Value : string.Empty;
        }

        public double ExtractHours(string text)
        {
            // Etsitään kaikki tunnit ja summataan
            var matches = WorkCodeRegex.Matches(text);
            double total = 0;
            foreach (Match m in matches)
            {
                if (double.TryParse(m.Groups["hours"].Value.Replace(",", "."), out double h))
                    total += h;
            }
            return total;
        }

        public List<(string Code, string Description, double Hours)> ExtractWorkCodeEntriesFromLines(string text)
        {
            var results = new List<(string, string, double)>();
            var matches = WorkCodeRegex.Matches(text);

            foreach (Match m in matches)
            {
                string code = m.Groups["code"].Value.Trim();
                string desc = m.Groups["desc"].Value.Trim();
                string hoursStr = m.Groups["hours"].Value.Replace(",", ".");
                double.TryParse(hoursStr, out double hours);

                results.Add((code, desc, hours));
            }

            return results;
        }
    }
}