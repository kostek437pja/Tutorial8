namespace Tutorial8.Models;

public class ClientTrip
{
    public Client Client { get; set; }
    public Trip Trip { get; set; }
    public int RegisteredAt;
    public int? PaymentDate;
}