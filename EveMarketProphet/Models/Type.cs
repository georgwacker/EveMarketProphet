using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EveMarketProphet.Models
{
    [Table("invTypes")]
    public class Type
    {
        [Key]
        public int typeID { get; set; }

        [StringLength(100)]
        public string typeName { get; set; }

        public double volume { get; set; }

        

    }
}
