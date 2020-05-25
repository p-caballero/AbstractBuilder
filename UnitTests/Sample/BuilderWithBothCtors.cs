namespace AbstractBuilder.Sample
{
    using System;
    using System.Collections.Generic;

    internal class BuilderWithBothCtors : AbstractBuilder<Car>
    {
        public enum CtrType
        {
            EmptyCtx,
            EmptyNoCtx,
            SeedCtx,
            SeedNoCtx
        }

        public static IList<CtrType> Ctors { get; } = new List<CtrType>();

        public BuilderWithBothCtors()
            : this(() => new Car())
        {
            Ctors.Add(CtrType.EmptyNoCtx);
        }

        protected BuilderWithBothCtors(Func<Car> seedFunc)
            : base(seedFunc)
        {
            Ctors.Add(CtrType.SeedNoCtx);
        }

        protected BuilderWithBothCtors(Func<BuilderContext, Car> seedFunc)
            : base(seedFunc)
        {
            Ctors.Add(CtrType.SeedCtx);
        }
    }
}
