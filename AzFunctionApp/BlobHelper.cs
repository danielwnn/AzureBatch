using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;
using System.IO;


namespace AzFunctionApp1
{
    public static class BlobHelper
    {
        /// <summary>
        /// Creates a blob client
        /// </summary>
        /// <param name="storageAccountName">The name of the Storage Account</param>
        /// <param name="storageAccountKey">The key of the Storage Account</param>
        /// <returns></returns>
        public static CloudBlobClient CreateCloudBlobClient(string storageAccountName, string storageAccountKey)
        {
            // Construct the Storage account connection string
            string storageConnectionString =
                $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}";

            // Retrieve the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }

        public static string GetBlobSasUri(CloudBlockBlob blob, int days = 8)
        {
            SharedAccessBlobPolicy adHocSas =
            new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(days),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasBlobToken = blob.GetSharedAccessSignature(adHocSas);
            return blob.Uri + sasBlobToken;
        }


        public static string GetBlobSasUri(CloudBlobContainer container, string blobName, int days = 8)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            return GetBlobSasUri(blob);
        }


        public static string GetContainerSasUri(CloudBlobClient blobClient, string containerName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();


            SharedAccessBlobPolicy adHocSas =
                new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(8),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write
                };
            var sasContainerToken = container.GetSharedAccessSignature(adHocSas);
            return container.Uri + sasContainerToken;
        }


        public static CloudBlockBlob GetLatestBlobByExtension(CloudBlobContainer container, string extension)
        {
            string ext = extension.First() == '.' ? extension : "." + extension;


            CloudBlockBlob blockBlob =
              container.ListBlobs().OfType<CloudBlockBlob>()
                .Where(b => Path.GetExtension(b.Name).Equals(ext)).OrderByDescending(c => c.Properties.Created).First();

            return blockBlob;
        }
    }
}
