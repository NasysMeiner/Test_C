namespace Test_prod.Models
{
    public class Filter
    {
        public NameFilter? NameFilter { get; set; }
        public Range<DateTime>? RangeDate { get; set; }
        public Range<float>? RangeValue { get; set; }
        public Range<int>? RangeTime { get; set; }
    }
}
