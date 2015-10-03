namespace OffWeb.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class TaxonomyViewModel : LanguageViewModel
    {
        [Required, DataType(DataType.MultilineText), AllowHtml, UIHint("Taxonomy")]
        public string Taxonomy { get; set; }       
    }
}