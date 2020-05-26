namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class AbstractBuilder<TResult>
    {
        private const string CtorMethodName = ".ctor";

        private const BindingFlags CtorBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Func<BuilderContext, TResult> _seedFunc;

        private readonly Queue<Action<TResult, BuilderContext>> _modifications;


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuilder{TResult}"/> class.
        /// </summary>
        /// <param name="seedFunc">Method to create a new instance</param>
        public AbstractBuilder(Func<TResult> seedFunc)
        {
            if (seedFunc == null)
            {
                throw new ArgumentNullException(nameof(seedFunc));
            }

            _seedFunc = _ => seedFunc();
            _modifications = new Queue<Action<TResult, BuilderContext>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuilder{TResult}"/> class.
        /// </summary>
        /// <param name="seedFunc">Method to create a new instance</param>
        public AbstractBuilder(Func<BuilderContext, TResult> seedFunc)
        {
            _seedFunc = seedFunc ?? throw new ArgumentNullException(nameof(seedFunc));
            _modifications = new Queue<Action<TResult, BuilderContext>>();
        }

        /// <summary>
        /// Attaches new modification(s) in a new director.
        /// This method ignores the cancellation token.
        /// </summary>
        /// <param name="modifications">Actions to modify the object</param>
        /// <returns>A new director</returns>
        public AbstractBuilder<TResult> Set(params Action<TResult>[] modifications)
        {
            var modificationsExtended = modifications?.Select(modification =>
            {
                return new Action<TResult, BuilderContext>((obj, _) =>
                {
                    modification(obj);
                });
            }).ToArray();

            return Set(modificationsExtended);
        }

        /// <summary>
        /// Attaches new modification(s) in a new director
        /// </summary>
        /// <param name="modifications">Actions to modify the object</param>
        /// <returns>A new director</returns>
        public AbstractBuilder<TResult> Set(params Action<TResult, BuilderContext>[] modifications)
        {
            if (modifications == null)
            {
                throw new ArgumentNullException(nameof(modifications));
            }

            if (!modifications.Any())
            {
                throw new ArgumentException(nameof(modifications));
            }

            AbstractBuilder<TResult> builder = CreateBuilder();

            foreach (Action<TResult, BuilderContext> modification in _modifications.Concat(modifications))
            {
                builder._modifications.Enqueue(modification);
            }

            return builder;
        }

        /// <summary>
        /// Attaches new modification(s) in a new builder.
        /// This method ignores the cancellation token
        /// </summary>
        /// <remarks>This is a sugar syntax.</remarks>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <param name="modifications"></param>
        /// <returns>A new director</returns>
        public TBuilder Set<TBuilder>(params Action<TResult>[] modifications)
            where TBuilder : AbstractBuilder<TResult>
        {
            if (!IsSupported<TBuilder>())
            {
                throw new NotSupportedException();
            }

            return (TBuilder)Set(modifications);
        }

        /// <summary>
        /// Attaches new modification(s) in a new builder.
        /// </summary>
        /// <remarks>This is a sugar syntax.</remarks>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <param name="modifications"></param>
        /// <returns>A new director</returns>
        public TBuilder Set<TBuilder>(params Action<TResult, BuilderContext>[] modifications)
            where TBuilder : AbstractBuilder<TResult>
        {
            if (!IsSupported<TBuilder>())
            {
                throw new NotSupportedException();
            }

            return (TBuilder)Set(modifications);
        }

        /// <summary>
        /// Builds a new instance
        /// </summary>
        /// <returns>A new object</returns>
        public virtual TResult Build(BuilderContext builderContext = null)
        {
            var currBuilderContext = builderContext ?? new BuilderContext();
            var cancelTkn = currBuilderContext.CancellationToken;

            cancelTkn.ThrowIfCancellationRequested();
            TResult obj = _seedFunc(currBuilderContext);

            return _modifications.Aggregate(obj, (result, nextModification) =>
            {
                cancelTkn.ThrowIfCancellationRequested();
                nextModification(result, currBuilderContext);
                return result;
            });
        }

        public virtual async Task<TResult> BuildAsync(BuilderContext builderContext = null)
        {
            var currBuilderContext = builderContext ?? new BuilderContext();
            var cancelTkn = currBuilderContext.CancellationToken;
            
            cancelTkn.ThrowIfCancellationRequested();
            TResult obj = await Task.Run(() => _seedFunc(currBuilderContext), cancelTkn);

            foreach (Action<TResult, BuilderContext> modifiction in _modifications)
            {
                cancelTkn.ThrowIfCancellationRequested();
                await Task.Run(() => modifiction(obj, currBuilderContext), cancelTkn);
            }

            return obj;
        }

        private AbstractBuilder<TResult> CreateBuilder()
        {
            var type = GetType();

            ConstructorInfo ctor = type.GetConstructor(CtorBindingFlags, null, new[] { typeof(Func<BuilderContext, TResult>) }, null);

            if (ctor != null)
            {
                return (AbstractBuilder<TResult>)ctor.Invoke(new object[] { _seedFunc });
            }

            ctor = type.GetConstructor(CtorBindingFlags, null, new[] { typeof(Func<TResult>) }, null)
                   ?? throw new MissingMethodException(GetType().Name, CtorMethodName);

            TResult SeedFuncWithoutCancellation() => _seedFunc(null);

            return (AbstractBuilder<TResult>)ctor.Invoke(new object[] { (Func<TResult>)SeedFuncWithoutCancellation });
        }

        /// <summary>
        /// Checks the heritage
        /// </summary>
        /// <typeparam name="TBuilder">Type of the target builder</typeparam>
        /// <returns>true if the heritage is followed, otherwise false</returns>
        private bool IsSupported<TBuilder>()
        {
            Type resultType = typeof(TBuilder);
            Type currentType = GetType();
            return resultType == currentType || resultType.IsInstanceOfType(currentType);
        }
    }
}
