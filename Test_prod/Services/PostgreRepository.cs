using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Test_prod.Data;
using Test_prod.Models;

namespace Test_prod.Services
{
    public class PostgreRepository
    {
        private readonly PostgreDbContext _context;

        private readonly DateTime _minDateTime = new(0001, 1, 1);

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

        public async Task<List<StatisticsDataCell>> GetStatistics(Filter filter)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM \"Results\" WHERE 1=1");
            var parameters = new List<object>();

            int count = 0;

            if(filter.NameFilter != null)
            {
                sb.Append($" AND \"FileName\" ILIKE @p{count++}");
                parameters.Add($"%{filter.NameFilter.Name}%");
            }

            if(filter.RangeDate != null)
            {
                sb.Append($" AND \"MinDateTime\" BETWEEN @p{count++} AND @p{count++}");
                parameters.Add(filter.RangeDate.Min);
                parameters.Add(filter.RangeDate.Max);
            }

            if(filter.RangeValue != null)
            {
                sb.Append($" AND \"AverageValue\" BETWEEN @p{count++} AND @p{count++}");
                parameters.Add(filter.RangeValue.Min);
                parameters.Add(filter.RangeValue.Max);
            }

            if(filter.RangeTime != null)
            {
                sb.Append($" AND \"AverageExecutionTime\" BETWEEN @p{count++} AND @p{count++}");
                parameters.Add(filter.RangeTime.Min);
                parameters.Add(filter.RangeTime.Max);
            }

            return await _context.Results
                .FromSqlRaw(sb.ToString(), parameters.ToArray())
                .ToListAsync();
        }

        public async Task<List<DataCell>> GetLastTask(string name)
        {
            return await _context.Values
                .FromSqlInterpolated($"SELECT * FROM \"Values\" WHERE \"FileName\" = '{name}' ORDER BY \"Time\" ASC LIMIT 10")
                .ToListAsync();
        }
    }
}
