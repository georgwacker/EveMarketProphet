using System.ComponentModel.DataAnnotations.Schema;

namespace EveMarketProphet.Models
{
    [Table("mapSolarSystemJumps")]
    public class SolarSystemJump
    {
        public int fromRegionID { get; set; }
        public int fromConstellationID { get; set; }
        public int fromSolarSystemID { get; set; }
        public int toSolarSystemID { get; set; }
        public int toConstellationID { get; set; }
        public int toRegionID { get; set; }
    }
}
