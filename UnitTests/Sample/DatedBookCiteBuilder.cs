namespace AbstractBuilder.Sample
{
    public class DatedBookCiteBuilder : RecordBuilder<DatedBookCite>
    {
        public DatedBookCiteBuilder WithYear()
        {
            return Set<DatedBookCiteBuilder, int>(x => x.Year, () => 1902);
        }
    }
}
