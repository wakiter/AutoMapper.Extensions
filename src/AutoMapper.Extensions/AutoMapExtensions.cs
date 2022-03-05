using System;
using System.Linq;

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

            if (sourceType == destinationType) return mappingExpression;

            foreach (var sourcePropertyInfo in sourceType.GetProperties().Where(x => !IsSystemType(x.PropertyType)))
            {
                var correspondingProperty = destinationType
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == sourcePropertyInfo.Name && !IsSystemType(p.PropertyType));

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
                        throw new GenericArgumentsCountMismatch(sourceType, sourceGenericArguments, destinationType, destinationGenericArguments); 
                    }

                    for (int i = 0; i < sourceGenericArguments.Length; i++)
                    {
                        var sourceGenericArgument = sourceGenericArguments[i];
                        var destinationGenericArgument = destinationGenericArguments[i];

                        if (!IsSystemType(sourceGenericArgument) && !IsSystemType(destinationGenericArgument))
                        {
                            CreateAutoMap(mapperConfigurationExpression, sourceGenericArgument, destinationGenericArgument);
                        }
                    }

                }
                else if (sourcePropertyInfo.PropertyType.IsClass && correspondingProperty.PropertyType.IsClass)
                {
                    CreateAutoMap(mapperConfigurationExpression, sourcePropertyInfo.PropertyType, correspondingProperty.PropertyType);
                }
            }

            return mappingExpression;

            bool IsSystemType(Type type)
            {
                return type.Namespace == "System";
            }
        }

        public static IMappingExpression<TSource, TDestination> CreateAutoMap<TSource, TDestination>(
            this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var mappingExpression = mapperConfigurationExpression.CreateMap<TSource, TDestination>();

            CreateAutoMap(mapperConfigurationExpression, sourceType, destinationType);

            return mappingExpression;
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