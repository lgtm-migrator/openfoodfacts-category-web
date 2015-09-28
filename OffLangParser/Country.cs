﻿namespace OffLangParser
{
    using System.Collections.Generic;

    public class Country : LinkedData
    {
        public Country(CultureData language, IReadOnlyList<string> words)
            : base(language, words)
        {
        }

        public override string Prefix
        {
            get
            {
                return Prefixes.Country;
            }
        }
    }
}
