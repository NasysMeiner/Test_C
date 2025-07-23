using System.Globalization;
using Test_prod.Models;

namespace Test_prod.Services
{
    public class FileReader
    {
        private readonly PostgreRepository _repository;

        private int _maxCountLine = 10000;
        private int _minCountLine = 1;

        private DateTime _minDateTime = new DateTime(2000, 1, 1);

        public FileReader(PostgreRepository repository)
        {
            _repository = repository;
        }

        public async Task<IResult> WriteData(string fileName)
        {
            if (fileName == "" || fileName == null)
                return Results.BadRequest("Not correct file name");

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName + ".txt");

            if (!System.IO.File.Exists(filePath))
                return Results.BadRequest("File not found");

            IResult err = CheckValid(filePath);

            if (err != Results.Ok())
                return err;

            IResult res = await _repository.AddFileData(fileName, filePath);

            return res;
        }

        private IResult CheckValid(string filePath)
        {
            try
            {
                using StreamReader sr = new(filePath);
                string line;
                int count = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    count++;

                    if (count > _maxCountLine || count < _minCountLine)
                        return Results.BadRequest("File is too large!");

                    string[] parts = line.Split(';');

                    if (parts.Length != 3)
                        return Results.BadRequest("Incorrect data entry! (Date;ExecutionTime;Value)");

                    if (DateTime.TryParseExact(parts[0], "yyyy-MM-ddTHH-mm-ss.fffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
                    {
                        if (dateTime >= DateTime.UtcNow || dateTime < _minDateTime)
                            return Results.BadRequest("Not correct date!");
                    }
                    else
                    {
                        return Results.BadRequest($"Error convert date! (Date: {parts[0]})");
                    }

                    if (!Int32.TryParse(parts[1], out int time) && time < 0)
                        return Results.BadRequest($"Error convert! (ExecutionTime: {parts[1]})");

                    if (!Single.TryParse(parts[2], out float value) && value < 0)
                        return Results.BadRequest($"Error convert! (Value: {parts[2]})");
                }
            }
            catch (Exception)
            {
                return Results.BadRequest($"Error open file: {filePath}");
            }

            return Results.Ok();
        }
    }
}
