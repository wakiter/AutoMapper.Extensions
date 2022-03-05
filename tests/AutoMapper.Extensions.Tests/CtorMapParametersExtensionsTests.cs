using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Extensions.Tests;

public sealed class CtorMapParametersExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Mapper_uses_configuration_defined_to_map_to_ctor_properties()
    {
        var input = _fixture.Create<SourceClass>();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<SourceClass, DestinationClass>()
                .CtorMapParameter(p => p.PropA, opt => opt.MapFrom(k => k.ToString()))
                .CtorMapParameter(p => p.PropB, opt => opt.MapFrom(k => k.ToString().GetHashCode()));

        }).CreateMapper();

        var actual = sut.Map<DestinationClass>(input);

        actual.Should().BeEquivalentTo(new
        {
            PropA = input.ToString(),
            PropB = input.ToString().GetHashCode()
        });
    }

    [Fact]
    public void Mapper_throws_an_exception_when_ctorParameter_argument_does_not_point_to_the_corresponding_property()
    {
        Action act = () => new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<SourceClass, DestinationClass>()
                .CtorMapParameter(p => p.ToString(), opt => opt.MapFrom(k => k.ToString()));

        }).CreateMapper();

        act.Should().Throw<Extensions.CtorMapParametersExtensions.CtorParameterIsNotPropertyOrField>();
    }

    [Fact]
    public void Mapper_throws_an_exception_when_ctorParameter_could_not_be_matched_with_a_constructor_argument()
    {
        Action act = () => new MapperConfiguration(cfg =>
        {
            cfg
                .CreateMap<SourceClass, AnotherDestinationClass>()
                .CtorMapParameter(p => p.PropA, opt => opt.MapFrom(k => k.ToString()));

        }).CreateMapper();

        act.Should().Throw<Extensions.CtorMapParametersExtensions.CtorParameterNotFound>();
    }

    sealed class SourceClass
    {
        public string PropA { get; set; }

        public int PropB { get; set; }
    }

    sealed class DestinationClass
    {
        public string PropA { get; }

        public int PropB { get; }

        public DestinationClass(string propA, int propB)
        {
            PropA = propA;
            PropB = propB;
        }
    }

    sealed class AnotherDestinationClass
    {
        public string PropA { get; }

        public int PropB { get; }

        public AnotherDestinationClass(string propAWithChangedNameInTheCtor, int propB)
        {
            PropA = propAWithChangedNameInTheCtor;
            PropB = propB;
        }
    }
}