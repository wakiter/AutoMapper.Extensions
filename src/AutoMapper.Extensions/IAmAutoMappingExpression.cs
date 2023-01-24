using System.Collections.Generic;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.Extensions
{
    internal interface IAmAutoMappingExpression : ITypeMapConfiguration
    {
        TypePair ParentPair { get; }

        TypePair ConversionPair { get; }

        IAmAutoMappingExpression ParentMappingExpression { get; }

        IReadOnlyCollection<IAmAutoMappingExpression> ChildrenMappingExpression { get; }

        IAmAutoMappingExpression AddChildrenMappingExpressions(IEnumerable<IAmAutoMappingExpression> children);
    }
}