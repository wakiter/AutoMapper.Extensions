using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

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
            
            CreateAutoMapCommon(mapperConfigurationExpression, sourceType, destinationType, (mce, source, target) => CreateAutoMap(mce, source, target));

            return mappingExpression;

        }

        public static IMappingExpression<TSource, TDestination> CreateAutoMap<TSource, TDestination>(
            this IMapperConfigurationExpression mapperConfigurationExpression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var mappingExpression = mapperConfigurationExpression.CreateMap<TSource, TDestination>();

            if (sourceType == destinationType)
            {
                return mappingExpression;
            }

            CreateAutoMapCommon(mapperConfigurationExpression, sourceType, destinationType, InvokeCreateAutoMapGeneric);

            return mappingExpression;
        }

        private static void CreateAutoMapCommon(
            IMapperConfigurationExpression mapperConfigurationExpression,
            Type sourceType,
            Type destinationType,
            Action<IMapperConfigurationExpression, Type, Type> recursiveInvocationAction)
        {
            foreach (var sourcePropertyInfo in sourceType
                         .GetProperties()
                         .Where(x => !x.PropertyType.IsSystemType() && !x.PropertyType.IsEnum))
            {
                var correspondingProperty = destinationType
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
                        throw new GenericArgumentsCountMismatch(sourceType, sourceGenericArguments, destinationType, destinationGenericArguments);
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
                            recursiveInvocationAction(mapperConfigurationExpression, sourceGenericArgument, destinationGenericArgument);
                        }
                    }

                }
                else if (sourcePropertyInfo.PropertyType.IsClass && correspondingProperty.PropertyType.IsClass)
                {
                    recursiveInvocationAction(mapperConfigurationExpression, sourcePropertyInfo.PropertyType, correspondingProperty.PropertyType);
                }
            }
        }

        private static void InvokeCreateAutoMapGeneric(
            IMapperConfigurationExpression mapperConfigurationExpression,
            Type sourceType, 
            Type destinationType)
        {
            var genericArgumentTypes = new[] { sourceType, destinationType };
            var openGenericCreateAutoMapMethod = typeof(AutoMapExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.Name == nameof(CreateAutoMap))
                .Where(x =>
                {
                    var genericArguments = x.GetGenericArguments();
                    if (genericArguments.Length == 0)
                    {
                        return false;
                    }

                    return true;
                })
                .SingleOrDefault();

            if (openGenericCreateAutoMapMethod == null)
            {
                throw new CreateAutMapGenericNotFound(genericArgumentTypes.Length);
            }

            var closedGenericCreateAutoMapMethod = openGenericCreateAutoMapMethod.MakeGenericMethod(genericArgumentTypes);

            closedGenericCreateAutoMapMethod
                .Invoke(null, new object[]{mapperConfigurationExpression});
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