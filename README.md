# XamSvg

XamSvg is a library to parse and render SVG images with Xamarin.Android

It's mainly a rough port of [AndEngine](http://www.andengine.org/) SVG plugin with added sugar for normal Android development.

## Usage

### Preparing your SVG

First and foremost, this library is not meant to be a full blown SVG parser. Rather the idea is to support a small subset of the format to allow scalable shapes to be used on Android. This means that path, shapes, color and gradients are supported but basically nothing else (e.g. pretty much no SVG effects).

It's also possible that some higher-level SVG constructs used by graphic softwares don't meddle well with the parser. In that case, you should try to export the image in a "lower-compatibility" or "simplified" mode. The excellent (and free) [Inkscape](http://inkscape.org/) SVG editor for instance supports saving pictures in a "Optimized SVG" format that is more suitable to be consummed by the library.

### Basic

The most straightforward way to use the library is through `SvgFactory`.

``` csharp
// Get a bitmap from resources (also raw SVG string, TextReader and Stream)
var bmp = SvgFactory.GetBitmap (Activity.Resources, Resource.Raw.ic_svg, 48, 48);

// Get a drawable that will automatically scale with the drawer bounds
var drawable = SvgFactory.GetDrawable (Activity.Resources, Resource.Raw.ic_svg);
```

If you are using the `Drawable` factory method, know that you should specify some bounds first. If you are using an `ImageView`, this can be simply done by specifying it like this in XML:

``` xml
<ImageView
	android:layout_width="14dp"
	android:layout_height="9dp"
	android:scaleType="fitXY" />
```

Note that we also use the *fitXY* value for scale type to avoid any canvas transformation.

In that case, the drawable will automatically generate a bitmap representation of your SVG at the right size and density.

### Color mapper

Every factory method supports an optional color mapper parameter that let you dynamically change the color of the SVG being parsed. This is useful if you want to reuse the same SVG multiple time with a different tint color.

You can create a mapper by implementing the `ISvgColorMapper` interface or directly create one with a `Func<Color, Color>` using the `SvgColorMapperFactory.FromFunc` method.

### Low-level, directly using SvgParser

If you want to have directly access to the parsed SVG representation, you can use the methods of the `SvgParser` class which return a `Svg` instance.

That instance has a `Picture` property returning (surprise) a `Picture` object that contains all the drawing calls necessary to render the SVG.

### Bigger example

[Moyeu](https://github.com/garuma/Moyeu) uses this library for pretty much all of its icon. Since it's also available under an open-source license, it's a good way to see how the library can be used.

## License

This work is placed under the [Apache 2.0 license](http://www.apache.org/licenses/LICENSE-2.0.html).
