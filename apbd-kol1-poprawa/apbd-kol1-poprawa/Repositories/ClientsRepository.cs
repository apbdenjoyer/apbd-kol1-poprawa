using System.Data.SqlClient;
using apbd_kol1_poprawa.Models.DTOs;

namespace apbd_kol1_poprawa.Repositories;

public class ClientsRepository : IClientsRepository
{
    private readonly IConfiguration _configuration;

    public ClientsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<bool> DoesClientExist(int id)
    {
        var query = @"SELECT 1 FROM CLIENTS WHERE ID = @ID";
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;

        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<ClientWithRentalsDto> GetClientWithRentals(int clientId)
    {
        var query = @"SELECT
                            clients.id as clientId,
                            clients.firstname as firstName,
                            clients.lastname as lastName,
                            clients.address as address,
                            cars.vin as vin,
                            colors.name as color,
                            models.name as model,
                            cr.dateFrom as dateFrom,
                            cr.dateTo as dateTo,
                            cr.totalprice as totalPrice
                            FROM CLIENTS 
                            JOIN CAR_RENTALS AS CR ON CLIENTS.ID = CR.CLIENTID
                            JOIN CARS ON CR.CARID = CARS.ID
                            JOIN COLORS ON CARS.COLORID = COLORS.ID
                            JOIN MODELS ON CARS.MODELID = MODELS.ID
                            WHERE CLIENTS.ID = @ID";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;

        command.Parameters.AddWithValue("@ID", clientId);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var clientIdOrd = reader.GetOrdinal("clientId");
        var firstNameOrd = reader.GetOrdinal("firstName");
        var lastNameOrd = reader.GetOrdinal("lastName");
        var addressOrd = reader.GetOrdinal("address");
        var vinOrd = reader.GetOrdinal("vin");
        var colorOrd = reader.GetOrdinal("color");
        var modelOrd = reader.GetOrdinal("model");
        var dateFromOrd = reader.GetOrdinal("dateFrom");
        var dateToOrd = reader.GetOrdinal("dateTo");
        var totalPriceOrd = reader.GetOrdinal("totalPrice");

        ClientWithRentalsDto clientWithRentalsDto = null;

        while (await reader.ReadAsync())
        {
            if (clientWithRentalsDto == null)
            {
                clientWithRentalsDto = new ClientWithRentalsDto()
                {
                    Id = reader.GetInt32(clientIdOrd),
                    FirstName = reader.GetString(firstNameOrd),
                    LastName = reader.GetString(lastNameOrd),
                    Address = reader.GetString(addressOrd),
                    Rentals = new List<RentalDto>()
                };
            }

            clientWithRentalsDto.Rentals.Add(new RentalDto()
            {
                Vin = reader.GetString(vinOrd),
                Color = reader.GetString(colorOrd),
                Model = reader.GetString(modelOrd),
                DateFrom = reader.GetDateTime(dateFromOrd),
                DateTo = reader.GetDateTime(dateToOrd),
                TotalPrice = reader.GetInt32(totalPriceOrd)
            });
        }

        if (clientWithRentalsDto is null)
        {
            throw new Exception();
        }

        return clientWithRentalsDto;
    }

    public async Task<bool> DoesCarExist(int id)
    {
        var query = @"SELECT 1 FROM CARS WHERE ID = @ID";
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;

        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return result is not null;
    }

    public async Task<int> GetCarPrice(int id)
    {
        var query = @"SELECT 1 FROM CARS WHERE ID = @ID";
        
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;

        command.CommandText = @"SELECT PricePerDay from CARS WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task AddClientWithRental(AddClientWithRentalDto addClientWithRentalDto)
    {
        var insert = @"INSERT INTO CLIENTS VALUES(@FirstName, @LastName, @Address); 
                       SELECT SCOPE_IDENTITY() AS ID";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = insert;

        command.Parameters.AddWithValue("@FirstName", addClientWithRentalDto.Client.FirstName);
        command.Parameters.AddWithValue("@LastName", addClientWithRentalDto.Client.LastName);
        command.Parameters.AddWithValue("@Address", addClientWithRentalDto.Client.Address);

        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            var clientId = await command.ExecuteScalarAsync();

            /*get total price*/
            var pricePerDay = await GetCarPrice(addClientWithRentalDto.CarId);
            var totalPrice = pricePerDay * (addClientWithRentalDto.DateTo - addClientWithRentalDto.DateFrom).Days;


            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO CAR_RENTALS VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice, null); 
                  SELECT SCOPE_IDENTITY() AS ID";

            command.Parameters.AddWithValue("@ClientID", clientId);
            command.Parameters.AddWithValue("@CarID", addClientWithRentalDto.CarId);
            command.Parameters.AddWithValue("@DateFrom", addClientWithRentalDto.DateFrom);
            command.Parameters.AddWithValue("@DateTo", addClientWithRentalDto.DateTo);
            command.Parameters.AddWithValue("@TotalPrice", totalPrice);

            await command.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public interface IClientsRepository
{
    public Task<bool> DoesClientExist(int id);

    public Task<ClientWithRentalsDto> GetClientWithRentals(int clientId);

    public Task<bool> DoesCarExist(int id);

    public Task<int> GetCarPrice(int id);

    public Task AddClientWithRental(AddClientWithRentalDto addClientWithRentalDto);
}