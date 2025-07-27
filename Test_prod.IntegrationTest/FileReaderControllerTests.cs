using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Test_prod.Models;

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

            string fileName = "Test1";

            FileStream file;

            try
            {
                file = File.Create(fileName + ".txt");
                file.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error create file: {ex.Message}");
            }

            string encodedFileName = Uri.EscapeDataString(fileName);
            string url = $"FileReader/WriteData?fileName={encodedFileName}";

            HttpResponseMessage response = await httpClient.PostAsync(url, null);

            string text = await response.Content.ReadAsStringAsync();

            try
            {
                File.Delete(fileName + ".txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error delete file: {ex.Message}");
            }

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task PostDataSendRequestReturnNotFound()
        {
            WebApplicationFactory<Test_prod.Controllers.FileReaderController> web = new WebApplicationFactory<Test_prod.Controllers.FileReaderController>().WithWebHostBuilder(_ => { });

            HttpClient httpClient = web.CreateClient();

            string fileName = "Test2";

            string encodedFileName = Uri.EscapeDataString(fileName);
            string url = $"FileReader/WriteData?fileName={encodedFileName}";

            HttpResponseMessage response = await httpClient.PostAsync(url, null);

            string text = await response.Content.ReadAsStringAsync();

            Console.WriteLine(text);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task PostDataSendRequestIncorrectData()
        {
            WebApplicationFactory<Test_prod.Controllers.FileReaderController> web = new WebApplicationFactory<Test_prod.Controllers.FileReaderController>().WithWebHostBuilder(_ => { });

            HttpClient httpClient = web.CreateClient();

            string fileName = "Test3";
            FileStream file;

            string encodedFileName = Uri.EscapeDataString(fileName);
            string url = $"FileReader/WriteData?fileName={encodedFileName}";

            List<string> data = new List<string>
            {
                "incorrect!",
                "incorrect!",
                "incorrect!",
                "incorrect!",
            };

            try
            {
                file = File.Create(fileName + ".txt");
                using StreamWriter sw = new StreamWriter(file);

                foreach (string line in data)
                    sw.WriteLine(line);

                file.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error create file: {ex.Message}");
            }

            HttpResponseMessage response = await httpClient.PostAsync(url, null);

            try
            {
                File.Delete(fileName + ".txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error delete file: {ex.Message}");
            }

            string text = await response.Content.ReadAsStringAsync();

            Console.WriteLine(text);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetDataSendRequestReturnData()
        {
            WebApplicationFactory<Test_prod.Controllers.FileReaderController> web = new WebApplicationFactory<Test_prod.Controllers.FileReaderController>().WithWebHostBuilder(_ => { });

            HttpClient httpClient = web.CreateClient();

            string fileName = "file1";

            string encodedFileName = Uri.EscapeDataString(fileName);
            string url = $"FileReader/LastTask?name={encodedFileName}";

            HttpResponseMessage response = await httpClient.GetAsync(url);

            string text = await response.Content.ReadAsStringAsync();

            Console.WriteLine(text);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task PostDataSendRequestUseFilterInData()
        {
            WebApplicationFactory<Test_prod.Controllers.FileReaderController> web = new WebApplicationFactory<Test_prod.Controllers.FileReaderController>().WithWebHostBuilder(_ => { });

            HttpClient httpClient = web.CreateClient();

            Filter data = new Filter
            {
                NameFilter = new NameFilter { Name = "file1" },
                RangeDate = new Range<DateTime> { Max = DateTime.Parse("2025-07-27T11:27:04.901Z").ToUniversalTime(), Min = DateTime.Parse("2020-07-27T11:27:04.901Z").ToUniversalTime() },
                RangeValue = new Range<float> { Max = 20, Min = 10 },
                RangeTime = new Range<int> { Max = 200, Min = 10}
            };

            var jsonContent = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("FileReader/Filter", jsonContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);

            Assert.That(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
