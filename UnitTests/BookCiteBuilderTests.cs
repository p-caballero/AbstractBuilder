namespace AbstractBuilder
{
    using AbstractBuilder.Sample;
    using ExpectedObjects;
    using Xunit;

    public class BookCiteBuilderTests
    {
        [Fact]
        public void Build_WithAuthor_CreatesCite()
        {
            // Arrange
            BookCiteBuilder builder = new BookCiteBuilder()
                .WithAuthor("Arthur", "Conan Doyle");

            // Act
            BookCite actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Title = (string)null,
                Author = "Conan Doyle, A.",
                Publisher = (string)null,
            }.ToExpectedObject().ShouldMatch(actual);
        }
    }
}
