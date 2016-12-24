using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EveMarketProphet.Models
{
    [Table("staStations")]
    public class Station
    {
        [Key]
        public long stationID { get; set; }

        public double security { get; set; }
        public int corporationID { get; set; }
        public int solarSystemID { get; set; }
        public int regionID { get; set; }

        [StringLength(100)]
        public string stationName { get; set; }
    }
}
