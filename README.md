# AutoMapper.Extensions
An extension library for AutoMapper that brings back automatic mapping creation, ignoring missing constructor parameters (default values for types are passed), strongly-typed mappings for constructor parameters to lessen code changes and possible issues

## Project Description
AutoMapper is a brilliant and powerfull tool. But even though, it has its own isseus and missing functionalities. If you want to follow zero friction theorem, AutoMapper can bring you closer to it. With creation of automated mapping, ignoring missing constructor parameters and strongly-typed constructor parameters (instead of using string value) you can create a better, more robust and requiring less maintenance code.

## Overview
This library was intended to restore automatic creation of AutoMapper mappings. If you have nested types, just use extension method `.CreateAutoMap<TSource, TDestination>()` or `.CreateAutoMap(typeof(TSource), typeof(TDestination))` and all types used in those classes will be automatically mapped. 

If you want to pass default type values for destination type constructor, use `.CtorPassDefaultsForMissingParameters()` or `.CtorPassDefaultsForMissingParameters(typeof(TSource), typeof(TDestination))`.

In case you want to pass a fixed value to a destination type parameter, then use `.CtorMapParameter(x => x.Parameter, TValueInstance)`. The name of the parameter is taken from an appropriate property that is defined within the type. The name must match, however, this match is case-insensitive.  

You can as well do a normal mapping for constructor parameters in AutoMapper style, but without need to specify name of the parameter manually. To achieve that use the following method `.CtorMapParameter(x => x.Parameter, cfg => cfg.MapFrom((...)))`. Name of the constructor parameter is calculated and matched automatically using case-insensitive comparison.

Example:

```c#

public sealed class ConverterFromSourceToDestination
{
	private readonly IMapper _mapper = new MapperConfiguration(cfg => 
	{
		cfg
			.CreateAutoMap<TSource, TDestination>() //this will create a map for those two mentioned types and all types they use for properties
			.CtorPassDefaultsForMissingParameters() //this will pass default parameters for TDestination constructor 
			.CtorMapParameter(x => x.Parameter, fixedValue) //this will pass an object with name fixedValue as a parameter for constructor parameter with a name that matches `Parameter`
			.CtorMapParameter(x => x.OtherParameter, paramCfg => paramCfg.MapFrom((...))); //this will allow you to create custom configuration for constructor parameter but using strongly-typed way and not magic strings
	});
}
```

## Versioning

AutoMapper.Extensions uses [Semantic Versioning 2.0.0](http://semver.org/spec/v2.0.0.html) for the public releases (published to the [nuget.org](https://www.nuget.org/)).

## Additional resources

* [Rafal Kozlowski's blog](https://rafalkozlowski.engineer)
