namespace AbstractBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class RecordBuilder<TResult>
    {
        private readonly IDictionary<string, Func<object>> _propertyBuilders = new Dictionary<string, Func<object>>();

        protected RecordBuilder()
        {
        }

        public TBuilder Set<TBuilder, TProperty>(Expression<Func<TResult, TProperty>> selector, Func<TProperty> propertyBuilder)
            where TBuilder : RecordBuilder<TResult>
        {
            if (!IsSupported<TBuilder>())
            {
                throw new NotSupportedException();
            }

            var propertyInfo = (PropertyInfo)((MemberExpression)selector.Body).Member;

            string propertyName = propertyInfo.Name;

            RecordBuilder<TResult> builder = CreateBuilder();

            foreach (var propBuilders in _propertyBuilders.Where(x => x.Key != propertyName))
            {
                builder._propertyBuilders.Add(propBuilders.Key, propBuilders.Value);
            }
            builder._propertyBuilders.Add(propertyName, () => propertyBuilder.Invoke());

            return (TBuilder)builder;
        }

        private RecordBuilder<TResult> CreateBuilder()
        {
            var type = GetType();

            ConstructorInfo ctor = type.GetConstructor(CtorConstants.BindingFlags, null, new Type[0], null)
                ?? throw new MissingMethodException(GetType().Name, CtorConstants.MethodName);

            return (RecordBuilder<TResult>)ctor.Invoke(new object[0]);
        }

        public TResult Build()
        {
            ConstructorInfo ctor = typeof(TResult).GetConstructors().FirstOrDefault()
                ?? throw new MissingMethodException(typeof(TResult).Name, CtorConstants.MethodName);

            var parameters = new List<object>();
            foreach (var parameter in ctor.GetParameters())
            {
                if (_propertyBuilders.TryGetValue(parameter.Name, out Func<object> propertyBuilder))
                {
                    parameters.Add(propertyBuilder.Invoke());
                }
                else if (parameter.HasDefaultValue)
                {
                    parameters.Add(parameter.DefaultValue);
                }
                else
                {
                    parameters.Add(CreateDefaultValue(parameter.ParameterType));
                }
            }

            return (TResult)ctor.Invoke(parameters.ToArray());
        }

        private object CreateDefaultValue(Type type)
        {
            return type.IsClass ? null : Activator.CreateInstance(type);
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
