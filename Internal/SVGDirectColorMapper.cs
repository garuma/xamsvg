using Android.Util;
using Android.Graphics;

namespace XamSvg.Internals
{
	public class SVGDirectColorMapper
	{
		SparseIntArray mColorMappings = new SparseIntArray();

		public SVGDirectColorMapper() {

		}

		public SVGDirectColorMapper(Color pColorFrom, Color pColorTo) {
			this.addColorMapping(pColorFrom, pColorTo);
		}

		public void addColorMapping(Color pColorFrom, Color pColorTo) {
			this.mColorMappings.Put(pColorFrom, pColorTo);
		}

		public Color mapColor(Color pColor) {
			int mappedColor = this.mColorMappings.Get(pColor);
			if(mappedColor == 0) {
				return pColor;
			} else {
				return new Color (mappedColor);
			}
		}
	}
}
