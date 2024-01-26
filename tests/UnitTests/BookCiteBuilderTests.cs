namespace AbstractBuilder
{
    using System;
    using System.Linq.Expressions;
    using AbstractBuilder.Examples.Builders;
    using AbstractBuilder.Examples.Entities;
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

        [Fact]
        public void Build_WithAuthorReducedVersion_CreatesCite()
        {
            // Arrange
            BookCiteBuilder builder = new BookCiteBuilder()
                .WithAuthorReducedVersion("Arthur", "Conan Doyle");

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

        [Fact]
        public void Build_UsingSetDirectly_CreatesCite()
        {
            // Arrange
            const string Title = "The Hound of the Baskervilles";
            var builder = new BookCiteBuilder()
                .WithAuthorReducedVersion("Arthur", "Conan Doyle")
                .Set<BookCiteBuilder>(x => x.Title, () => Title);

            // Act
            BookCite actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Title = Title,
                Author = "Conan Doyle, A.",
                Publisher = (string)null,
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Build_UsingTwiceSetForSameParameter_CreatesCite()
        {
            // Arrange
            const string Title = "The Hound of the Baskervilles";
            var builder = new BookCiteBuilder()
                .Set<BookCiteBuilder>(x => x.Title, () => "TEST")
                .Set<BookCiteBuilder>(x => x.Title, () => Title);

            // Act
            BookCite actual = builder.Build();

            // Assert
            Assert.NotNull(actual);

            new
            {
                Title = Title,
                Author = (string)null,
                Publisher = (string)null,
            }.ToExpectedObject().ShouldMatch(actual);
        }

        [Fact]
        public void Set_WrongParameter_ThrowsException()
        {
            // Arrange
            var builder = new BookCiteBuilder();

            // Act && Assert
            Assert.Throws<MissingFieldException>(() => builder.Set<BookCiteBuilder>("Price", () => "29.99€"));
        }

        [Fact]
        public void Set_NullSelector_ThrowsException()
        {
            // Arrange
            var builder = new BookCiteBuilder();

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() => builder.Set<BookCiteBuilder, string>(null, () => "TEST"));
        }

        [Fact]
        public void Set_NullParameterBuilder_ThrowsException()
        {
            // Arrange
            var builder = new BookCiteBuilder();

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() => builder.Set<BookCiteBuilder, string>(x => x.Title, null));
        }

        [Fact]
        public void Set_ReducedWithNullSelector_ThrowsException()
        {
            // Arrange
            var builder = new BookCiteBuilder();

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() => builder.Set<BookCiteBuilder>((Expression<Func<BookCite, object>>)null, () => "TEST"));
        }

        [Fact]
        public void Set_ReducedWithNullParameterBuilder_ThrowsException()
        {
            // Arrange
            var builder = new BookCiteBuilder();

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() => builder.Set<BookCiteBuilder>(x => x.Title, null));
        }
    }
}
