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

        [HttpPost("WriteData")]
        public async Task<IResult> PostData(string fileName)
        {
            return await _fileReader.WriteData(fileName);
        }

        [HttpPost("Filter")]
        public async Task<List<StatisticsDataCell>> PostFilter(Filter filter)
        {
            return await _fileReader.GetStatistics(filter);
        }

        [HttpGet("LastTask")]
        public async Task<List<DataCell>> GetLastTask(string name)
        {
            return await _fileReader.GetLastTask(name);
        }
    }
}
