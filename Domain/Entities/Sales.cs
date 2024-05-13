using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Sales
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int Company_ID { get; set; }
        public string Mov { get; set; }
        public string MovID { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime UltimoCambio { get; set; }
        public string Moneda { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }
        public string Warehouse { get; set; }
        public decimal Importe { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Saldo { get; set; }
        public decimal Discount { get; set;}
        public decimal PrecioTotal { get; set; }

    }
}
