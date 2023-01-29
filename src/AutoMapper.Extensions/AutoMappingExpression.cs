using System;
using System.Collections.Generic;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.Extensions
{
    internal sealed class AutoMappingExpression<TSource, TDestination> : MappingExpression<TSource, TDestination>, IAmAutoMappingExpression
    {
        private readonly List<IAmAutoMappingExpression> _childrenMappingExpression = new List<IAmAutoMappingExpression>();

        public TypePair ConversionPair { get; }
        public IReadOnlyCollection<IAmAutoMappingExpression> ChildrenMappingExpression => _childrenMappingExpression;

        public AutoMappingExpression(TypePair conversionPair)
            : base(
                MemberList.Destination,
                false)
        {
            ConversionPair = conversionPair;
        }

        public IAmAutoMappingExpression AddChildrenMappingExpressions(IEnumerable<IAmAutoMappingExpression> children)
        {
            _childrenMappingExpression.AddRange(children);

            return this;
        }
    }
}