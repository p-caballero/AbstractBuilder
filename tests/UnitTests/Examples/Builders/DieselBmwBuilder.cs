namespace AbstractBuilder.Examples.Builders
{
    using System.Drawing;
    using AbstractBuilder.Examples.Entities;

    public sealed class DieselBmwBuilder : AbstractBuilder<Car>
    {
        private static int _lastId;

        public DieselBmwBuilder WithNumDoors(int numDoors)
        {
            return Set<DieselBmwBuilder>(x => x.NumDoors = numDoors);
        }

        protected override Car CreateDefault()
        {
            return new Car
            {
                Id = ++_lastId,
                Color = Color.SlateGray.Name,
                Model = "318d",
                NumDoors = 3
            };
        }
    }
}
