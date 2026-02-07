namespace ActPro.DAL.Entities;

public partial class City
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Place> Places { get; set; } = new List<Place>();
}
