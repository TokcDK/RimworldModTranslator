namespace RimworldModTranslator.Models
{
    public class LanguageValueData(string? language, string? value)
    {
        public string? Language { get; } = language;

        public string? Value { get; set; } = value;
    }
}
