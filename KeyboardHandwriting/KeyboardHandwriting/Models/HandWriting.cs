namespace KeyboardHandwriting.Models
{
    public class HandWriting
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public double? Pauses { get; set; }
        public double? Holding { get; set; }
        public double? ErrorsCount { get; set; }
        public double? Speed { get; set; }
        public double? Overlapping { get; set; }
    }
}
