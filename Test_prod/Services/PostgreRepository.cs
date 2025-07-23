using System.Globalization;
using Test_prod.Data;
using Test_prod.Models;

namespace Test_prod.Services
{
    public class PostgreRepository
    {
        private readonly PostgreDbContext _context;

        private DateTime _minDateTime = new(0001, 1, 1);

        public PostgreRepository(PostgreDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> AddFileData(string fileName, string filePath)
        {
            StatisticsDataCell statisticsData = new() 
            { 
                FileName = fileName,
                MaxValue = Int32.MinValue,
                MinValue = Int32.MaxValue,
            };

            int count = 0;

            DateTime maxDateTime = new(2000, 1, 1);
            int sumTime = 0;
            float sumValue = 0;
            List<float> floats = new();

            using StreamReader sr = new(filePath); 
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(';');

                DateTime dateTime;

                if (!DateTime.TryParseExact(parts[0], "yyyy-MM-ddTHH-mm-ss.fffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime))
                    return Results.BadRequest($"Error convert date! (Date: {parts[0]})");

                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                int time = Int32.Parse(parts[1]);
                float value = Single.Parse(parts[2]);

                DataCell cell = new()
                {
                    FileName = fileName,
                    Time = dateTime,
                    ExecutionTime = time,
                    Value = value
                };

                count++;

                if (statisticsData.MinDateTime == _minDateTime || statisticsData.MinDateTime > dateTime)
                    statisticsData.MinDateTime = dateTime;

                if (maxDateTime < dateTime)
                    maxDateTime = dateTime;

                sumTime += time;
                sumValue += value;

                if (statisticsData.MinValue > value)
                    statisticsData.MinValue = value;

                if (statisticsData.MaxValue < value)
                    statisticsData.MaxValue = value;

                floats.Add(value);

                await _context.Values.AddAsync(cell);
            }

            statisticsData.DeltaDate = (maxDateTime - statisticsData.MinDateTime).TotalSeconds;

            statisticsData.AverageValue = sumValue / count;
            statisticsData.AverageExecutionTime = sumTime / count;

            floats = floats.OrderBy(x => x).ToList();

            float medianValue = 0;

            if (count % 2 == 0)
                medianValue = (floats[count / 2] + floats[(count / 2) - 1]) / 2;
            else
                medianValue = floats[count / 2];

            statisticsData.MedianValue = medianValue;

            await _context.Results.AddAsync(statisticsData);
            await _context.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
