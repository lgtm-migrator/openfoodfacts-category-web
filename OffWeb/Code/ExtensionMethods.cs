namespace OffWeb.Code
{
    using OffLangParser;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ExtensionMethods
    {
        public static IEnumerable<TranslationSet> HasNoEntryInCulture(this IEnumerable<TranslationSet> source, CultureData culture)
        {
            return source.Where(ts => !ts.Translations.Any(t => t.Language == culture));
        }

        public static IEnumerable<TranslationSet> HasEntryInCulture(this IEnumerable<TranslationSet> source, CultureData culture)
        {
            return source.Where(ts => ts.Translations.Any(t => t.Language == culture));
        }

        public static LangFile RemoveRedundantParents(this LangFile source)
        {
            return new LangFile(source.StopWords, source.Synonyms, source.TranslationSets.WithoutRedundantParents());
        }

        public static IReadOnlyList<TranslationSet> WithoutRedundantParents(this IReadOnlyList<TranslationSet> source)
        {
            var result = new List<TranslationSet>(source.Count);

            foreach (var translationSet in source)
            {
                var dupes = new List<TranslationSet>();
                var parallelParents = translationSet.Parents.Distinct().AsParallel();
                Parallel.ForEach(
                    parallelParents,
                    parent =>
                    {
                        if (parallelParents.Any(p => p != parent && p.ContainsRecursive(parent)))
                        {
                            dupes.Add(parent);
                        }
                    });

                result.Add(new TranslationSet(translationSet.Parents.Except(dupes).ToList(), translationSet.Translations, translationSet.LinkedData));
            }

            return result;
        }

        public static bool ContainsRecursive(this TranslationSet haystack, TranslationSet needle)
        {
            if (haystack == null)
            {
                return false;
            }

            if (needle == null)
            {
                throw new ArgumentNullException(nameof(needle));
            }

            if (ReferenceEquals(haystack, needle)
                || haystack.Translations.Select(t => t.Words.Select(w => new Word(w, t.Language))).Any(ws => needle.Translations.Any(t => t.Words.Select(ww => new Word(ww, t.Language)).Any(ww => ws.Any(w => w.Equals(ww))))))
            {
                return true;
            }

            var any = false;
            Parallel.ForEach(
                haystack.Parents,
                (parent, state) =>
                {
                    if (parent.ContainsRecursive(needle))
                    {
                        any = true;
                        state.Break();
                    }
                });
            return any;
        }
    }
}