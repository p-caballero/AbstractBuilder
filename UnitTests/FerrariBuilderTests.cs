namespace AbstractBuilder
{
    using System.Drawing;
    using AbstractBuilder.Sample;
    using ExpectedObjects;
    using Xunit;

    public class FerrariBuilderTests
    {
        private const string Ferrari208Gts = "FERRARI 208 GTS";
        private const string Ferrari308Gtbi = "FERRARI 308 GTBI";
        private const int NumDoors = 3;

        [Fact]
        public void Build_ProtectedCtor_BuildsDefaultObject()
        {
            // Arrange
            FerrariBuilder builder = new FerrariBuilder();

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Car.DefaultColor,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_WithModifications_BuildsModifiedObject()
        {
            // Arrange
            FerrariBuilder builder = new FerrariBuilder()
                .WithColor(Color.Red)
                .WithNumDoors(NumDoors);

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Color.Red.Name,
                NumDoors,
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_MultipleBuilders_BuildsModifiedObject()
        {
            // Arrange
            FerrariBuilder builder = new FerrariBuilder()
                .WithColor(Color.Red)
                .WithNumDoors(NumDoors);

            // Act
            Car actualFerrari208Gts = builder
                .WithModel(Ferrari208Gts)
                .Build();

            Car actualFerrari308Gtbi = builder
                .WithModel(Ferrari308Gtbi)
                .Build();

            // Assert
            Assert.NotNull(actualFerrari208Gts);
            Assert.NotNull(actualFerrari308Gtbi);
            Assert.True(actualFerrari208Gts != actualFerrari308Gtbi);
            Assert.True(actualFerrari208Gts.Id != actualFerrari308Gtbi.Id);

            new
            {
                Color = Color.Red.Name,
                NumDoors,
                Model = Ferrari208Gts
            }.ToExpectedObject().ShouldMatch(actualFerrari208Gts);

            new
            {
                Color = Color.Red.Name,
                NumDoors,
                Model = Ferrari308Gtbi
            }.ToExpectedObject().ShouldMatch(actualFerrari308Gtbi);
        }
    }
}
