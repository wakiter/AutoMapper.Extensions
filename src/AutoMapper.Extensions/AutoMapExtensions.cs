using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.Extensions
{
    public static class AutoMapExtensions
    {
        public static IMappingExpression CreateAutoMap(
            this IMapperConfigurationExpression mapperConfigurationExpression,
            Type sourceType, 
            Type destinationType)
        {
            var mappingExpression = mapperConfigurationExpression.CreateMap(sourceType, destinationType);

            if (sourceType == destinationType)
            {
                return mappingExpression;
            }
            
            CreateAutoMapForAllComplexProperties(mapperConfigurationExpression, new TypePair(sourceType, destinationType), new TypePair(typeof(void), typeof(void)), null);

            return mappingExpression;

        }

        public static IMappingExpression<TSource, TDestination> CreateAutoMap<TSource, TDestination>(
            this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            return mapperConfigurationExpression.CreateAutomaticMap<TSource, TDestination>(new TypePair(sourceType, destinationType), new TypePair(typeof(void), typeof(void)), null);
        }

        private static IMappingExpression CreateAutoMap(
            this IMapperConfigurationExpression mapperConfigurationExpression,
            TypePair conversionPair,
            TypePair parentPair,
            IAmAutoMappingExpression parentMappingExpression)
        {
            var mappingExpression = mapperConfigurationExpression.CreateMap(conversionPair.SourceType, conversionPair.DestinationType);

            if (conversionPair.SourceType == conversionPair.DestinationType)
            {
                return mappingExpression;
            }

            CreateAutoMapForAllComplexProperties(mapperConfigurationExpression, conversionPair, parentPair, null); //todo: this null shouldn't be here! it's temporary to check generic version only!

            return mappingExpression;
        }

        private static IMappingExpression<TSource, TDestination> CreateAutomaticMap<TSource, TDestination>(
            this IMapperConfigurationExpression mapperConfigurationExpression, 
            TypePair conversionPair,
            TypePair parentPair, 
            IAmAutoMappingExpression parentMappingExpression)
        {
            return (IMappingExpression<TSource, TDestination>)mapperConfigurationExpression.CreateAutomaticMappingExpression(conversionPair, parentPair, parentMappingExpression);
        }

        private static IAmAutoMappingExpression CreateAutomaticMappingExpression(
            this IMapperConfigurationExpression mapperConfigurationExpression,
            TypePair conversionPair,
            TypePair parentPair,
            IAmAutoMappingExpression parentMappingExpression)
        {
            var profileConfiguration = (IProfileConfiguration)mapperConfigurationExpression;
            var typeMapConfiguration = (IList<ITypeMapConfiguration>)profileConfiguration.TypeMapConfigs;

            var autoMappingExpression = CreateAutoMappingExpression(conversionPair, parentPair, parentMappingExpression);

            typeMapConfiguration.Add(autoMappingExpression);

            if (conversionPair.SourceType == conversionPair.DestinationType)
                return autoMappingExpression;

            var subMaps = CreateAutoMapForAllComplexProperties(mapperConfigurationExpression, conversionPair, parentPair, autoMappingExpression);

            autoMappingExpression.AddChildrenMappingExpressions(subMaps);

            return autoMappingExpression;
        }

        private static IEnumerable<IAmAutoMappingExpression> CreateAutoMapForAllComplexProperties(
            IMapperConfigurationExpression mapperConfigurationExpression,
            TypePair conversionPair,
            TypePair parentPair,
            IAmAutoMappingExpression parentMappingExpression)
        {
            var retVal = new List<IAmAutoMappingExpression>();

            foreach (var sourcePropertyInfo in conversionPair.SourceType
                         .GetProperties()
                         .Where(x => !x.PropertyType.IsSystemType() && !x.PropertyType.IsEnum))
            {
                var correspondingProperty = conversionPair.DestinationType
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == sourcePropertyInfo.Name && !p.PropertyType.IsSystemType() && !p.PropertyType.IsEnum);

                if (correspondingProperty == null)
                {
                    continue;
                }

                if (sourcePropertyInfo.PropertyType.IsGenericType &&
                    correspondingProperty.PropertyType.IsGenericType)
                {
                    var sourceGenericArguments = sourcePropertyInfo.PropertyType.GetGenericArguments();
                    var destinationGenericArguments = correspondingProperty.PropertyType.GetGenericArguments();

                    if (sourceGenericArguments.Length != destinationGenericArguments.Length)
                    {
                        throw new GenericArgumentsCountMismatch(conversionPair.SourceType, sourceGenericArguments, conversionPair.DestinationType, destinationGenericArguments);
                    }

                    for (int i = 0; i < sourceGenericArguments.Length; i++)
                    {
                        var sourceGenericArgument = sourceGenericArguments[i];
                        var destinationGenericArgument = destinationGenericArguments[i];

                        if (!sourceGenericArgument.IsSystemType() 
                            && !sourceGenericArgument.IsEnum
                            && !destinationGenericArgument.IsSystemType()
                            && !destinationGenericArgument.IsEnum)
                        {
                            var mappingExpForSubProperty = mapperConfigurationExpression.CreateAutomaticMappingExpression(new TypePair(sourceGenericArgument, destinationGenericArgument), conversionPair, parentMappingExpression);
                            retVal.Add(mappingExpForSubProperty);
                        }
                    }

                }
                else if (sourcePropertyInfo.PropertyType.IsClass && correspondingProperty.PropertyType.IsClass)
                {
                    var mappingExpForSubProperty = mapperConfigurationExpression.CreateAutomaticMappingExpression(new TypePair(sourcePropertyInfo.PropertyType, correspondingProperty.PropertyType), conversionPair, parentMappingExpression);
                    retVal.Add(mappingExpForSubProperty);
                }
            }

            return retVal;
        }

        private static IAmAutoMappingExpression CreateAutoMappingExpression(TypePair conversionPair,
            TypePair parentPair, IAmAutoMappingExpression parentMappingExpression)
        {
            var openGenericType = typeof(AutoMappingExpression<,>);
            var closedGenericType = openGenericType.MakeGenericType(conversionPair.SourceType, conversionPair.DestinationType);

            var instance = (IAmAutoMappingExpression)Activator.CreateInstance(closedGenericType, conversionPair, parentPair, parentMappingExpression);

            return instance;
        }

        public sealed class CreateAutMapGenericNotFound : Exception
        {
            public CreateAutMapGenericNotFound(int genericParameters)
            : base($"Unable to find method {nameof(CreateAutoMap)} with {genericParameters} generic arguments!")
            {
            }
        }

        public sealed class GenericArgumentsCountMismatch : Exception
        {
            public GenericArgumentsCountMismatch(Type sourceType, Type[] genericSourceTypeArguments, Type destinationType, Type[] genericDestinationTypeArguments)
            : base($"{sourceType} has {genericSourceTypeArguments.Length} generic arguments, whereas {destinationType} has {genericDestinationTypeArguments.Length}!")
            {
            }
        }
    }
}