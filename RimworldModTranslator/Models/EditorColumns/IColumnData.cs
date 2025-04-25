namespace RimworldModTranslator.Models.EditorColumns
{
    public interface IColumnData
    {
        public string Name { get; }
        public string Caption { get; }
        public bool IsReadOnly { get; }
    }
}
