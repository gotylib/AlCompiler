namespace ALCompiler.CodeGenerator.RegisterModel
{
    public class RegisterRow
    {
        private Dictionary<int, object> _values = new();

        public object? GetValue(int columnIndex) =>
            _values.TryGetValue(columnIndex, out var value) ? value : null;

        public void SetValue(int columnIndex, object value) =>
            _values[columnIndex] = value;

    }
}
