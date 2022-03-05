using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Xunit;

namespace AutoMapper.Extensions.Tests;

public sealed class CtorMapFixedValueExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Mapper_assigns_fixed_values_to_ctor_properties()
    {
        var input = _fixture.Create<SourceClass>();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<SourceClass, DestinationClass>()
                .CtorMapParameter(p => p.FixedPropA, "fixedPropAValue")
                .CtorMapParameter(p => p.FixedPropB, 123)
                .CtorMapParameter(p => p.FixedPropC, new DateTime(2022, 01, 01));

        }).CreateMapper();

        var actual = sut.Map<DestinationClass>(input);

        actual.Should().BeEquivalentTo(input, opt => opt.Excluding((IMemberInfo mi) => mi.Name.StartsWith("Fixed")));

        actual.Should().BeEquivalentTo(new
        {
            FixedPropA = "fixedPropAValue",
            FixedPropB = 123,
            FixedPropC = new DateTime(2022, 01, 01)
        });
    }

    private sealed class SourceClass
    {
        public string PropA { get; set; }

        public int PropB { get; set; }

        public DateTime PropC { get; set; }
    }

    private sealed class DestinationClass
    {
        public string PropA { get;  }

        public int PropB { get; }

        public DateTime PropC { get; }

        public string FixedPropA { get;  }

        public int FixedPropB { get; }

        public DateTime FixedPropC { get; }

        public DestinationClass(string propA, int propB, DateTime propC, string fixedPropA, int fixedPropB, DateTime fixedPropC)
        {
            PropA = propA;
            PropB = propB;
            PropC = propC;
            FixedPropA = fixedPropA;
            FixedPropB = fixedPropB;
            FixedPropC = fixedPropC;
        }
    }

}