using System;
using System.Collections.Generic;

namespace ActPro.DAL.Entities;

public partial class Favorite
{
    public int Id { get; set; }

    public string? AspNetUserId { get; set; }

    public int? PlaceId { get; set; }

    public virtual Place? Place { get; set; }
}
