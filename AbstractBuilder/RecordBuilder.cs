namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AbstractBuilder.Internal;

    /// <summary>
    /// Builder for records.
    /// </summary>
    /// <typeparam name="TResult">Type of the result of this builder</typeparam>
    public class RecordBuilder<TResult>
    {
        private readonly IDictionary<string, Func<object>> _parameterBuilders = new Dictionary<string, Func<object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordBuilder{TResult}"/> class.
        /// </summary>
        protected RecordBuilder()
        {
        }

        /// <summary>
        /// Attaches new parameter construction in a new director.
        /// </summary>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <param name="selector">Selects the parameter name to be initilized</param>
        /// <param name="parameterBuilder">Parameter builder</param>
        /// <returns>An incremental new director</returns>
        /// <exception cref="MissingFieldException">When the parameter does not exist</exception>
        public TBuilder Set<TBuilder>(Expression<Func<TResult, object>> selector, Func<object> parameterBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var propertyInfo = (PropertyInfo)((MemberExpression)selector.Body).Member;

            return Set<TBuilder>(propertyInfo.Name, parameterBuilder);
        }

        /// <summary>
        /// Attaches new parameter construction in a new director.
        /// </summary>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <typeparam name="TParameter">Type of the parameter</typeparam>
        /// <param name="selector">Selects the parameter name to be initilized</param>
        /// <param name="parameterBuilder"></param>
        /// <returns>An incremental new director</returns>
        /// <exception cref="MissingFieldException">When the parameter does not exist</exception>
        public TBuilder Set<TBuilder, TParameter>(Expression<Func<TResult, TParameter>> selector, Func<TParameter> parameterBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (parameterBuilder == null)
            {
                throw new ArgumentNullException(nameof(parameterBuilder));
            }

            var propertyInfo = (PropertyInfo)((MemberExpression)selector.Body).Member;

            return Set<TBuilder>(propertyInfo.Name, () => parameterBuilder.Invoke());
        }

        /// <summary>
        /// Attaches new parameter construction in a new director.
        /// </summary>
        /// <typeparam name="TBuilder">Type of the builder</typeparam>
        /// <param name="parameterName"></param>
        /// <param name="parameterBuilder"></param>
        /// <returns>An incremental new director</returns>
        /// <exception cref="MissingFieldException">When the parameter does not exist</exception>
        public TBuilder Set<TBuilder>(string parameterName, Func<object> parameterBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException($"Invalid parameter name {parameterName}.", nameof(parameterName));
            }

            if (parameterBuilder == null)
            {
                throw new ArgumentNullException(nameof(parameterBuilder));
            }

            if (!IsSupported<TBuilder>())
            {
                throw new NotSupportedException();
            }

            if (!ExistParameter(parameterName))
            {
                throw new MissingFieldException(typeof(TResult).Name, parameterName);
            }

            return InternalSet<TBuilder>(parameterName, parameterBuilder);
        }

        /// <summary>
        /// Builds a new instance
        /// </summary>
        /// <returns>A new object</returns>
        /// <exception cref="MissingMethodException">When the constructor is not available</exception>
        public TResult Build()
        {
            ConstructorInfo ctor = typeof(TResult).GetConstructors().FirstOrDefault()
                ?? throw new MissingMethodException(typeof(TResult).Name, CtorConstants.MethodName);

            var parameters = ctor.GetParameters()
                .Select(p => BuildParameter(p))
                .ToArray();

            return (TResult)ctor.Invoke(parameters.ToArray());
        }

        /// <summary>
        /// Build the given parameter
        /// </summary>
        /// <param name="parameter">Parameter info</param>
        /// <returns>The value of the parameter for the new object</returns>
        private object BuildParameter(ParameterInfo parameter)
        {
            if (_parameterBuilders.TryGetValue(parameter.Name, out Func<object> parameterBuilder))
            {
                return parameterBuilder.Invoke();
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameter.ParameterType.IsClass)
            {
                return null;
            }

            return Activator.CreateInstance(parameter.ParameterType);
        }

        /// <summary>
        /// Creates a new empty builder
        /// </summary>
        /// <exception cref="MissingMethodException">When the constructor is not available</exception>
        private RecordBuilder<TResult> CreateBuilder()
        {
            var type = GetType();

            ConstructorInfo ctor = type.GetConstructor(CtorConstants.BindingFlags, null, new Type[0], null)
                ?? throw new MissingMethodException(GetType().Name, CtorConstants.MethodName);

            return (RecordBuilder<TResult>)ctor.Invoke(new object[0]);
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
        /// Checks the existence of a parameter in the first found constructor.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>true if it was found, otherwise false</returns>
        private static bool ExistParameter(string name)
        {
            return typeof(TResult).GetConstructors().FirstOrDefault()?.GetParameters().Any(p => p.Name == name) ?? false;
        }

        /// <summary>
        /// Attaches new parameter construction in a new director.
        /// </summary>
        /// <remarks>This method doesn't check the arguments.</remarks>
        /// <typeparam name="TBuilder">Type of the target builder</typeparam>
        /// <param name="parameterName"></param>
        /// <param name="parameterBuilder"></param>
        /// <returns>An incremental new director</returns>
        private TBuilder InternalSet<TBuilder>(string parameterName, Func<object> parameterBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            RecordBuilder<TResult> builder = CreateBuilder();

            foreach (var propBuilders in _parameterBuilders.Where(x => x.Key != parameterName))
            {
                builder._parameterBuilders.Add(propBuilders.Key, propBuilders.Value);
            }
            builder._parameterBuilders.Add(parameterName, () => parameterBuilder.Invoke());

            return (TBuilder)builder;
        }
    }
}
