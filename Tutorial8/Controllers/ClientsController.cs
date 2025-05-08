
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ITripsService _tripsService;
        private readonly IClientsService _clientsService;

        public ClientsController(ITripsService tripsService, IClientsService clientsService)
        {
            _tripsService = tripsService;
            _clientsService = clientsService;
        }

        [HttpGet("{clientId}/trips")]
        public async Task<IActionResult> GetClientTrips(int clientId)
        {   
            var clientTrips = await _tripsService.GetTripsBasedOnClient(clientId);
            return Ok(clientTrips);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO client)
        {
            try
            {
                var newClientId = await _clientsService.CreateClient(client);
                return Created("", new { id = newClientId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }
        
        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
        {
            try
            {
                var result = await _clientsService.RegisterClientForTrip(id, tripId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpDelete("{clientId}/trips/{tripId}")]
        public async Task<IActionResult> DeleteRegistration(int clientId, int tripId)
        {
            try
            {
                bool result = await _clientsService.DeleteRegistration(clientId, tripId);
                if (!result)
                {
                    return NotFound();   
                }
                
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
