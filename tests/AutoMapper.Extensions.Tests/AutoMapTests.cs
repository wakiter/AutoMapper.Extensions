using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AutoMapper.Extensions.Tests;

public sealed class AutoMapTests
{
    private readonly IFixture _fixture = new Fixture();

    public AutoMapTests()
    {
        AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(20))).WhenTypeIs<DateTimeOffset>();

                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(20))).WhenTypeIs<DateTime>();

                return options;
            });
    }

    [Fact]
    public void CreateAutoMap_generic_creates_the_proper_map()
    {
        var input = _fixture.Create<AutoMapTestRootSource>();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg.CreateAutoMap<AutoMapTestRootSource, AutoMapTestRootDestination>();
        }).CreateMapper();

        var actual = sut.Map<AutoMapTestRootDestination>(input);

        actual.Should().BeEquivalentTo(input, opt => opt.Excluding(x => x.MissingPropertyInDestinationObject));
    }

    [Fact]
    public void CreateAutoMap_non_generic_creates_the_proper_map()
    {
        var input = _fixture.Create<AutoMapTestRootSource>();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg.CreateAutoMap(typeof(AutoMapTestRootSource), typeof(AutoMapTestRootDestination));
        }).CreateMapper();

        var actual = sut.Map<AutoMapTestRootDestination>(input);

        actual.Should().BeEquivalentTo(input, opt => opt.Excluding(x => x.MissingPropertyInDestinationObject));
    }

    private sealed class AutoMapTestRootSource
    {
        public string PropA { get; set; }

        public int PropB { get; set; }

        public Uri PropC { get; set; }

        public AutoMapSubClassA PropD { get; set; }

        public AutoMapSubClassB PropE { get; set; }

        public DateTime PropF { get; set; }

        public DateTimeOffset PropG { get; set; }

        public IEnumerable<DateTimeOffset> PropH { get; set; }

        public IEnumerable<AutoMapSubClassA> PropI { get; set; }

        public IEnumerable<int> MissingPropertyInDestinationObject { get; set; }

        public IDictionary<string, AutoMapSubClassA> PropJ { get; set; }

        public sealed class AutoMapSubClassA
        {
            public string PropAOfAutoMapSubClassA { get; set; }

            public int PropBOfAutoMapSubClassA { get; set; }

            public sealed class AutoMapSubSubClassA
            {
                public string PropAOfAutoMapSubSubClassA { get; set; }

                public int PropBOfAutoMapSubSubClassA { get; set; }
            }
        }

        public sealed class AutoMapSubClassB
        {
            public string PropAOfAutoMapSubClassB { get; set; }

            public int PropBOfAutoMapSubClassB { get; set; }
        }
    }

    private sealed class AutoMapTestRootDestination
    {
        public string PropA { get; set; }

        public int PropB { get; set; }

        public Uri PropC { get; set; }

        public AutoMapSubClassA PropD { get; set; }

        public AutoMapSubClassB PropE { get; set; }

        public DateTime PropF { get; set; }

        public DateTimeOffset PropG { get; set; }

        public IEnumerable<DateTimeOffset> PropH { get; set; }

        public IEnumerable<AutoMapSubClassA> PropI { get; set; }

        public IEnumerable<int> MissingPropertyInSourceObject { get; set; }

        public IDictionary<string, AutoMapSubClassA> PropJ { get; set; }

        public sealed class AutoMapSubClassA
        {
            public string PropAOfAutoMapSubClassA { get; set; }

            public int PropBOfAutoMapSubClassA { get; set; }

            public sealed class AutoMapSubSubClassA
            {
                public string PropAOfAutoMapSubSubClassA { get; set; }

                public int PropBOfAutoMapSubSubClassA { get; set; }
            }
        }

        public sealed class AutoMapSubClassB
        {
            public string PropAOfAutoMapSubClassB { get; set; }

            public int PropBOfAutoMapSubClassB { get; set; }
        }
    }
}