using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Extensions
{
    public static class CtorPassDefaultsForMissingParametersExtensions
    {
        public static IMappingExpression CtorPassDefaultsForMissingParameters(this IMappingExpression mapping,
            Type sourceType, Type destinationType)
        {
            var missingCtorParameters = GetMissingCtorParameters(sourceType, destinationType);

            foreach (var missingCtorParameter in missingCtorParameters)
            {
                mapping.ForCtorParam(missingCtorParameter.Name, cfg =>
                {
                    var defaultValue = GetDefaultValue(missingCtorParameter.Type);
                    cfg.MapFrom((source, resolutionContext) => defaultValue);
                });
            }

            return mapping;
        }
        
        public static IMappingExpression<TSource, TDestination> CtorPassDefaultsForMissingParameters<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var missingCtorParameters = GetMissingCtorParameters(sourceType, destinationType);

            foreach (var missingCtorParameter in missingCtorParameters)
            {
                mapping.ForCtorParam(missingCtorParameter.Name, cfg =>
                {
                    var defaultValue = GetDefaultValue(missingCtorParameter.Type);
                    cfg.MapFrom((source, resolutionContext) => defaultValue);
                });
            }

            return mapping;
        }

        private static IEnumerable<(string Name, Type Type)> GetMissingCtorParameters(Type sourceType, Type destinationType)
        {
            var destinationCtor = destinationType
                .GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .First();

            var sourceProperties = sourceType
                .GetProperties()
                .Where(x => x.CanRead);

            var ctorParameters = destinationCtor.GetParameters();

            var missingCtorParameters = ctorParameters
                .Select(x => (x.Name, x.ParameterType))
                .ExceptBy(sourceProperties.Select(k => k.Name), x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            return missingCtorParameters;
        }

        private static object? GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

    }
}