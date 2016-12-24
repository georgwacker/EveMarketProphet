using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace EveMarketProphet.Models
{
    [Table("mapRegions")]
    [UsedImplicitly]
    public class Region
    {
        [Key]
        public int regionID { get; set; }
        [StringLength(100)]
        public string regionName { get; set; }
    }
}
