namespace AbstractBuilder.Benchmarks
{
    using System.Drawing;
    using System.Threading.Tasks;
    using AbstractBuilder.Sample;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;

    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class Test
    {
        [Benchmark]
        public Car FerrariBuilder_MultipleSets_Build()
        {
            return new FerrariBuilder()
                .WithColor(Color.Red)
                .WithModel(FerraryModels.Ferrari208Gts)
                .WithNumDoors(3)
                .Build();
        }

        [Benchmark]
        public Car BmwBuilder_NoSets_Build()
        {
            var builderContext = new BmwBuilder.BmwBuilderContext
            {
                IsDiesel = true
            };

            return new BmwBuilder()
                .Set<BmwBuilder>(x => x.Color = Color.Red.Name)
                .Set<BmwBuilder>(x => x.NumDoors = 3)
                .Build(builderContext);
        }

        [Benchmark]
        public async Task<Car> FerrariBuilder_MultipleSets_BuildAsync()
        {
            return await new FerrariBuilder()
                .WithColor(Color.Red)
                .WithModel(FerraryModels.Ferrari208Gts)
                .WithNumDoors(3)
                .BuildAsync();
        }

        [Benchmark]
        public async Task<Car> BmwBuilder_NoSets_BuildAsync()
        {
            var builderContext = new BmwBuilder.BmwBuilderContext
            {
                IsDiesel = true
            };

            return await new BmwBuilder()
                .Set<BmwBuilder>(x => x.Color = Color.Red.Name)
                .Set<BmwBuilder>(x => x.NumDoors = 3)
                .BuildAsync(builderContext);
        }
    }

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public static void Main(string[] _)
        {
            BenchmarkRunner.Run<Test>();
        }
    }
}
