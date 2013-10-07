# XamSvg

## Usage

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

Every factory method supports a optional color mapper parameter that lets you dynamically change the color of the SVG being parsed. This is useful if you want to reuse the same SVG multiple time with a different tint color.

You can create a mapper by implementing the `ISvgColorMapper` interface or directly create one with a `Func<Color, Color>` using the `SvgColorMapperFactory.FromFunc` method.

### Low-level, directly using SvgParser

If you want to have directly access to the parsed SVG representation, you can use the method of `SvgParser` which returns a `Svg` instance.

Notably that instance contains a `Picture` property that contains all the drawing calls necessary to render the SVG.

## License

This work is placed under the Apache 2.0 license.
