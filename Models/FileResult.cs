using System.Collections.Generic;

namespace KuittiParseri.Models
{
    /// <summary>
    /// Edustaa yksittäisestä PDF-kuittitiedostosta jäsennettyjä tietoja.
    /// </summary>
    public class FileResult
    {
        /// <summary>
        /// Tiedoston nimi (esim. "2021-11-12-VekkaGroupOy.pdf")
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Tunnistekoodi (esim. henkilötunnus, työntekijänumero tai muu yksilöivä koodi)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Työ- ja vähennyskoodien yhteenveto (esim. "1000:38.67; 1030:1.33")
        /// </summary>
        public string WorkCodesSummary { get; set; }

        /// <summary>
        /// Palkansaajan nimi (esim. "Virtanen Juha")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tuntimäärä (esim. 40.00)
        /// </summary>
        public double Hours { get; set; }

        /// <summary>
        /// Palkkajakso (esim. "18.10.2021 - 31.10.2021")
        /// </summary>
        public string PayPeriod { get; set; }

        /// <summary>
        /// Mahdollinen virheviesti (esim. "Ei tekstiä" tai OCR-virhe)
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Tarkempi lista työkoodeista ja tunneista
        /// </summary>
        public List<(string Code, string Description, double Hours)> WorkCodes { get; set; }
    }
}