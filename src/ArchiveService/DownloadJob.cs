using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService
{
    public class DownloadJob : IJob
    {
        public readonly IBookBlobService _bookBlobService;
        public readonly IConfiguration _configuration;
        public readonly string archivePath;
        public bool IsRunning;
        public DownloadJob(IBookBlobService bookBlobService, IConfiguration configuration)
        {
            _bookBlobService = bookBlobService;
            _configuration = configuration;
            archivePath = _configuration.GetValue<string>("ArchiveDirectory");
            IsRunning = false;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            var isbns = _bookBlobService.GetIsbns();
            if (isbns == null || !isbns.Any())
            {
                return;
            }

            foreach (var isbn in isbns)
            {
                var files = _bookBlobService.GetFileNamesByIsbn(isbn);
                if (files == null || !files.Any())
                {
                    return;
                }

                foreach (var filename in files)
                {
                  await DownloadFile(isbn, filename);
                }

            }

            IsRunning = false;
        }

        private async Task DownloadFile(string container, string filename)
        {
            var filePath = @$"{ archivePath }\{ container}\{ filename}";
            if (File.Exists(filePath))
            {

                await File.AppendAllLinesAsync(@$"{archivePath}\downloads.txt", new[] { $"{DateTime.Now} - Filen redan nedladdad {filename}" });
                return;
            }

            try
            {
                var startTime = DateTime.Now;
                var blockBlob = _bookBlobService.GetBlobClient(container, filename);
                Directory.CreateDirectory(@$"{archivePath}\{container}\");
                using (var fileStream = File.OpenWrite(filePath))
                {
                    blockBlob.DownloadTo(fileStream);
                }
                var finished = DateTime.Now;
                await File.AppendAllLinesAsync(@$"{archivePath}\DownloadLog.txt", new[] { $" ISBN {container} Start {startTime} Finished: {finished} Filename: {filename}" });
            }
            catch (Exception e)
            {
                throw;
            }          
        }
    }
}
