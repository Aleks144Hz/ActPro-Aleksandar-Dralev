namespace ActPro.DAL.Entities;

public partial class PlaceImage
{
    public int Id { get; set; }

    public int? PlaceId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Place? Place { get; set; }
}
