namespace AbstractBuilder
{
    using System.Drawing;
    using AbstractBuilder.Examples.Builders;
    using Xunit;

    public class DieselBmwBuilderTests : IClassFixture<DieselBmwBuilder>
    {
        private readonly DieselBmwBuilder _dieselBmwBuilder;

        public DieselBmwBuilderTests(DieselBmwBuilder dieselBmwBuilder)
        {
            _dieselBmwBuilder = dieselBmwBuilder;
        }

        [Fact]
        public void Build_EmptyCtor_BuildsDefaultObject()
        {
            // Act
            var actual = _dieselBmwBuilder.Build();

            // Assert
            Assert.Equivalent(new
            {
                Color = Color.SlateGray.Name,
                Model = "318d",
                NumDoors = 3
            }, actual);
        }

        [Fact]
        public void Build_EmptyCtorAndOneSet_BuildsDefaultObject()
        {
            // Act
            var actual = _dieselBmwBuilder.WithNumDoors(5).Build();

            // Assert
            Assert.Equivalent(new
            {
                Color = Color.SlateGray.Name,
                Model = "318d",
                NumDoors = 5
            }, actual);
        }
    }
}
