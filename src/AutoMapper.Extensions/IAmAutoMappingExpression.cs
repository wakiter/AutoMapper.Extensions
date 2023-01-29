using System.Collections.Generic;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.Extensions
{
    internal interface IAmAutoMappingExpression : ITypeMapConfiguration
    {
        TypePair ConversionPair { get; }

        IReadOnlyCollection<IAmAutoMappingExpression> ChildrenMappingExpression { get; }

        IAmAutoMappingExpression AddChildrenMappingExpressions(IEnumerable<IAmAutoMappingExpression> children);
    }
}