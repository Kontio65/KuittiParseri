using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace KuittiParseri.Services
{
    /// <summary>
    /// Vastaa OCR-tekstin jäsentämisestä rakenteiseksi dataksi.
    /// Parsii myös taulukon rivit: koodi, selite, määrä, a-hinta, rahamäärä.
    /// </summary>
    public class PdfTextParser
    {
        // Hyväksytään sekä tiivis muoto ilman pisteitä että .-muoto (09.08.2021 - 22.08.2021)
        private static readonly Regex PayPeriodRegex = new(@"\b\d{2}\.\d{2}\.\d{4}\s*-\s*\d{2}\.\d{2}\.\d{4}\b|\b\d{6,8}-\d{6,8}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Maksupäivä erikseen (yksittäinen päivämäärä, käytetään jos palkkajaksoa ei löydy samalla rivillä)
        private static readonly Regex PayDateRegex = new(@"\b\d{2}\.\d{2}\.\d{4}\b", RegexOptions.Compiled);

        // Nimi: vähintään etunimi + sukunimi (voi olla useampi osa)
        private static readonly Regex NameRegex = new(@"\b[A-ZÅÄÖ][a-zåäö]+(?: [A-ZÅÄÖ][a-zåäö]+)+\b", RegexOptions.Compiled);

        // Koodin tunnistus rivin alusta (esim. "1000 Tuntipalkka ...")
        private static readonly Regex LineWithCodeRegex = new(@"^\s*(?<code>\d{2,5})\s+(?<rest>.+)$", RegexOptions.Compiled);

        // Numerot: sallitaan tuhansierotin (välilyönti tai NBSP) ja desimaalierotin pilkku tai piste
        private static readonly Regex NumberTokenRegex = new(@"\d{1,3}(?:[ \u00A0]\d{3})*(?:[.,]\d+)?|\d+[.,]?\d*", RegexOptions.Compiled);

        // Otsikon tunnistus (rivi, jossa on SELITE ja MÄÄRÄ tms.)
        private static readonly string[] TableHeaderKeywords = new[] { "SELITE", "MÄÄRÄ", "A-HINTA", "RAHAMÄÄRÄ", "PALKK" };

        public string ExtractPayPeriod(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = NormalizeTextStart(text);

            var match = PayPeriodRegex.Match(text);
            if (match.Success) return match.Value.Trim();

            // Jos ei löytynyt täydellistä palkkajaksoa, etsitään riviltä päivä-muoto (voi olla maksupäivä)
            var dateMatch = PayDateRegex.Match(text);
            return dateMatch.Success ? dateMatch.Value : string.Empty;
        }

        public string ExtractPayDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = NormalizeTextStart(text);

            // Etsi ensin päivämäärä, joka seuraa palkkajaksoa samalla rivillä (esim. "... 03.09.2021")
            var periodMatch = PayPeriodRegex.Match(text);
            if (periodMatch.Success)
            {
                // ota samalta riviltä viimeinen päivämäärä (maksupäivä)
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains(periodMatch.Value))
                    {
                        var lastDate = PayDateRegex.Matches(line).Cast<Match>().LastOrDefault();
                        if (lastDate != null && lastDate.Success && lastDate.Value != "")
                            return lastDate.Value;
                    }
                }
            }

            // fallback: etsi yksittäinen päivämäärä koko tekstistä (ensimmäinen esiintymä)
            var m = PayDateRegex.Match(text);
            return m.Success ? m.Value : string.Empty;
        }

        public string ExtractName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = NormalizeTextStart(text);

            var match = NameRegex.Match(text);
            return match.Success ? match.Value.Trim() : string.Empty;
        }

        public double ExtractHours(string text)
        {
            var entries = ExtractTableRows(text);
            return entries.Sum(e => e.Quantity);
        }

        // Työkohtainen tietue
        public class WorkRow
        {
            public string Code { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public double Quantity { get; set; } = 0;    // Määrä (tunnit)
            public double UnitPrice { get; set; } = 0;   // A-hinta
            public double Amount { get; set; } = 0;      // Rahamäärä
        }

        /// <summary>
        /// Etsii taulukon aloitusrivin (otsikko) ja parsii sitä seuraavat rivit kunnes "YHTEENVETO" tai tyhjä alue.
        /// Palauttaa listan WorkRow-olioita.
        /// </summary>
        public List<WorkRow> ExtractTableRows(string text)
        {
            var results = new List<WorkRow>();
            if (string.IsNullOrWhiteSpace(text)) return results;

            text = NormalizeTextStart(text);
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None)
                            .Select(l => l.Trim())
                            .ToList();

            // Etsi header-rivi (rivi, jossa on ainakin kaksi header‑avainsanaa kuten SELITE ja MÄÄRÄ tai PALKK)
            int headerIdx = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                var up = lines[i].ToUpperInvariant();
                int found = 0;
                foreach (var kw in TableHeaderKeywords)
                    if (up.Contains(kw)) found++;
                if (found >= 2)
                {
                    headerIdx = i;
                    break;
                }
            }

            // Jos ei löydy, yritetään etsiä rivi, jossa pelkkä "PALKKAKIRJAUKSET" ja käytetään seuraavaa riviä
            if (headerIdx < 0)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].ToUpperInvariant().Contains("PALKKAKIRJAUKSET"))
                    {
                        // usein otsikko on seuraavalla rivillä tai pari rivin jälkeen
                        headerIdx = i + 1;
                        break;
                    }
                }
            }

            // Jos header löytyi, aloitetaan seuraavalta riviltä
            int start = headerIdx >= 0 ? headerIdx + 1 : 0;

            WorkRow current = null;
            for (int i = start; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var up = line.ToUpperInvariant();
                // pysäytetään kun saavutetaan yhteenveto-otsikko tai uusi lohko
                if (up.Contains("YHTEENVETO") || up.Contains("EDELLINEN VUOSI") || up.Contains("PALKKATEKIJÄT"))
                    break;

                // Tarkista, alkaako rivi numerokoodilla
                var headMatch = LineWithCodeRegex.Match(line);
                if (!headMatch.Success)
                {
                    // jatkorivi — liitetään edelliseen selitteeseen, jos sellainen on
                    if (current != null)
                    {
                        current.Description = (current.Description + " " + line).Trim();
                        // siivotaan ylimääräiset välilyönnit
                        current.Description = Regex.Replace(current.Description, @"\s{2,}", " ");
                    }
                    continue;
                }

                // Tässä aloitetaan uusi rivi
                current = new WorkRow();
                current.Code = headMatch.Groups["code"].Value.Trim();
                var rest = headMatch.Groups["rest"].Value.Trim();

                // Etsi numerot riviltä
                var numMatches = NumberTokenRegex.Matches(rest).Cast<Match>().ToList();

                int firstNumIndex = -1;
                if (numMatches.Count > 0)
                {
                    // Sijainti ensimmäisen numeron alussa (käytetään descriptionin rajaamiseen)
                    firstNumIndex = rest.IndexOf(numMatches[0].Value, StringComparison.Ordinal);
                }

                // Kuvaus on ennen ensimmäistä numeroa tai koko rest jos numeroa ei löydy
                string desc = firstNumIndex >= 0 ? rest.Substring(0, firstNumIndex).Trim() : rest;
                desc = Regex.Replace(desc, @"\s{2,}", " ");
                current.Description = desc;

                // Parsitaan numerot eri tilanteisiin:
                // - 3+ numeroa: määrä, a-hinta, rahamäärä (otetaan ensimmäinen, toinen ja viimeinen)
                // - 2 numeroa: määrä ja rahamäärä (ensimmäinen ja toinen)
                // - 1 numero: yleensä rahamäärä, asetetaan Amount
                if (numMatches.Count >= 3)
                {
                    current.Quantity = ParseFinnishNumber(numMatches[0].Value);
                    current.UnitPrice = ParseFinnishNumber(numMatches[1].Value);
                    current.Amount = ParseFinnishNumber(numMatches[numMatches.Count - 1].Value);
                }
                else if (numMatches.Count == 2)
                {
                    current.Quantity = ParseFinnishNumber(numMatches[0].Value);
                    current.Amount = ParseFinnishNumber(numMatches[1].Value);
                }
                else if (numMatches.Count == 1)
                {
                    // Jos vain yksi numero, yritä päätellä: jos kuvaus sisältää sanaa kuten "Ennakonpidätys" tms.
                    current.Amount = ParseFinnishNumber(numMatches[0].Value);
                }

                results.Add(current);
            }

            return results;
        }

        /// <summary>
        /// Backward compatibility method - converts WorkRow to tuple format.
        /// </summary>
        public List<(string Code, string Description, double Hours)> ExtractWorkCodeEntriesFromLines(string text)
        {
            var rows = ExtractTableRows(text);
            return rows.Select(r => (r.Code, r.Description, r.Quantity)).ToList();
        }

        // Apumetodi: parsii suomalaista numeromuotoa kuten "1 291,62" tai "83,33" tai "15.5"
        private static double ParseFinnishNumber(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return 0;

            // Normalisoi NBSP->space ja poista tuhansierottimet ( välilyönnit )
            var s = token.Replace('\u00A0', ' ').Trim();

            // Poistetaan tuhansierottimet (välilyönnit)
            // mutta varovaisesti: "1 291,62" -> "1291,62"
            s = s.Replace(" ", string.Empty);

            // Korvataan pilkku pisteellä
            s = s.Replace(',', '.');

            if (double.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double d))
                return d;

            return 0;
        }

        private static string NormalizeTextStart(string text)
        {
            // Poista BOM jos sellainen on ja palauta
            if (string.IsNullOrEmpty(text)) return text;
            return text.TrimStart('\uFEFF');
        }
    }
}