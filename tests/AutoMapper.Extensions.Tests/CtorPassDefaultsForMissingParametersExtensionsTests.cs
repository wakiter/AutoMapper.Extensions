using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Extensions.Tests;

public sealed class CtorPassDefaultsForMissingParametersExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Mapper_passes_default_values_for_missing_arguments()
    {
        var input = _fixture.Create<SourceClass>();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg
                .CreateAutoMap<SourceClass, DestinationClass>()
            //.CtorPassDefaultsForMissingParameters()
                .CtorMapParameter(x => x.PropF, cfg => cfg.MapFrom((source, resolution) => "aaaaa"));
        }).CreateMapper();

        var actual = sut.Map<DestinationClass>(input);
        
        actual
            .Should()
            .BeEquivalentTo(new
            {
                PropA = input.PropA,
                PropB = input.PropB,
                PropC = default(bool),
                PropD = default(decimal),
                PropE = default(DestinationClass.ValueType)
            });
    }

    private sealed class SourceClass
    {
        public string PropA { get; set; }

        public int PropB { get; set; }
    }

    private sealed class DestinationClass
    {
        public string PropA { get; }

        public int PropB { get; }

        //public Boolean PropC { get; }

        //public decimal PropD { get; }

        //public ValueType PropE { get; }

        public string PropF { get; }

        public DestinationClass(string propA, int propB, /*DateTimeOffset propC,*/ /*decimal propD*//*, ValueType propE*/ string propF)
        {
            PropA = propA;
            PropB = propB;
            //PropC = propC;
            //PropD = propD;
            //PropE = propE;
            PropF = propF;
        }

        public struct ValueType
        {
            public int A { get; set; } = 123;
        }
    }
}