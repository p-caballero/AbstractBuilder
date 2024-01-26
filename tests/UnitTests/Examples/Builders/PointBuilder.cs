namespace AbstractBuilder.Examples.Builders
{
    using AbstractBuilder.Examples.Entities;

    internal class PointBuilder : RecordBuilder<Point>
    {
        public PointBuilder As2DX10Y20()
        {
            return Set<PointBuilder, double>(p => p.X, () => 10)
                .Set<PointBuilder, double>(p => p.Y, () => 20);
        }
    }
}
