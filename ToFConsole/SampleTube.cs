namespace ToFConsole
{
    /// <summary>
    /// Example setup of a sample tube
    /// </summary>
    public class SampleTube
    {
        public string Colour { get; set; }
        public string SpecimenType { get; set; }
        public double Volume { get; set; }
        public string Type => this.GetType().Name;

        public SampleTube(string colour, string specimenType, double volume)
        {
            Colour = colour;
            SpecimenType = specimenType;
            Volume = volume;
        }
    }
}