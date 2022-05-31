namespace AbstractBuilder.Examples.Builders
{
    using AbstractBuilder;
    using AbstractBuilder.Examples.Entities;

    internal class DatedBookCiteBuilder : RecordBuilder<DatedBookCite>
    {
        public DatedBookCiteBuilder WithYear()
        {
            return Set<DatedBookCiteBuilder, int>(x => x.Year, () => 1902);
        }
    }
}
