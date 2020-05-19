namespace AbstractBuilder.Sample
{
    internal class BuilderWithoutSeedCtor : AbstractBuilder<Car>
    {
        public BuilderWithoutSeedCtor()
            : base(() => new Car())
        {
        }
    }
}
