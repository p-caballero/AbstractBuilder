namespace AbstractBuilder.Examples.Builders
{
    using AbstractBuilder.Examples.Entities;

    internal class BuilderWithoutSeedCtor : AbstractBuilder<Car>
    {
        public BuilderWithoutSeedCtor()
            : base(() => new Car())
        {
        }
    }
}
