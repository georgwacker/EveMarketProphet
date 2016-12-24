using System.ComponentModel.DataAnnotations.Schema;

namespace EveMarketProphet.Models
{
    [Table("invContrabandTypes")]
    public class ContrabandType
    {
        public int factionID { get; set; }
        public int typeID { get; set; }
    }
}
