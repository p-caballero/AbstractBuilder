namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class AbstractBuilder<TResult>
    {
        private const string ConstructorMethodName = ".ctor";

        private readonly Func<TResult> _seedFunc;

        private readonly Queue<Action<TResult>> _modifications;


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuilder{TResult}"/> class.
        /// </summary>
        /// <param name="seedFunc">Method to create a new instance</param>
        public AbstractBuilder(Func<TResult> seedFunc)
        {
            _seedFunc = seedFunc ?? throw new ArgumentNullException(nameof(seedFunc));
            _modifications = new Queue<Action<TResult>>();
        }

        /// <summary>
        /// Attaches new modification(s) in a new director
        /// </summary>
        /// <param name="modifications">Actions to modify the object</param>
        /// <returns>A new director</returns>
        public AbstractBuilder<TResult> Set(params Action<TResult>[] modifications)
        {
            if (modifications == null)
            {
                throw new ArgumentNullException(nameof(modifications));
            }

            if (!modifications.Any())
            {
                throw new ArgumentException(nameof(modifications));
            }

            var builder = (AbstractBuilder<TResult>)GetConstructor().Invoke(new object[] { _seedFunc });

            foreach (Action<TResult> modification in _modifications)
            {
                builder._modifications.Enqueue(modification);
            }

            foreach (Action<TResult> modification in modifications)
            {
                builder._modifications.Enqueue(modification);
            }

            return builder;
        }

        /// <summary>
        /// Attaches new modification(s) in a new builder.
        /// </summary>
        /// <remarks>This is a sugar syntax.</remarks>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <param name="modifications"></param>
        /// <returns></returns>
        public TBuilder Set<TBuilder>(params Action<TResult>[] modifications)
            where TBuilder : AbstractBuilder<TResult>
        {
            Type resultType = typeof(TBuilder);
            Type currentType = GetType();
            if (resultType != currentType && !resultType.IsInstanceOfType(currentType))
            {
                throw new NotSupportedException();
            }

            return (TBuilder)Set(modifications);
        }

        /// <summary>
        /// Builds a new instance
        /// </summary>
        /// <returns>A new object</returns>
        public virtual TResult Build()
        {
            return _modifications.Aggregate(_seedFunc(), (result, next) =>
            {
                next(result);
                return result;
            });
        }

        public virtual async Task<TResult> BuildAsync(CancellationToken? cancellationToken = null)
        {
            var currentCnclTkn = cancellationToken ?? CancellationToken.None;

            TResult obj = await Task.Run(_seedFunc, currentCnclTkn);

            foreach (Action<TResult> action in _modifications.TakeWhile(action => !currentCnclTkn.IsCancellationRequested))
            {
                await Task.Run(() => action(obj), currentCnclTkn);
            }

            currentCnclTkn.ThrowIfCancellationRequested();

            return obj;
        }

        /// <summary>
        /// Gets the constructor info for the current builder.
        /// </summary>
        private ConstructorInfo GetConstructor()
        {
            Type currentType = GetType();
            ConstructorInfo ctor = currentType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Func<TResult>) }, null);
            return ctor ?? throw new MissingMethodException(currentType.Name, ConstructorMethodName);
        }
    }
}
