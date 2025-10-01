using System;
using System.Text.RegularExpressions;

namespace PekkasParseri.Services
{
    public class PayrollParser
    {
        private readonly Regex _palkkajakso = new Regex(
            @"PALKKAKAUSI\s+(?<start>\d{1,2}\.\d{1,2}\.\d{4})\s*-\s*(?<end>\d{1,2}\.\d{4})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Regex _maksupaiva = new Regex(
            @"MAKSUPÄIVÄ\s+(?<date>\d{1,2}\.\d{1,2}\.\d{4})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public (DateTime Start, DateTime End)? ParsePalkkajakso(string text)
        {
            var m = _palkkajakso.Match(text);
            if (m.Success &&
                DateTime.TryParse(m.Groups["start"].Value, out var start) &&
                DateTime.TryParse(m.Groups["end"].Value, out var end))
            {
                return (start, end);
            }
            return null;
        }

        public DateTime? ParseMaksupaiva(string text)
        {
            var m = _maksupaiva.Match(text);
            if (m.Success && DateTime.TryParse(m.Groups["date"].Value, out var date))
            {
                return date;
            }
            return null;
        }
    }
}