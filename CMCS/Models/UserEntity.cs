using Azure;
using Azure.Data.Tables;

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } // This will store the role (e.g., 'Lecturer')
    public string RowKey { get; set; } // This will store the email (which acts as the unique ID)
    public string PasswordHash { get; set; } // Store the hashed password

    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public UserEntity() { }

    public UserEntity(string email, string role)
    {
        PartitionKey = role;
        RowKey = email;
    }
}
