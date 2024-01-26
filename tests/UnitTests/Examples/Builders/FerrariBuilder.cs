namespace AbstractBuilder.Examples.Builders
{
    using System;
    using AbstractBuilder;
    using AbstractBuilder.Examples.Entities;

    internal class FerrariBuilder : AbstractBuilder<Car>
    {
        private static int _lastId;

        public FerrariBuilder()
            : base(CreateDefault)
        {
        }

        protected FerrariBuilder(Func<Car> seedFunc)
            : base(seedFunc)
        {
        }

        public FerrariBuilder WithColor(System.Drawing.Color color)
        {
            return Set<FerrariBuilder>(x => x.Color = color.Name);
        }

        public FerrariBuilder WithNumDoors(int numDoors)
        {
            return Set<FerrariBuilder>(x => x.NumDoors = numDoors);
        }

        public FerrariBuilder WithModel(string model)
        {
            return Set<FerrariBuilder>(x => x.Model = model);
        }

        private static Car CreateDefault()
        {
            return new Car
            {
                Id = ++_lastId
            };
        }
    }
}
