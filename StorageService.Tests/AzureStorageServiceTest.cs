using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace StorageService.Tests
{
    [TestClass]
    public class AzureStorageServiceTest
    {
        [TestMethod]
        public void SaveFileToBlob_ValidScenario_ReturnTrue()
        {
            FileStream stream = new FileStream(@"D:/DSC_0235.jpg", FileMode.Open);
            AzureStorageService service = new AzureStorageService(stream, "AAAA", "test.jpg");
            var response = service.SaveFileToBlob();

            Assert.IsTrue(response.IsSuccess);
        }
    }
}
