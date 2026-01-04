using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActPro.DAL.Entities;

public partial class Reservation
{
    public int Id { get; set; }

    public string? AspNetUserId { get; set; }

    public int? PlaceId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public DateOnly? ReservationDate { get; set; }

    public TimeOnly? ReservationTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Place? Place { get; set; }
    [ForeignKey("AspNetUserId")]
    public virtual ApplicationUser? AspNetUser { get; set; }
}
