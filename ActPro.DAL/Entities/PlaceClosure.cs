using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActPro.DAL.Entities
{
    public class PlaceClosure
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        [Required]
        public DateTime ClosureDate { get; set; }

        public string Reason { get; set; }
    }
}
