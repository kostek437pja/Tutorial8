using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    
    public async Task<int> CreateClient(CreateClientDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email) ||
            string.IsNullOrWhiteSpace(client.Telephone) ||
            string.IsNullOrWhiteSpace(client.Pesel))
        {
            throw new ArgumentException("All fields are required.");
        }

        string insertQuery = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(insertQuery, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            if (result != null)
            {
                return (int) result;
            }

            throw new Exception("Client insertion failed.");
        }
    }
    
    public async Task<string> RegisterClientForTrip(int clientId, int tripId)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            using (SqlCommand cmd = conn.CreateCommand())
            {
                
                cmd.CommandText = "SELECT COUNT(1) FROM Client WHERE IdClient = @clientId";
                cmd.Parameters.AddWithValue("@clientId", clientId);
                var clientExists = (int)await cmd.ExecuteScalarAsync() > 0;

                if (!clientExists)
                {
                    throw new ArgumentException("Client not found");
                }
                
                cmd.CommandText = "SELECT COUNT(1) FROM Trip WHERE IdTrip = @tripId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@tripId", tripId);
                var tripExists = (int)await cmd.ExecuteScalarAsync() > 0;

                if (!tripExists)
                {
                    throw new ArgumentException("Trip not found");
                }
                
                cmd.CommandText = "SELECT COUNT(1) FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@clientId", clientId);
                cmd.Parameters.AddWithValue("@tripId", tripId);
                var alreadyRegistered = (int)await cmd.ExecuteScalarAsync() > 0;

                if (alreadyRegistered)
                {
                    throw new ArgumentException("Client already registered to this trip");
                }
                
                cmd.CommandText = @"
                    SELECT 
                        (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId) AS RegisteredCount,
                        (SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId) AS MaxPeople";

                using var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                    
                int registered = reader.GetInt32(0);
                int maxPeople = reader.GetInt32(1);

                await reader.CloseAsync();

                if (registered >= maxPeople)
                {
                    throw new ArgumentException("Trip is full");
                }
                
                cmd.CommandText = @"
                    INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                    VALUES (@clientId, @tripId, @registeredAt)";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@clientId", clientId);
                cmd.Parameters.AddWithValue("@tripId", tripId);
                cmd.Parameters.AddWithValue("@registeredAt", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                await cmd.ExecuteScalarAsync();

                return "Client registered to the trip successfully.";
            }
        }
    }

    public async Task<bool> DeleteRegistration(int clientId, int tripId)
    {
        
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string delete = @"DELETE FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId";
        const string verify = @"SELECT 1 FROM Client_Trip WHERE IdClient = @clientId AND IdTrip = @tripId";

        using (var checkCmd = new SqlCommand(verify, conn))
        {
            checkCmd.Parameters.AddWithValue("@clientId", clientId);
            checkCmd.Parameters.AddWithValue("@tripId", tripId);

            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null)
            {
                return false;
            }
        }

        using (var deleteCmd = new SqlCommand(delete, conn))
        {
            deleteCmd.Parameters.AddWithValue("@clientId", clientId);
            deleteCmd.Parameters.AddWithValue("@tripId", tripId);
            await deleteCmd.ExecuteScalarAsync();
        }

        return true;
    }

}