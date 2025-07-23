namespace Test_prod.Models
{
    public class StatisticsDataCell
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public double DeltaDate { get; set; }
        public DateTime MinDateTime { get; set; }
        public float AverageExecutionTime { get; set; }
        public float AverageValue { get; set; }
        public float MedianValue { get; set; }
        public float MaxValue { get; set; }
        public float MinValue { get; set; }
    }
}
