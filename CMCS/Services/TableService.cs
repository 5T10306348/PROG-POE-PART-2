using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task<TableEntity> GetUserByEmailAsync(string email)
    {
        try
        {
            var query = _userTableClient.Query<TableEntity>(entity => entity.PartitionKey == email);
            return query.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user.");
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
        try
        {
            var query = _claimsTableClient.Query<TableEntity>(entity => entity.PartitionKey == userId);
            return query.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims.");
            return new List<TableEntity>();
        }
    }
}
