using RimworldModTranslator.Services;

namespace RimworldModTranslator.Models.EditorColumns
{
    public class IdColumnData : IColumnData
    {
        public string Name => "ID";
        public bool IsReadOnly => true;
        public string Caption => T._("ID");
    }
}
