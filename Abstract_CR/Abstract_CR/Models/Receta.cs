namespace Abstract_CR.Models
{
    public class Receta
    {
        public int RecetaID { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Instrucciones { get; set; }
        public bool EsGratuita { get; set; }
        public bool EsParteDeEbook { get; set; }
        public int ChefID { get; set; }
        public int RecetarioID { get; set; }
        public byte[] RowVer { get; set; }
    }

    public class RecetasViewModel
    {
        public int RecetaID { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Instrucciones { get; set; }
        public bool EsGratuita { get; set; }
        public bool EsParteDeEbook { get; set; }
        public string Chef { get; set; }
    }
}
