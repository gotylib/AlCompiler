namespace ALCompiler.CodeGenerator.RegisterModel
{
    public class TaxRegister
    {
        public string Code { get; set; }
        public List<dynamic[]> Table { get; set; } = new();
    }
}
