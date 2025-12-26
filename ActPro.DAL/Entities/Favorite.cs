using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActPro.DAL.Entities;

public partial class Favorite
{
    public int Id { get; set; }
    public string AspNetUserId { get; set; }
    public int PlaceId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Place Place { get; set; }
}
