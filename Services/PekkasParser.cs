using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PekkasParseri.Services
{
    public class PekkasParser
    {
        private readonly Regex _pekkasRow = new Regex(
            @"(?<code>\d{3,4})\s+(?<name>[A-Za-zÅÄÖåäö\s]+?)\s+(?<hours>\d+[.,]\d+)",
            RegexOptions.Compiled);

        public (string Code, string Name, double Hours)? Parse(string text)
        {
            var m = _pekkasRow.Match(text);
            if (m.Success)
            {
                var code = m.Groups["code"].Value.Trim();
                var name = m.Groups["name"].Value.Trim();
                var hoursStr = m.Groups["hours"].Value.Replace(",", ".");
                if (double.TryParse(hoursStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double hours))
                {
                    return (code, name, hours);
                }
            }
            return null;
        }
    }
}