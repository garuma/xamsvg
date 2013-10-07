using Android.Graphics;

namespace XamSvg.Internals.Filters
{
	public class SVGFilterElementGaussianBlur : ISVGFilterElement
	{
		BlurMaskFilter mBlurMaskFilter;

		public SVGFilterElementGaussianBlur (float pStandardDeviation)
		{
			float radius = pStandardDeviation * 2;
			this.mBlurMaskFilter = new BlurMaskFilter(radius, BlurMaskFilter.Blur.Normal);
		}

		public void Apply(Paint pPaint)
		{
			pPaint.SetMaskFilter (mBlurMaskFilter);
		}
	}
}