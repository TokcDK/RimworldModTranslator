using RimworldModTranslator.Services;

namespace RimworldModTranslator.Models.EditorColumns
{
    public class FolderColumnData : IColumnData
    {
        public string Name => "Folder";
        public bool IsReadOnly => true;
        public string Caption => T._("Folder");
    }
}
