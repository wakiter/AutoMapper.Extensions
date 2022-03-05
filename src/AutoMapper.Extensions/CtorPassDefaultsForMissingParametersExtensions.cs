using System;
using System.Linq;

namespace AutoMapper.Extensions
{
    public static class CtorPassDefaultsForMissingParametersExtensions
    {
        public static IMappingExpression<TSource, TDestination> CtorPassDefaultsForMissingParameters<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
        {
            var destinationCtor = typeof(TDestination)
                .GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .First();

            var sourceProperties = typeof(TSource)
                .GetProperties()
                .Where(x => x.CanRead);

            var ctorParameters = destinationCtor.GetParameters();

            var missingCtorParameters = ctorParameters
                .Select(x => new {x.Name, Type = x.ParameterType})
                .ExceptBy(sourceProperties.Select(k => k.Name), x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            foreach (var missingCtorParameter in missingCtorParameters)
            {
                mapping.ForCtorParam(missingCtorParameter.Name, cfg =>
                {
                    var defaultValue = GetDefaultValue(missingCtorParameter.Type);
                    cfg.MapFrom((source, resolutionContext) => defaultValue);
                });
            }

            object? GetDefaultValue(Type t)
            {
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }

            return mapping;
        }
    }
}