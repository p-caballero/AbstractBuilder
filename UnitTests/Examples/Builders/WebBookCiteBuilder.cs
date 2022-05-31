namespace AbstractBuilder.Examples.Builders
{
    using System;
    using AbstractBuilder;
    using AbstractBuilder.Examples.Entities;

    public class WebBookCiteBuilder : RecordBuilder<WebBookCite>
    {
        public WebBookCiteBuilder WithCite()
        {
            return Set<WebBookCiteBuilder, BookCite>(x => x.Cite, () => new BookCiteBuilder().WithAuthor("Gustavo", "Adolfo", "Bécquer").Build());
        }

        public WebBookCiteBuilder WithUri(Uri uri)
        {
            return Set<WebBookCiteBuilder, string>(x => x.Url, () => uri.ToString());
        }
    }
}
