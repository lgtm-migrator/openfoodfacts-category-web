namespace OffWeb.Controllers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OffLangParser;
    using OffWeb.Code;
    using OffWeb.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        private const string wikiUrl = "http://en.wiki.openfoodfacts.org/w/api.php?action=query&prop=revisions&rvprop=content&format=json&titles=Global_categories_taxonomy";

        private readonly Lazy<LinkedLangFileParser> parser = new Lazy<LinkedLangFileParser>(ParserFactory.GetParser, LazyThreadSafetyMode.PublicationOnly);

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        private LinkedLangFileParser Parser
        {
            get
            {
                return this.parser.Value;
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(TaxonomyViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (this.ModelState.IsValid)
            {
                using (var stream = GetTaxonomyStream(model.Taxonomy))
                {
                    try
                    {
                        return await this.Analyze(stream, model.Language);
                    }
                    catch (DuplicateWordsException ex)
                    {
                        this.ModelState.AddModelError(nameof(model.Taxonomy), ex);
                        return this.View("Dupes", ex);
                    }
                }
            }
            else
            {
                return this.View("Index", model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CurrentWiki(LanguageViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (this.ModelState.IsValid)
            {
                using (var stream = await GetTaxonomyFromWikiAsync())
                {
                    try
                    {
                        return await this.Analyze(stream, model.Language);
                    }
                    catch (DuplicateWordsException ex)
                    {
                        this.ModelState.AddModelError(string.Empty, ex);
                        return this.View("Dupes", ex);
                    }
                }
            }
            else
            {
                return this.View("Index", model);
            }
        }

        private static async Task<Stream> GetTaxonomyFromWikiAsync()
        {
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(wikiUrl);
                var data = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<dynamic>(json));

                var wikitext = ((JObject)data.query.pages).PropertyValues().Cast<dynamic>().First().revisions.First["*"];
                return GetTaxonomyStream((string)wikitext);
            }
        }

        private async Task<ActionResult> Analyze(Stream stream, string language)
        {
            var result = new AnalyzedTaxonomyViewModel
            {
                Language = language
            };

            var file = this.Parser.Parse(stream, Encoding.UTF8);

            var en = Culture.FromIsoName("en");
            var culture = string.IsNullOrWhiteSpace(language) ? null : Culture.FromIsoName(language);
            if (!en.Equals(culture))
            {
                var builder = new StringBuilder();

                var translationSetWithoutEnglish = file.TranslationSets.HasNoEntryInCulture(en);
                if (culture != null)
                {
                    translationSetWithoutEnglish = translationSetWithoutEnglish.HasEntryInCulture(culture);
                }

                foreach (var translationSet in translationSetWithoutEnglish)
                {
                    foreach (var translation in translationSet.Translations)
                    {
                        builder.Append(translation.Language.Name);
                        builder.Append(':');
                        builder.AppendLine(string.Join(", ", translation.Words));
                    }

                    builder.AppendLine();
                }

                result.CategoryWithoutEnglishEntry = builder.ToString();
            }

            using (var writer = new StringWriter())
            {
                await file.RemoveRedundantParents().WriteToAsync(writer);
                result.UpdatedTaxonomy = writer.GetStringBuilder().ToString();
            }

            return this.View("Result", result);
        }

        private static Stream GetTaxonomyStream(string taxonomy)
        {
            return new MemoryStream(GetTaxonomyBytes(taxonomy));
        }

        private static byte[] GetTaxonomyBytes(string taxonomy)
        {
            return Encoding.UTF8.GetBytes(GetTaxonomy(taxonomy));
        }

        private static string GetTaxonomy(string taxonomy)
        {
            // This method removed anything outside the <pre></pre>. It
            // is designed for a single <pre> in the text and is obviously
            // not meant as a HTML parser, but just as a convenience feature.
            const string startTag = "<pre>";
            const string endTag = "</pre>";

            var positionStartTag = taxonomy.LastIndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            var positionEndTag = taxonomy.LastIndexOf(endTag, StringComparison.OrdinalIgnoreCase);
            if (positionEndTag < 0 || positionStartTag < 0 || positionEndTag <= positionStartTag)
            {
                return taxonomy;
            }
            else
            {
                return taxonomy.Substring(positionStartTag, positionEndTag - positionStartTag);
            }
        }
    }
}
