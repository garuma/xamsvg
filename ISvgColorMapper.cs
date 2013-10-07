using System;
using Android.Graphics;

namespace XamSvg
{
	public interface ISvgColorMapper
	{
		Color MapColor (Color pColor);
	}

	public static class SvgColorMapperFactory
	{
		class FuncBasedColorMapper : ISvgColorMapper
		{
			Func<Color, Color> mapper;

			public FuncBasedColorMapper (Func<Color, Color> mapper)
			{
				this.mapper = mapper;
			}

			Color ISvgColorMapper.MapColor (Color c)
			{
				return mapper (c);
			}
		}

		public static ISvgColorMapper FromFunc (Func<Color, Color> mapper)
		{
			return new FuncBasedColorMapper (mapper);
		}
	}
}
