namespace OffWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class AnalyzedTaxonomyViewModel : LanguageViewModel
    {
        [DataType(DataType.MultilineText), Editable(false), UIHint("Taxonomy")]
        public string UpdatedTaxonomy { get; set; }

        [DataType(DataType.MultilineText) Editable(false), UIHint("Taxonomy")]
        public string CategoryWithoutEnglishEntry { get; set; }
    }
}