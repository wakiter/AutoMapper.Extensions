using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.Extensions
{
    public static class CtorMapParametersExtensions
    {
        public static IMappingExpression<TSource, TDestination> CtorMapParameter<TSource, TDestination, TCtorParameter>(
            this IMappingExpression<TSource, TDestination> mapping,
            Expression<Func<TDestination, TCtorParameter>> ctorParameter,
            Action<ICtorParamConfigurationExpression<TSource>> cfg)
        {
            if (!(ctorParameter.Body is MemberExpression ctorParameterBody))
            {
                throw new CtorParameterIsNotPropertyOrField($"{nameof(ctorParameter)}.{nameof(ctorParameter.Body)} is not of type {nameof(MemberExpression)}!");
            }

            var ctorParameterName = ctorParameterBody.Member.Name;

            var destinationCtor = typeof(TDestination)
                .GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .First();

            var ctorParameters = destinationCtor.GetParameters();

            var constructorParameter = ctorParameters
                .FirstOrDefault(x => string.Equals(x.Name, ctorParameterName, StringComparison.InvariantCultureIgnoreCase));

            if (constructorParameter == null)
            {
                throw new CtorParameterNotFound(ctorParameterName);
            }

            mapping.ForCtorParam(constructorParameter.Name, cfg);

            return mapping;
        }

        public sealed class CtorParameterIsNotPropertyOrField : Exception
        {
            public CtorParameterIsNotPropertyOrField(string message)
            : base(message)
            {
            }
        }

        public sealed class CtorParameterNotFound : Exception
        {
            public CtorParameterNotFound(string ctorParameterName)
                : base(ctorParameterName)
            {
            }
        }
    }
}