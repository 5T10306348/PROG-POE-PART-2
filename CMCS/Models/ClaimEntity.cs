using Azure;
using Azure.Data.Tables;


namespace CMCS.Models
{
    public class ClaimEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public string ExtraNotes { get; set; }
        public string FileUrl { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ClaimEntity() { }

        public ClaimEntity(string userId, string claimId)
        {
            PartitionKey = userId;
            RowKey = claimId;
        }
    }
}
