using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Entities
{
	public class Users
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string code { get; set; }
        public string nameUser { get; set; }
        public string pass { get; set; }
        public string nameP { get; set; }
        public string lastNameP { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
    }
}

