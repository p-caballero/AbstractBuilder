namespace AbstractBuilder.Sample
{
    using System;
    using System.Linq;

    internal class BmwBuilder : AbstractBuilder<Car>
    {
        public const char PetrolSuffix = 'i';
        public const char DieselSuffix = 'd';
        private static readonly string[] _models = { "118i", "118d", "218d", "220d", "220i", "225d", "316i", "318i", "320i", "323i", "325i", "325xi", "328i", "328xi", "330i", "330xi", "335i", "335is", "335xi", "320d", "320xd", "325d", "330d", "330xd", "335d", "428i", "435i", "420d", "420i", "420i xDrive", "420d xDrive", "430d", "435d xDrive", "520i", "523i", "528i", "530i", "535i", "518d", "520d", "525d", "530d", "640i", "650i", "640d", "730d", "740d", "750i xDrive", "750Li xDrive", "760i", "760Li", "840Ci", "850i", "850Ci", "850CSi", "860i" };
        private static int _lastId;

        public BmwBuilder()
            : this(context => CreateDefault((BmwBuilderContext)context))
        {
        }

        protected BmwBuilder(Func<BuilderContext, Car> seedFunc)
            : base(seedFunc)
        {
        }

        private static Car CreateDefault(BmwBuilderContext arg)
        {
            var car = new Car
            {
                Id = ++_lastId
            };

            int index = (car.Id - 1) % _models.Length;
            char letter = arg.IsDiesel ? DieselSuffix : PetrolSuffix;
            car.Model = _models.Skip(index).FirstOrDefault(x => x.Split(' ').First().EndsWith(letter)) ??
                        _models.Take(index + 1).FirstOrDefault(x => x.Split(' ').First().EndsWith(letter));

            return car;
        }

        public class BmwBuilderContext : BuilderContext
        {
            public bool IsDiesel { get; set; }
        }
    }
}
