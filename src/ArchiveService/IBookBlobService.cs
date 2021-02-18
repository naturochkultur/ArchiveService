using Azure.Storage.Blobs.Specialized;
using System.Collections.Generic;

namespace ArchiveService
{
    public interface IBookBlobService
    {
        BlockBlobClient GetBlobClient(string containerName, string blobName);
        List<string> GetFileNamesByIsbn(string isbn);
        List<string> GetIsbns();
    }
}