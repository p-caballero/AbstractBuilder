namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AbstractBuilder.Internal;

    public class RecordBuilder<TResult>
    {
        private readonly IDictionary<string, Func<object>> _propertyBuilders = new Dictionary<string, Func<object>>();

        protected RecordBuilder()
        {
        }

        public TBuilder Set<TBuilder>(Expression<Func<TResult, object>> selector, Func<object> propertyBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)selector.Body).Member;
            return Set<TBuilder>(propertyInfo.Name, () => propertyBuilder.Invoke());
        }

        public TBuilder Set<TBuilder, TProperty>(Expression<Func<TResult, TProperty>> selector, Func<TProperty> propertyBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)selector.Body).Member;
            return Set<TBuilder>(propertyInfo.Name, () => propertyBuilder.Invoke());
        }

        public TBuilder Set<TBuilder>(string propertyName, Func<object> propertyBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            if (!IsSupported<TBuilder>())
            {
                throw new NotSupportedException();
            }

            if (!ExistParameter(propertyName))
            {
                throw new MissingFieldException(typeof(TResult).Name, propertyName);
            }

            RecordBuilder<TResult> builder = CreateBuilder();

            foreach (var propBuilders in _propertyBuilders.Where(x => x.Key != propertyName))
            {
                builder._propertyBuilders.Add(propBuilders.Key, propBuilders.Value);
            }
            builder._propertyBuilders.Add(propertyName, () => propertyBuilder.Invoke());

            return (TBuilder)builder;
        }

        public TResult Build()
        {
            ConstructorInfo ctor = typeof(TResult).GetConstructors().FirstOrDefault()
                ?? throw new MissingMethodException(typeof(TResult).Name, CtorConstants.MethodName);

            var parameters = ctor.GetParameters()
                .Select(p => BuildParameter(p))
                .ToArray();

            return (TResult)ctor.Invoke(parameters.ToArray());
        }

        private object BuildParameter(ParameterInfo parameter)
        {
            if (_propertyBuilders.TryGetValue(parameter.Name, out Func<object> propertyBuilder))
            {
                return propertyBuilder.Invoke();
            }
            else if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }
            else if (parameter.ParameterType.IsClass)
            {
                return null;
            }
            else
            {
                return Activator.CreateInstance(parameter.ParameterType);
            }
        }

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

        private static bool ExistParameter(string name)
        {
            return typeof(TResult).GetConstructors().FirstOrDefault()?.GetParameters().Any(p => p.Name == name) ?? false;
        }
    }
}
