using System.ComponentModel.DataAnnotations.Schema;

namespace ActPro.DAL.Entities;

public partial class Place
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? CityId { get; set; }

    public int? ActivityId { get; set; }

    public int? Capacity { get; set; }

    public decimal? Price { get; set; }

    public int? Rating { get; set; }

    public bool? IsOutdoor { get; set; }

    public string Address { get; set; } = string.Empty;

    public virtual Activity? Activity { get; set; }

    public virtual City? City { get; set; }

    public string? OwnerId { get; set; }

    [ForeignKey("OwnerId")]
    public virtual ApplicationUser? Owner { get; set; }

    public bool IsApproved { get; set; } = false;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<PlaceImage> PlaceImages { get; set; } = new List<PlaceImage>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<PlaceClosure> PlaceClosures { get; set; } = new List<PlaceClosure>();
}
