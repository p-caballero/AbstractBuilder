namespace AbstractBuilder
{
    using AbstractBuilder.Sample;
    using ExpectedObjects;
    using Xunit;

    public class DatedBookCiteBuilderTests
    {
        private const string TitleUnknown = "Unknown";
        private const string AuthorAnonymous = "Anonymous";

        [Fact]
        public void Build_WithDefaultValues_CreatesCiteWithDefaultValues()
        {
            // Arrange
            DatedBookCiteBuilder builder = new DatedBookCiteBuilder();

            // Act
            BookCite actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Title = TitleUnknown,
                Author = AuthorAnonymous,
                Publisher = (string)null,
                Year = 2022,
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_WithYear_CreatesCiteWithGivenValues()
        {
            // Arrange
            var builder = new DatedBookCiteBuilder()
                .WithYear();

            // Act
            BookCite actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Title = TitleUnknown,
                Author = AuthorAnonymous,
                Publisher = (string)null,
                Year = 1902,
            }.ToExpectedObject().ShouldMatch(actual);
        }
    }
}
