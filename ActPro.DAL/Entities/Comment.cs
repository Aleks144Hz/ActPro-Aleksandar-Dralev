using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActPro.DAL.Entities;

public partial class Comment
{
    public int Id { get; set; }

    public int PlaceId { get; set; }

    public string AspNetUserId { get; set; } = null!;

    public string? CommentText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Place Place { get; set; } = null!;

    public int Rating { get; set; }

    [ForeignKey("AspNetUserId")]
    public virtual ApplicationUser User { get; set; }
}
