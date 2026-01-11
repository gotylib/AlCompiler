namespace ALCompiler.CodeGenerator.RegisterModel
{
    public class TaxRegister
    {
        public string Code { get; set; }
        public List<RegisterRow> Table { get; set; } = new();

        // Для доступа к данным по номерам граф
        public object GetValue(int rowIndex, int columnIndex)
        {
            if (rowIndex >= 0 && rowIndex < Table.Count)
                return Table[rowIndex].GetValue(columnIndex);
            return null;
        }

        public void SetValue(int rowIndex, int columnIndex, object value)
        {
            if (rowIndex >= 0 && rowIndex < Table.Count)
                Table[rowIndex].SetValue(columnIndex, value);
        }
    }
}
