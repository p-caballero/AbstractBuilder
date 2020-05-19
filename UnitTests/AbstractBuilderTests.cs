namespace AbstractBuilder
{
    using System;
    using System.Drawing;
    using AbstractBuilder.Sample;
    using ExpectedObjects;
    using Xunit;

    public class AbstractBuilderTests
    {
        [Fact]
        public void Build_DefaultSeed_BuildsDefaultObject()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

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
        public void Build_OneModification_BuildsAppliesModification()
        {
            // Arrange
            AbstractBuilder<Car> builder = new AbstractBuilder<Car>(() => new Car())
                .Set(x => x.Color = Color.Red.Name);

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Color.Red.Name,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_MoreThanOneModifications_BuildsAppliesIncrementalModification()
        {
            // Arrange
            AbstractBuilder<Car> builder = new AbstractBuilder<Car>(() => new Car())
                .Set(x => x.Color = Color.Red.Name, x => x.Color = Color.Blue.Name);

            // Act
            Car actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Color.Blue.Name,
                NumDoors = default(int),
                Model = Car.DefaultModel
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Set_NullModifications_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                builder.Set(null);
            });
        }

        [Fact]
        public void Set_EmptyModifications_ReturnsSimilarBuilder()
        {
            // Arrange
            var builder = new AbstractBuilder<Car>(() => new Car());

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                builder.Set();
            });
        }

        [Fact]
        public void Set_BuilderWithoutSeedCtor_ThrowsMissingMethodException()
        {
            // Arrange
            var builder = new BuilderWithoutSeedCtor();

            // Act & Assert
            Assert.Throws<MissingMethodException>(() =>
            {
                builder.Set(x => x.NumDoors = 5);
            });
        }
    }
}
