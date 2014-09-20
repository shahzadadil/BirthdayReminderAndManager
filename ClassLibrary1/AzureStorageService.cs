using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace StorageService
{
    public class AzureStorageService
    {
        private FileStream ImageStream;
        private string UserId;
        private string FileName;

        public AzureStorageService(FileStream imageStream, string userId, string fileName)
        {
            ImageStream = imageStream;
            UserId = userId;
            FileName = fileName;
        }

        public ResponseStatus SaveFileToBlob()
        {
            try
            {
                CloudBlobContainer container = GetContainerReference();

                //Create the container if it does not exist
                container.CreateIfNotExists();

                //Make the container public
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                //Create a reference to the bob file to upload
                CloudBlockBlob blobReference = container.GetBlockBlobReference(FileName);


                //if file already exists, return error
                if (blobReference.Exists())
                {
                    return new ResponseStatus { IsSuccess = false, ErrorMessage = "File already exists" }; 
                }

                //Upload the blob from stream
                blobReference.UploadFromStream(ImageStream);

                return new ResponseStatus { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { IsSuccess = false, ErrorMessage = ex.Message + ex.StackTrace };
            }
        }

        public List<BlobModel> GetListOfExistingFiles()
        {
            try
            {
                CloudBlobContainer container = GetContainerReference();
                List<BlobModel> blobList=new List<BlobModel>();

                //iterate over list of returned blobs
                foreach (var blob in container.ListBlobs())
                {
                    if (blob.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blockBlob = (CloudBlockBlob)blob;
                        blobList.Add(new BlobModel { Name = blockBlob.Name, Uri = blockBlob.Uri.AbsoluteUri });
                    }
                }

                return blobList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ResponseStatus DeleteBlob()
        {
            try
            {
                CloudBlobContainer container = GetContainerReference();

                CloudBlockBlob blob = container.GetBlockBlobReference(FileName);

                blob.Delete();

                return new ResponseStatus { IsSuccess = true };
            }
            catch (Exception)
            {
                return new ResponseStatus { IsSuccess = false, ErrorMessage = ex.Message + ex.StackTrace };
            }
        }

        public CloudBlobContainer GetContainerReference()
        {
            try
            {
                //Retrieve connection parameters
                CloudStorageAccount account = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString.ToString());

                //Create a reference to the client
                CloudBlobClient client = account.CreateCloudBlobClient();

                //Get the container reference
                return client.GetContainerReference(UserId.ToLower());
            }
            catch (Exception)
            {
                throw;
            }
        }        
    }
}
