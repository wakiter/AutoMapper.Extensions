using System;
using System.Linq.Expressions;

namespace AutoMapper.Extensions
{
    public static class CtorMapFixedValueExtensions
    {
        public static IMappingExpression<TSource, TDestination> CtorMapParameter<TSource, TDestination, TCtorParameter>(
            this IMappingExpression<TSource, TDestination> mapping,
            Expression<Func<TDestination, TCtorParameter>> ctorParameter,
            TCtorParameter value)
        {
            return mapping.CtorMapParameter(ctorParameter, cfg => cfg.MapFrom((source, resolvingContext) => value));
        }
    }
}