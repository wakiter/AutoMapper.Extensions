using System;
using System.Collections.Generic;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.Extensions
{
    internal sealed class AutoMappingExpression<TSource, TDestination> : MappingExpression<TSource, TDestination>, IAmAutoMappingExpression
    {
        private readonly List<IAmAutoMappingExpression> _childrenMappingExpression = new List<IAmAutoMappingExpression>();

        public TypePair ParentPair { get; }
        public TypePair ConversionPair { get; }
        public IAmAutoMappingExpression ParentMappingExpression { get; }
        public IReadOnlyCollection<IAmAutoMappingExpression> ChildrenMappingExpression => _childrenMappingExpression;

        public AutoMappingExpression(
            TypePair conversionPair,
            TypePair parentPair,
            IAmAutoMappingExpression parentMappingExpression)
            : base(
                MemberList.Destination,
                false)
        {
            ParentPair = parentPair;
            ConversionPair = conversionPair;
            ParentMappingExpression = parentMappingExpression;
        }

        public IAmAutoMappingExpression AddChildrenMappingExpressions(IEnumerable<IAmAutoMappingExpression> children)
        {
            _childrenMappingExpression.AddRange(children);

            return this;
        }

    }
}