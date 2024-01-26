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

        [Fact]
        public void Build_RecordWithStringWithoutTargetParameter_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithUriWithoutTargetParameter(new UriBuilder("http", "localhost", 8080).Uri);

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Url = "http://localhost:8080/",
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_RecordWithInteger_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithYear(2023);

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Year = 2023
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_RecordWithChar_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithInitial('A');

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Initial = 'A'
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_RecordWithBool_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithFree();

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Free = true
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_RecordWithNullableDouble_CreatesAnEntityWithEmptyValues()
        {
            // Arrange
            var builder = new WebBookCiteBuilder()
                .WithPrice(100.05);

            // Act
            WebBookCite actual = builder.Build();

            // Assert
            new
            {
                Price = 100.05
            }.ToExpectedObject().ShouldMatch(actual);
        }
    }
}
