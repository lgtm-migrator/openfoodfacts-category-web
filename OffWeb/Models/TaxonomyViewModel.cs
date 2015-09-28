namespace OffWeb.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class TaxonomyViewModel
    {
        [Required, DataType(DataType.MultilineText), AllowHtml, UIHint("Taxonomy")]
        public string Taxonomy { get; set; }

        [MinLength(2), MaxLength(2)]
        public string Language { get; set; }
    }
}