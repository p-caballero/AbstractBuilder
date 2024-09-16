namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using AbstractBuilder.Internal;

    /// <summary>
    /// Generic abstract builder.
    /// </summary>
    /// <typeparam name="TResult">Type of the result of this builder</typeparam>
    public class AbstractBuilder<TResult>
    {
        private readonly Func<BuilderContext, TResult> _seedFunc;

        private readonly Queue<Action<TResult, BuilderContext>> _modifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuilder{TResult}"/> class.
        /// </summary>
        /// <remarks>Default constructor.</remarks>
        /// <param name="seedFunc">Method to create a new instance</param>
        public AbstractBuilder(Func<TResult> seedFunc = null)
        {
            if (seedFunc == null)
            {
                _seedFunc = _ => CreateDefault();
            }
            else
            {
                _seedFunc = _ => seedFunc();
            }

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
        /// <returns>An incremental new director</returns>
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
        /// <returns>An incremental new director</returns>
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
        /// <param name="modifications">Actions to modify the object</param>
        /// <returns>An incremental new director</returns>
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
        /// <returns>An incremental new director</returns>
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

        /// <summary>
        /// Creates an instance of <see cref="AbstractBuilder{TResult}"/> using various constructor strategies.
        /// </summary>
        /// <returns>An instance of <see cref="AbstractBuilder{TResult}"/>.</returns>
        /// <exception cref="MissingMethodException">Thrown when no suitable constructor is found.</exception>
        private AbstractBuilder<TResult> CreateBuilder()
        {
            if (TryCreateBuilderWithBuilderContext(out AbstractBuilder<TResult> result))
            {
                return result;
            }

            if (TryCreateBuilderWithFunction(out result))
            {
                return result;
            }

            if (TryCreateBuilderWithDefaultCtor(out result))
            {
                return result;
            }

            throw new MissingMethodException(GetType().Name, CtorConstants.MethodName);
        }

        /// <summary>
        /// Attempts to create an instance of <see cref="AbstractBuilder{TResult}"/> using a constructor that accepts a <see cref="Func{BuilderContext, TResult}"/>.
        /// </summary>
        /// <param name="type">The type of the builder.</param>
        /// <param name="result">The created builder instance, if successful.</param>
        /// <returns><c>true</c> if the builder was successfully created; otherwise, <c>false</c>.</returns>
        private bool TryCreateBuilderWithBuilderContext(out AbstractBuilder<TResult> result)
        {
            ConstructorInfo ctor = GetType().GetConstructor(CtorConstants.BindingFlags, null, new[] { typeof(Func<BuilderContext, TResult>) }, null);

            if (ctor != null)
            {
                result = (AbstractBuilder<TResult>)ctor.Invoke(new object[] { _seedFunc });
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Attempts to create an instance of <see cref="AbstractBuilder{TResult}"/> using a constructor that accepts a <see cref="Func{TResult}"/>.
        /// </summary>
        /// <param name="type">The type of the builder.</param>
        /// <param name="result">The created builder instance, if successful.</param>
        /// <returns><c>true</c> if the builder was successfully created; otherwise, <c>false</c>.</returns>
        private bool TryCreateBuilderWithFunction(out AbstractBuilder<TResult> result)
        {
            ConstructorInfo ctor = GetType().GetConstructor(CtorConstants.BindingFlags, null, new[] { typeof(Func<TResult>) }, null);

            if (ctor != null)
            {
                TResult SeedFuncWithoutCancellation() => _seedFunc(null);

                result = (AbstractBuilder<TResult>)ctor.Invoke(new object[] { (Func<TResult>)SeedFuncWithoutCancellation });
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Attempts to create an instance of <see cref="AbstractBuilder{TResult}"/> using a parameterless constructor.
        /// </summary>
        /// <param name="type">The type of the builder.</param>
        /// <param name="result">The created builder instance, if successful.</param>
        /// <returns><c>true</c> if the builder was successfully created; otherwise, <c>false</c>.</returns>
        private bool TryCreateBuilderWithDefaultCtor(out AbstractBuilder<TResult> result)
        {
            ConstructorInfo ctor = GetType().GetConstructor(CtorConstants.BindingFlags, null, Type.EmptyTypes, null);

            if (ctor != null)
            {
                result = (AbstractBuilder<TResult>)ctor.Invoke(null);
                return true;
            }

            result = null;
            return false;
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

        /// <summary>
        /// Creates a default instance of <see cref="TResult"/>.
        /// </summary>
        /// <remarks>It is not used when a seed function is provided.</remarks>
        protected virtual TResult CreateDefault()
        {
            return default;
        }
    }
}
