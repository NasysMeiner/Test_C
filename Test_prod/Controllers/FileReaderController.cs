using Microsoft.AspNetCore.Mvc;
using Test_prod.Models;
using Test_prod.Services;

namespace Test_prod.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileReaderController : ControllerBase
    {
        private FileReader _fileReader;

        public FileReaderController(FileReader fileReader)
        {
            _fileReader = fileReader;
        }

        [HttpGet(Name = "Write")]
        public async Task<IResult> Post(string fileName)
        {
            IResult res = await _fileReader.WriteData(fileName);
            Console.WriteLine(res.ToString());

            return res;
        }
    }
}
