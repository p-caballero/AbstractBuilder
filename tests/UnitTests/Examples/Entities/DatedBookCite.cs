namespace AbstractBuilder.Examples.Entities
{
    internal record DatedBookCite(string Title = "Unknown", string Author = "Anonymous", string Publisher = null, int Year = 2022)
        : BookCite(Title, Author, Publisher)
    {
    }
}
