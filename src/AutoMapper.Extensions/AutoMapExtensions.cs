using System;
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

            foreach (var sourcePropertyInfo in sourceType.GetProperties().Where(x => !x.PropertyType.IsSystemType()))
            {
                var correspondingProperty = destinationType
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == sourcePropertyInfo.Name && !p.PropertyType.IsSystemType());

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

                        if (!sourceGenericArgument.IsSystemType() && !destinationGenericArgument.IsSystemType())
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

            foreach (var sourcePropertyInfo in sourceType.GetProperties().Where(x => !x.PropertyType.IsSystemType()))
            {
                var correspondingProperty = destinationType
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == sourcePropertyInfo.Name && !p.PropertyType.IsSystemType());

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

                        if (!sourceGenericArgument.IsSystemType() && !destinationGenericArgument.IsSystemType())
                        {
                            InvokeCreateAutoMapGeneric(mapperConfigurationExpression, sourceGenericArgument, destinationGenericArgument);
                        }
                    }

                }
                else if (sourcePropertyInfo.PropertyType.IsClass && correspondingProperty.PropertyType.IsClass)
                {
                    InvokeCreateAutoMapGeneric(mapperConfigurationExpression, sourcePropertyInfo.PropertyType, correspondingProperty.PropertyType);
                }
            }

            return mappingExpression;
        }

        private static void InvokeCreateAutoMapGeneric(
            IMapperConfigurationExpression mapperConfigurationExpression,
            Type sourceType, 
            Type destinationType)
        {
            var genericArgumentTypes = new[] { sourceType, destinationType };
            var openGenericCreateAutoMapMethod = typeof(AutoMapExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x =>
                {
                    if (x.Name != nameof(CreateAutoMap))
                    {
                        return false;
                    }

                    var genericArguments = x.GetGenericArguments();
                    if (genericArguments.Length == 0)
                    {
                        return false;
                    }

                    if (genericArguments.Length != genericArgumentTypes.Length)
                    {
                        throw new CreateAutoMapGenericArgumentsCountMismatch(genericArguments.Length, genericArgumentTypes.Length);
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

        public sealed class CreateAutoMapGenericArgumentsCountMismatch : Exception
        {
            public CreateAutoMapGenericArgumentsCountMismatch(int expectedGenericParameters, int receivedGenericParameters)
                : base($"{nameof(CreateAutoMap)} expects {expectedGenericParameters} parameters, but received {receivedGenericParameters}!")
            {
            }
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