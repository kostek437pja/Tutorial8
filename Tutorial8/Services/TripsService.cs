using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name as Country
                FROM Trip t
                INNER JOIN Country_Trip ct ON ct.IdTrip = t.IdTrip
                INNER JOIN Country c ON c.IdCountry = ct.IdCountry";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));


                    if (trips.Find(dto => dto.Id == tripId) == null)
                    {
                        trips.Add(new TripDTO()
                        {
                            Id = tripId,
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountryDTO>()
                        });
                    }
                    Console.WriteLine(trips.Find(dto => dto.Id == tripId).Name);
                    trips.Find(dto => dto.Id == tripId).Countries.Add(new CountryDTO()
                    {
                        Name = reader.GetString(6),
                    });
                    trips.Sort((dto, tripDto) => dto.Id.CompareTo(tripDto.Id));
                    
                }
            }
        }
        

        return trips;
    }

    public async Task<List<ClientTripDTO>> GetTripsBasedOnClient(int clientId)
    {
        var clientTrips = new List<ClientTripDTO>();

        string command = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name as Country, IdClient, RegisteredAt, PaymentDate 
                            FROM Trip t
                            INNER JOIN Country_Trip ct ON ct.IdTrip = t.IdTrip
                            INNER JOIN Country c ON c.IdCountry = ct.IdCountry INNER JOIN Client_Trip clt ON clt.IdTrip = t.IdTrip where clt.IdClient = @clientId";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {   
            cmd.Parameters.AddWithValue("@clientId", clientId);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(0);
                    
                    var existing = clientTrips.FirstOrDefault(ct => ct.TripDto.Id == tripId);

                    if (existing == null)
                    {
                        var trip = new TripDTO
                        {
                            Id = tripId,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };

                        var clientTrip = new ClientTripDTO
                        {
                            RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                            PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) ? null : reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                            TripDto = trip
                        };

                        clientTrip.TripDto.Countries.Add(new CountryDTO
                        {
                            Name = reader.GetString(reader.GetOrdinal("Country"))
                        });

                        clientTrips.Add(clientTrip);
                    }
                    else
                    {
                        existing.TripDto.Countries.Add(new CountryDTO
                        {
                            Name = reader.GetString(reader.GetOrdinal("Country"))
                        });
                    }

                }
            }
        }
        

        return clientTrips;
    }
}