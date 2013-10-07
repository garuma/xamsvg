using System;

namespace XamSvg.Internals
{
	using Utils;

	public class SVGGroup
	{
		SVGGroup mSVGroupParent;
		SVGProperties mSVGProperties;
		bool mHasTransform;
		bool mHidden;

		public SVGGroup(SVGGroup pSVGroupParent, SVGProperties pSVGProperties, bool pHasTransform) {
			this.mSVGroupParent = pSVGroupParent;
			this.mSVGProperties = pSVGProperties;
			this.mHasTransform = pHasTransform;
			this.mHidden = (this.mSVGroupParent != null && this.mSVGroupParent.isHidden()) || this.isDisplayNone();
		}

		public bool hasTransform() {
			return this.mHasTransform;
		}

		public SVGProperties getSVGProperties() {
			return this.mSVGProperties;
		}

		public bool isHidden() {
			return this.mHidden;
		}

		private bool isDisplayNone() {
			return SVGConstants.VALUE_NONE.Equals(this.mSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_DISPLAY, false));
		}

	}
}