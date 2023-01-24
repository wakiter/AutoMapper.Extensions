using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;

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
            var autoMappingExpression = mapping as IAmAutoMappingExpression;

            if (autoMappingExpression != null)
            {
                SetupCtorPassDefaultsForMissingParameters(autoMappingExpression);
            }
            else
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
            
            return mapping;
        }

        private static void SetupCtorPassDefaultsForMissingParameters(
            this IAmAutoMappingExpression autoMappingExpression)
        {
            var missingCtorParameters = GetMissingCtorParameters(autoMappingExpression.ConversionPair.SourceType, autoMappingExpression.ConversionPair.DestinationType);

            foreach (var missingCtorParameter in missingCtorParameters)
            {
                InvokeSetDefaultCtorParam(autoMappingExpression, missingCtorParameter.Name, missingCtorParameter.Type);
            }

            foreach (var childAutoMappingExpression in autoMappingExpression.ChildrenMappingExpression)
            {
                SetupCtorPassDefaultsForMissingParameters(childAutoMappingExpression);
            }
        }

        private static void InvokeSetDefaultCtorParam(
            this IAmAutoMappingExpression autoMappingExpression,
            string parameterName, 
            Type parameterType)
        {
            var setDefaultCtorParamOpenGenericMethodInfo = typeof(CtorPassDefaultsForMissingParametersExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.ReturnType == typeof(void) && x.Name == nameof(SetDefaultCtorParam))
                .Single(x => x.GetGenericArguments().Length == 2);

            var closedGenericMethod = setDefaultCtorParamOpenGenericMethodInfo.MakeGenericMethod(autoMappingExpression.SourceType, autoMappingExpression.DestinationType);

            closedGenericMethod.Invoke(null, new object[] { autoMappingExpression, parameterName, parameterType });
        }

        private static void SetDefaultCtorParam<TSource, TDestination>(
            IAmAutoMappingExpression mappingExpression,
            string parameterName,
            Type parameterType)
        {
            var defaultValue = parameterType.GetDefaultValue();
            Func<TSource, ResolutionContext, object?> mapFromResolver = (source, context) => defaultValue;

            Action<ICtorParamConfigurationExpression<TSource>> cfgMethod = cfg => { cfg.MapFrom(mapFromResolver); };
            ((IMappingExpression<TSource, TDestination>)mappingExpression).ForCtorParam(parameterName, cfgMethod);
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

        private static object? GetDefaultValue(this Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

    }
}