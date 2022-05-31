using AbstractBuilder.Examples.Entities;

namespace AbstractBuilder.Examples.Builders
{
    internal class BuilderWithoutSeedCtor : AbstractBuilder<Car>
    {
        public BuilderWithoutSeedCtor()
            : base(() => new Car())
        {
        }
    }
}
