namespace OffWeb.Models
{
    using System.ComponentModel.DataAnnotations;

    public class LanguageViewModel
    {
        [MinLength(2), MaxLength(2)]
        public string Language { get; set; }
    }
}