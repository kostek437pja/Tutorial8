namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public int RegisteredAt { get; set; } 
    public int? PaymentDate { get; set; }
    public TripDTO TripDto { get; set; }
}

