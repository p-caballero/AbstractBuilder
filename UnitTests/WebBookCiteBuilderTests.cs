namespace AbstractBuilder
{
    using System;
    using AbstractBuilder.Examples.Builders;
    using AbstractBuilder.Examples.Entities;
    using ExpectedObjects;
    using Xunit;

    public class WebBookCiteBuilderTests
    {
        [Fact]
        public void Build_RecordWithRecordAndDefaultValues_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder();

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Cite = (BookCite)null,
                Url = (string)null,
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_RecordWithRecord_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithCite()
                .WithUri(new UriBuilder("http", "localhost", 8080).Uri);

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Cite = new
                {
                    Author = "Bécquer, G. A."
                },
                Url = "http://localhost:8080/",
            }.ToExpectedObject().ShouldMatch(actual);
        }
    }
}
