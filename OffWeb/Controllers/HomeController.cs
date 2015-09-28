namespace OffWeb.Controllers
{
    using OffLangParser;
    using OffWeb.Code;
    using OffWeb.Models;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        private readonly Lazy<LinkedLangFileParser> parser = new Lazy<LinkedLangFileParser>(ParserFactory.GetParser, LazyThreadSafetyMode.PublicationOnly);

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View(new TaxonomyViewModel());
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
                var result = new AnalyzedTaxonomyViewModel
                {
                    Taxonomy = model.Taxonomy,
                    Language = model.Language
                };

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(model.Taxonomy)))
                {
                    try
                    {
                        var file = this.Parser.Parse(stream, Encoding.UTF8);

                        var en = Culture.FromIsoName("en");
                        var culture = string.IsNullOrWhiteSpace(model.Language) ? null : Culture.FromIsoName(model.Language);
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
                    catch (DuplicateWordsException ex)
                    {
                        this.ModelState.AddModelError(nameof(model.Taxonomy), ex);
                        return this.View("Index", model);
                    }
                }
            }
            else
            {
                return this.View("Index", model);
            }
        }
    }
}
