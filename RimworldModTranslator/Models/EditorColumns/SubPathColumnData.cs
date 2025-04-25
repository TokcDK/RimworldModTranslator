using RimworldModTranslator.Services;

namespace RimworldModTranslator.Models.EditorColumns
{
    public class SubPathColumnData : IColumnData
    {
        public string? Name => "SubPath";
        public bool IsReadOnly => true;
        public string? Caption => T._("SubPath");
    }
}
