using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService
{
    public class BookBlobService : IBookBlobService
    {
        BlobServiceClient _blobServiceClient;

        public BookBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public List<string> GetIsbns()
        {
            var containers = _blobServiceClient.GetBlobContainers();
            var first = containers.FirstOrDefault();
            return containers.Select(c => c.Name).ToList();
        }

        public List<string> GetFileNamesByIsbn(string isbn)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(isbn);
            if (containerClient == null)
            {
                return null;
            }
            var blobs = containerClient.GetBlobs();
            return blobs.Select(b => b.Name).ToList();
        }

        public BlockBlobClient GetBlobClient(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            if (containerClient == null)
            {
                return null;
            }

            var blockBlob = containerClient.GetBlockBlobClient(blobName);
            return blockBlob;
        }
    }
}
