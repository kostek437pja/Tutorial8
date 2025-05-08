using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<int> CreateClient(CreateClientDTO clientDto);
    Task<string> RegisterClientForTrip(int clientId, int tripId);
    Task<bool> DeleteRegistration(int clientId, int tripId);
}