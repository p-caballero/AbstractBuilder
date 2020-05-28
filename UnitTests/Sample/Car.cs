namespace AbstractBuilder.Sample
{
    public class Car
    {
        public const string DefaultModel = "UNKNOW MODEL";

        public static readonly string DefaultColor = System.Drawing.Color.Black.Name;

        public int Id { get; set; }

        public string Model { get; set; } = DefaultModel;

        public int NumDoors { get; set; }

        public string Color { get; set; } = DefaultColor;
    }
}
