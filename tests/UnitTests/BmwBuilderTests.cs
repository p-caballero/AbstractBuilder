namespace AbstractBuilder
{
    using System.Linq;
    using AbstractBuilder.Examples.Builders;
    using AbstractBuilder.Examples.Entities;
    using ExpectedObjects;
    using Xunit;

    public class BmwBuilderTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Build_WithCustomBuilderContext_AccessToContextInConstructor(bool isDiesel)
        {
            // Arrange
            var builder = new BmwBuilder();

            var context = new BmwBuilder.BmwBuilderContext
            {
                IsDiesel = isDiesel
            };

            // Act
            Car actual = builder.Build(context);

            // Assert
            Assert.NotNull(actual);

            new
            {
                Color = Car.DefaultColor,
                NumDoors = default(int),
            }.ToExpectedObject().ShouldMatch(actual);

            bool actualIsDiesel = actual.Model
                .Split(' ')
                .First()
                .EndsWith(BmwBuilder.DieselSuffix);

            Assert.Equal(isDiesel, actualIsDiesel);
        }

        [Fact]
        public void Build_WithCustomBuilderContext_AccessToContextInSet()
        {
            // Arrange
            BuilderContext contextInSetMethod = null;
            var builder = new BmwBuilder()
                .Set((car, builderContext) => contextInSetMethod = builderContext);

            var context = new BmwBuilder.BmwBuilderContext
            {
                IsDiesel = true
            };

            // Act
            Car actual = builder.Build(context);

            // Assert
            Assert.NotNull(actual);

            Assert.Equal(context, contextInSetMethod);
            Assert.True(context.IsDiesel);
        }
    }
}
