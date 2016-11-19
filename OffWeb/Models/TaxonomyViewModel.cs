namespace OffWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class TaxonomyViewModel : LanguageViewModel
    {
        [Required, DataType(DataType.MultilineText), UIHint("Taxonomy")]
        public string Taxonomy { get; set; }       
    }
}