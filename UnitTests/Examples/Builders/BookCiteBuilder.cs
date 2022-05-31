namespace AbstractBuilder.Examples.Builders
{
    using AbstractBuilder;
    using AbstractBuilder.Examples.Entities;

    internal class BookCiteBuilder : RecordBuilder<BookCite>
    {
        public BookCiteBuilder WithAuthor(string firstName, string lastName)
        {
            return Set<BookCiteBuilder, string>(x => x.Author, () => $"{lastName}, {firstName[..1].ToUpperInvariant()}.");
        }

        public BookCiteBuilder WithAuthor(string firstName, string midName, string lastName)
        {
            return Set<BookCiteBuilder, string>(x => x.Author, () => $"{lastName}, {firstName[..1].ToUpperInvariant()}. {midName[..1].ToUpperInvariant()}.");
        }
    }
}
