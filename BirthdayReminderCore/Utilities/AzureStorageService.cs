using System.Linq;
using BirthdayReminder.ViewModels;
using BirthdayReminderCore.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace BirthdayReminderCore.Utilities
{
    public class AzureStorageService
    {
        private readonly CloudBlobContainer _container;       

        public AzureStorageService(string connectionString, string userId)
        {
            _container = GetContainerReference(connectionString, userId);
        }

        public async Task<ResponseStatus> SaveFileToBlob(string fileName, FileStream imageStream)
        {
            try
            {
                if (_container==null)
                {
                    return new ResponseStatus{IsSuccess = false,ErrorMessage = "Access denied"};
                } 
                //Create the container if it does not exist
                await _container.CreateIfNotExistsAsync();

                //Make the container public
                await _container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                //Create a reference to the bob file to upload
                var blobReference = _container.GetBlockBlobReference(fileName);

                var fileExists = await blobReference.ExistsAsync();
                //if file already exists, return error
                if (fileExists)
                {
                    return new ResponseStatus { IsSuccess = false, ErrorMessage = "File already exists" }; 
                }

                //Upload the blob from stream
                await blobReference.UploadFromStreamAsync(imageStream);

                return new ResponseStatus { IsSuccess = true, IdentifierData=blobReference.Uri.AbsoluteUri };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { IsSuccess = false, ErrorMessage = ex.Message + ex.StackTrace };
            }
        }

        public async Task<ObservableCollection<CardEntity>> GetBirthdayCards()
        {
            var cards = new ObservableCollection<CardEntity>();

            foreach (var blob in await GetListOfExistingFiles())
            {
                int cardId;

                if (!int.TryParse(blob.Name.Split(new []{'.'})[0], out cardId)) continue;

                cards.Add(new CardEntity { Id = cardId, Url = blob.Uri.AbsoluteUri });
            }

            return cards;
        }

        private async Task<List<CloudBlockBlob>> GetListOfExistingFiles()
        {

            if (_container == null)
            {
                return new List<CloudBlockBlob>();
            } 

            //return blob list object and not birthday card objetc to generalize method
            var blobList = new List<CloudBlockBlob>();

            if (!await _container.ExistsAsync()) return blobList;

            var blobResult = await _container.ListBlobsSegmentedAsync(new BlobContinuationToken());

            //iterate over list of returned blobs
            blobList.AddRange(blobResult.Results.OfType<CloudBlockBlob>());

            return blobList;
        }

        public async Task<ResponseStatus> DeleteBlob(string fileName)
        {
            try
            {
                if (_container == null)
                {
                    return new ResponseStatus { IsSuccess = false, ErrorMessage = "Access denied" };
                } 

                var blob = _container.GetBlockBlobReference(fileName);

                if (await blob.ExistsAsync())
                {
                    await blob.DeleteAsync();
                }
                else
                {
                    return new ResponseStatus { IsSuccess = false, ErrorMessage = "The file with the given filename does not exist" };
                }

                return new ResponseStatus { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { IsSuccess = false, ErrorMessage = ex.Message + ex.StackTrace };
            }
        }

        private static CloudBlobContainer GetContainerReference(string connectionString, string userId)
        {
            try
            {
                //Retrieve connection parameters
                var account = CloudStorageAccount.Parse(connectionString);

                //Create a reference to the client
                var client = account.CreateCloudBlobClient();

                var container = client.GetContainerReference(userId.ToLower());

                return container;
            }
            catch (Exception)
            {
                return null;
            }
        }        
    }
}
