using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace PPPK_Zadatak04.Repository
//https://learn.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=visual-studio%2Cmanaged-identity%2Croles-azure-portal%2Csign-in-azure-cli%2Cidentity-visual-studio
{
    public class FileStorageRepository
    {
        private const string ContainerName = "blobcontainer";
        private static string? storageAccKey;
        private static BlobContainerClient? container;

        public static void Initialize(string connectionString,string accountKey)
        {
            container = new BlobServiceClient(connectionString).GetBlobContainerClient(ContainerName);
            storageAccKey = accountKey ?? string.Empty;
        }

        public static BlobContainerClient Container
        {
            get
            {
                if (container == null)
                {
                    throw new InvalidOperationException("Repository has not been initialized. Call Repository.Initialize with the connection string.");
                }

                return container;
            }
        }

        public static async Task UploadFileAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
            var blobName = $"{extension}/{file.FileName}";
            var containerClient = container;
            var blobClient = containerClient?.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }

        public static async Task<Stream> DownloadFileAsync(string blobName)
        {
            var blobClient = Container.GetBlobClient(blobName);
            var blobDownloadInfo = await blobClient.DownloadAsync();
            return blobDownloadInfo.Value.Content;
        }

        public static async Task<IEnumerable<string>> ListFilesAsync(string prefix = null)
        {
            var results = new List<string>();
            await foreach (var blobItem in Container.GetBlobsAsync(prefix: prefix))
            {
                results.Add(blobItem.Name);
            }
            return results;
        }

        public static async Task DeleteFileAsync(string blobName)
        {
            var blobClient = Container.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public static async Task<string?> DisplayFileAsync(string fileName)
        {
            var blobClient = Container.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = Container.Name,
                    BlobName = fileName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var accountKey = storageAccKey;
                var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(Container.AccountName, accountKey)).ToString();

                return $"{blobClient.Uri}?{sasToken}";
            }

            return null;
        }
    }
}
