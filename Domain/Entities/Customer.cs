using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxId { get; set; }
        public string ZipCode { get; set; }
        public string Commentary { get; set; }
        public int CountryId { get; set; }
        [Required]
        public int StateId { get; set; }
        [Required]
        public DateTime Created_at { get; set; } = DateTime.Now;
        public string InteriorNumber { get; set; }
        public string ExteriorNumber { get; set; }
        public int ExternalId { get; set; }

        [DefaultValue(1)]
        public int StatusId { get; set; }
    }
}

