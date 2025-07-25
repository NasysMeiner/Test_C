using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Test_prod.IntegrationTest
{
    [TestFixture]
    public class FileReaderControllerTests
    {
        [Test]
        public async Task PostDataSendRequestShouldReturnNotFound()
        {
            WebApplicationFactory<Test_prod.Controllers.FileReaderController> web = new WebApplicationFactory<Test_prod.Controllers.FileReaderController>().WithWebHostBuilder(_ => { });

            HttpClient httpClient = web.CreateClient();

            TestContext.WriteLine(Directory.GetCurrentDirectory());

            string fileName = "test/Test1";
            string encodedFileName = Uri.EscapeDataString(fileName);
            string url = $"FileReader/WriteData?fileName={encodedFileName}";

            HttpResponseMessage response = await httpClient.PostAsync(url, null);

            string text = await response.Content.ReadAsStringAsync();

            TestContext.WriteLine(text);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
