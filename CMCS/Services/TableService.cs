using Azure.Data.Tables;
using Azure;
using System.Text;
using System.Security.Cryptography;

public class TableService
{
    private readonly TableClient _userTableClient;
    private readonly TableClient _claimsTableClient;
    private readonly ILogger<TableService> _logger;

    public TableService(IConfiguration configuration, ILogger<TableService> logger)
    {
        string storageConnectionString = configuration.GetSection("AzureStorage")["ConnectionString"];

        _userTableClient = new TableClient(storageConnectionString, "Users");
        _claimsTableClient = new TableClient(storageConnectionString, "Claims");

        _logger = logger;

        CreateTableIfNotExists(_userTableClient);
        CreateTableIfNotExists(_claimsTableClient);
    }

    private void CreateTableIfNotExists(TableClient tableClient)
    {
        try
        {
            tableClient.CreateIfNotExists();
            _logger.LogInformation($"Azure Table '{tableClient.Name}' created or already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating the table '{tableClient.Name}'.");
        }
    }

    public async Task RegisterUserAsync(string email, string passwordHash, string role)
    {
        try
        {
            var userEntity = new TableEntity(email, Guid.NewGuid().ToString())
            {
                { "PasswordHash", passwordHash },
                { "Role", role }
            };
            await _userTableClient.AddEntityAsync(userEntity);
            _logger.LogInformation("User registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user.");
        }
    }

   public async Task<UserEntity> GetUserByEmailAsync(string email)
{
    try
    {
        // Query the table to get the user by email (RowKey)
        var queryResult = _userTableClient.Query<UserEntity>(u => u.RowKey == email).FirstOrDefault();

        if (queryResult != null)
        {
            return queryResult;
        }

        return null; // Return null if the user is not found
    }
    catch (RequestFailedException ex)
    {
        _logger.LogError(ex, "Error retrieving user by email.");
        return null;
    }
}

    public async Task SubmitClaimAsync(string userId, double hoursWorked, double hourlyRate, string extraNotes, string fileUrls)
    {
        try
        {
            var claimEntity = new TableEntity(userId, Guid.NewGuid().ToString())
            {
                { "HoursWorked", hoursWorked },
                { "HourlyRate", hourlyRate },
                { "ExtraNotes", extraNotes },
                { "FileUrls", fileUrls } // Store the file URLs as a comma-separated string
            };
            await _claimsTableClient.AddEntityAsync(claimEntity);
            _logger.LogInformation("Claim submitted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting claim.");
        }
    }

    public async Task<List<TableEntity>> GetClaimsByUserAsync(string userId)
    {
        var claims = new List<TableEntity>();

        try
        {
            var query = _claimsTableClient.QueryAsync<TableEntity>(claim => claim.PartitionKey == userId);

            await foreach (var claim in query)
            {
                claims.Add(claim);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims for user: {UserId}", userId);
        }

        return claims;
    }

    // Get all claims (for Lecturer/Admin)
    public async Task<List<TableEntity>> GetAllClaimsAsync()
    {
        var allClaims = new List<TableEntity>();

        try
        {
            var query = _claimsTableClient.QueryAsync<TableEntity>();

            await foreach (var claim in query)
            {
                allClaims.Add(claim);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all claims");
        }

        return allClaims;
    }

    public async Task<bool> InsertUserAsync(UserEntity user)
    {
        try
        {
            // Check if the user already exists
            var existingUser = await GetUserByEmailAsync(user.RowKey);
            if (existingUser != null)
            {
                // User already exists
                return false;
            }

            // Add the new user if it doesn't exist
            await _userTableClient.AddEntityAsync(user);
            return true;
        }
        catch (RequestFailedException ex)
        {
            // Handle other request failures
            throw new Exception("User could not be added.", ex);
        }
    }

    public async Task RegisterAdminsAsync()
    {
        try
        {
            // Programme Coordinator
            var programmeCoordinator = new UserEntity("programmecoordinator@gmail.com", "ProgrammeCoordinator", "Programme Coordinator")
            {
                PasswordHash = "password123" // Store plain-text password
            };
            await _userTableClient.AddEntityAsync(programmeCoordinator);

            // Academic Manager
            var academicManager = new UserEntity("academicmanager@gmail.com", "AcademicManager", "Academic Manager")
            {
                PasswordHash = "password123" // Store plain-text password
            };
            await _userTableClient.AddEntityAsync(academicManager);

            _logger.LogInformation("Admins registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering admins.");
        }
    }

}
