namespace OffLangParser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    public static class Culture
    {
        private static readonly IDictionary<string, CultureData> cultures = new Dictionary<string, CultureData>();

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iso")]
        /// <remarks>
        /// http://stackoverflow.com/a/9841533/11963
        /// </remarks>
        public static CultureData FromIsoName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var key = name.ToUpperInvariant();
            CultureData result;
            if (!cultures.TryGetValue(key, out result))
            {
                var info = (from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
                            where c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                            select c).FirstOrDefault();
                result = info != null ? new CultureData(info) : new CultureData(name);
                cultures.Add(key, result);
            }

            return result;
        }
    }
}
