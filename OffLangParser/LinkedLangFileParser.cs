namespace OffLangParser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class LinkedLangFileParser
    {
        private readonly LangFileParser langFileParser;

        public LinkedLangFileParser(LangFileParser langFileParser)
        {
            if (langFileParser == null)
            {
                throw new ArgumentNullException(nameof(langFileParser));
            }

            this.langFileParser = langFileParser;
        }

        public LangFile Parse(string fileName)
        {
            var unlinkedFile = this.langFileParser.Parse(fileName);

            // Reverse map each translation to their translation set,
            // useful for faster linking of the entries to their parents.
            var reverseMap = GetReverseMap(unlinkedFile);

            var resultSet = new List<TranslationSet>();

            foreach (var set in unlinkedFile.TranslationSets)
            {
                if (set.Parents.Count == 0)
                {
                    resultSet.Add(set);
                }
                else
                {
                    var realParents = new List<TranslationSet>();

                    foreach (var parent in set.Parents)
                    {
                        foreach (var translation in parent.Translations)
                        {
                            foreach (var word in translation.Words)
                            {
                                var mappedWord = new Word(word, translation.Language);

                                TranslationSet parentSet;
                                if (reverseMap.TryGetValue(mappedWord, out parentSet))
                                {
                                    realParents.Add(parentSet);
                                }
                            }
                        }
                    }

                    resultSet.Add(new TranslationSet(realParents.Distinct().ToList(), set.Translations, set.LinkedData));
                }
            }

            return new LangFile(unlinkedFile.StopWords, unlinkedFile.Synonyms, resultSet);
        }

        private static IReadOnlyDictionary<Word, TranslationSet> GetReverseMap(LangFile unlinkedFile)
        {
            Validate(unlinkedFile);

            var result = new Dictionary<Word, TranslationSet>();

            foreach (var translationSet in unlinkedFile.TranslationSets)
            {
                foreach (var translation in translationSet.Translations)
                {
                    foreach (var word in HacketyHack(translation.Words))
                    {
                        result.Add(new Word(word, translation.Language), translationSet);
                    }
                }
            }

            return new WordTranslationSetDictionary(result);
        }

        private static void Validate(LangFile unlinkedFile)
        {
            var dupes = (from ts in unlinkedFile.TranslationSets
                         from t in ts.Translations
                         from w in t.Words
                         let word = new Word(w, t.Language)
                         group word by word into g
                         let cw = new { g.Key, Count = g.Count() }
                         where cw.Count > 1
                         orderby cw.Count descending
                         select cw).ToList();

            if (dupes.Count > 0)
            {
                var message = new StringBuilder(dupes.Count * 100);
                message.AppendLine("Duplicate words: ");
                foreach (var dupe in dupes)
                {
                    message.AppendFormat(CultureInfo.CurrentCulture, "{0}: {1} ({2} occurences)", dupe.Key.Language.Name, dupe.Key.Name, dupe.Count).AppendLine();
                }

                throw new Exception(message.ToString());
            }
        }

        private static IEnumerable<string> HacketyHack(IReadOnlyList<string> words)
        {
            // HACK for duplicate translations "Frozen string beans" and "Fruit syrups" as of 2015-06-28T20:17+0200.
            var filterBeans = words.Any(w => w.Equals("Frozen string beans") && words.Any(w2 => w2.Equals("Frozen common beans")));
            var filterSyrup = words.Any(w => w.Equals("Fruit syrups") && words.Any(w2 => w2.Equals("Fruit molasses")));
            return words.Where(w => (!filterBeans || (filterBeans && !w.Equals("Frozen string beans")))
            && (!filterSyrup || (filterSyrup && !w.Equals("Fruit syrups"))));
        }
    }
}
