using Azure;
using Azure.Data.Tables;

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } // Role, e.g., "User"
    public string RowKey { get; set; } // Email
    public string PasswordHash { get; set; }
    public string FullName { get; set; } // Add this for the user's full name
    public string ProfilePictureUrl { get; set; }

    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public UserEntity() { }

    public UserEntity(string email, string role, string fullName)
    {
        PartitionKey = role;
        RowKey = email;
        FullName = fullName; // Store the full name
    }
}

