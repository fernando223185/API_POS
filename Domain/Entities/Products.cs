using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Domain.Entities
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string? name { get; set; }

        public string? description { get; set; }
        public string? code { get; set; }
        public string? barcode { get; set; }
        [Precision(18, 2)]
        public decimal price { get; set; }
    }
}
