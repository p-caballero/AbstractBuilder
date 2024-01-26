namespace AbstractBuilder.Examples.Builders
{
    using System;
    using AbstractBuilder;
    using AbstractBuilder.Examples.Entities;

    internal class WebBookCiteBuilder : RecordBuilder<WebBookCite>
    {
        public WebBookCiteBuilder WithCite()
        {
            return Set<WebBookCiteBuilder, BookCite>(x => x.Cite, () => new BookCiteBuilder().WithAuthor("Gustavo", "Adolfo", "Bécquer").Build());
        }

        public WebBookCiteBuilder WithUri(Uri uri)
        {
            return Set<WebBookCiteBuilder, string>(x => x.Url, () => uri.ToString());
        }

        public WebBookCiteBuilder WithUriWithoutTargetParameter(Uri uri)
        {
            return Set<WebBookCiteBuilder>(x => x.Url, () => uri.ToString());
        }

        public WebBookCiteBuilder WithYear(int year)
        {
            return Set<WebBookCiteBuilder>(x => x.Year, () => year);
        }

        public WebBookCiteBuilder WithInitial(char initial)
        {
            return Set<WebBookCiteBuilder>(x => x.Initial, () => initial);
        }

        public WebBookCiteBuilder WithFree()
        {
            return Set<WebBookCiteBuilder>(x => x.Free, () => true);
        }

        public WebBookCiteBuilder WithPrice(double price)
        {
            return Set<WebBookCiteBuilder>(x => x.Price, () => price);
        }
    }
}
