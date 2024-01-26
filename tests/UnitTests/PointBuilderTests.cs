namespace AbstractBuilder
{
    using AbstractBuilder.Examples.Builders;
    using Xunit;

    public class PointBuilderTests
    {
        [Fact]
        public void Build_RecordStructPoint_BuildsAppliesModification()
        {
            // Arrange
            var builder = new PointBuilder().As2DX10Y20();

            // Act
            var actual = builder.Build();

            // Assert
            Assert.Equal(10, actual.X);
            Assert.Equal(20, actual.Y);
            Assert.Equal(0, actual.Z);
        }
    }
}
