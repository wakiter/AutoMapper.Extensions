using System;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace AutoMapper.Extensions.Tests;

public class CtorPassDefaultsForMissingParametersExtensionsTests_Part2
{
    public sealed class UpsertJobOffer
    {
        public string PositionName { get; set; }

        public CompanyDetails CompanyDetailsProp { get; set; }

        public sealed class CompanyDetails
        {
            public string Name { get; set; }

            public Uri Uri { get; set; }
        }
    }

    public sealed class JobOffer
    {
        public JobOffer(
            string positionName,
            CompanyDetails companyDetailsProp)
            : this(0,
                positionName,
                companyDetailsProp)
        {
        }

        public JobOffer(
            long id,
            string positionName,
            CompanyDetails companyDetailsProp)
        {
            Id = id;
            PositionName = positionName;
            CompanyDetailsProp = companyDetailsProp;
        }

        public long Id { get; private set; }

        public string PositionName { get; }

        public CompanyDetails CompanyDetailsProp { get; }

        public sealed class CompanyDetails
        {
            public CompanyDetails(string name, Uri uri, Uri originalImgUrl, Uri imgUrl)
            {
                Name = name;
                Uri = uri;
                OriginalImgUrl = originalImgUrl;
                ImgUrl = imgUrl;
            }

            public string Name { get; }

            public Uri Uri { get; }

            public Uri OriginalImgUrl { get; }

            public Uri ImgUrl { get; private set; } = new Uri("about:blank");
        }
    }

    public class Source
    {
    }

    public class Destination
    {
    }

    [Fact]
    public void Mapper_passes_default_values_for_missing_arguments_for_autoMap_generic_with_complex_property_with_missing_ctor_parameters()
    {
        var fixture = new Fixture();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();

            cfg.CreateAutoMap<UpsertJobOffer, JobOffer>()
                .CtorPassDefaultsForMissingParameters();

        }).CreateMapper();

        var input = fixture.Create<UpsertJobOffer>();

        var actual = sut.Map<JobOffer>(input);

        actual
            .Should()
            .BeEquivalentTo(new
            {
                Id = 0,
                PositionName = input.PositionName,
                CompanyDetailsProp = new { Name = input.CompanyDetailsProp.Name, Uri = input.CompanyDetailsProp.Uri }
            });
    }

    [Fact]
    public void Mapper_passes_default_values_for_missing_arguments_for_normal_map()
    {
        var fixture = new Fixture();

        var sut = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UpsertJobOffer.CompanyDetails, JobOffer.CompanyDetails>()
                .CtorPassDefaultsForMissingParameters();
        }).CreateMapper();

        var input = fixture.Create<UpsertJobOffer.CompanyDetails>();

        var actual = sut.Map<JobOffer.CompanyDetails>(input);

        actual
            .Should()
            .BeEquivalentTo(new
            {
                Name = input.Name,
                Uri = input.Uri,
                OriginalImgUrl = default(Uri),
                ImgUrl = default(Uri)
            });
    }
}