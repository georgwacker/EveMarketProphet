using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EveMarketProphet.Models
{
    [Table("mapSolarSystems")]
    public class SolarSystem
    {
        [Key]
        public int solarSystemID { get; set; }

        public int regionID { get; set; }
        
        [StringLength(100)]
        public string solarSystemName { get; set; }

        public double security { get; set; }
    }
}
