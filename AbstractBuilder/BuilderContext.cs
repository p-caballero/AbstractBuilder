namespace AbstractBuilder
{
    using System.Threading;

    /// <summary>
    /// Context for the builder.
    /// </summary>
    /// <remarks>If you don't use a context then an default context will be created.</remarks>
    public class BuilderContext
    {
        /// <summary>
        /// Notification that the operation build should be cancelled.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
