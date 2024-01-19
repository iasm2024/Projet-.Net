namespace PanierMVC.Models
{
    
        public class Produit
        {
            public int id { get; set; }
            public required string nom { get; set; }
            public string? type { get; set; }
            public string? prix { get; set; }

            public string? image { get; set; }
        }
    

}
