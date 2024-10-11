using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class FileService
{
    private readonly BlobContainerClient _blobContainer;
    private readonly ILogger<FileService> _logger;

    public FileService(IConfiguration configuration, ILogger<FileService> logger)
    {
        string storageConnectionString = configuration.GetSection("AzureStorage")["ConnectionString"];
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
        _blobContainer = blobServiceClient.GetBlobContainerClient("claimfiles");

        _logger = logger;

        try
        {
            _blobContainer.CreateIfNotExists();
            _logger.LogInformation("Azure Blob Container 'claimfiles' created or already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating the blob container.");
        }
    }

    public async Task<(string FileName, string FileUrl)> UploadFileAsync(IFormFile file)
    {
        try
        {
            // Use the original file name to upload
            string fileName = file.FileName;
            BlobClient blobClient = _blobContainer.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            _logger.LogInformation("File uploaded successfully. Blob URI: {BlobUri}", blobClient.Uri);

            // Return the original file name and its URL
            return (fileName, blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Blob Storage.");
            throw;
        }
    }
}
