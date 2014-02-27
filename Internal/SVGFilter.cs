using System;
using System.Collections.Generic;
using Android.Graphics;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Filters;
	using Utils;

	public class SVGFilter
	{
		string mID;
		string mHref;
		private SVGFilter mParent;

		SVGAttributes mSVGAttributes;

		List<ISVGFilterElement> mSVGFilterElements = new List<ISVGFilterElement>();

		public SVGFilter(string pID, Attributes pAttributes) {
			this.mID = pID;
			this.mHref = SVGParserUtils.parseHref(pAttributes);
			this.mSVGAttributes = new SVGAttributes(pAttributes, true);
		}

		public string getID() {
			return this.mID;
		}

		public string getHref() {
			return this.mHref;
		}

		public bool hasHref() {
			return this.mHref != null;
		}

		public bool hasHrefResolved() {
			return this.mHref == null || this.mParent != null;
		}

		public void ensureHrefResolved(Dictionary<string, SVGFilter> pSVGFilterMap) {
			if(!this.hasHrefResolved()) {
				this.resolveHref(pSVGFilterMap);
			}
		}

		private void resolveHref(Dictionary<string, SVGFilter> pSVGFilterMap) {
			SVGFilter parent = pSVGFilterMap[this.mHref];
			if(parent == null) {
				throw new SVGParseException("Could not resolve href: '" + this.mHref + "' of SVGGradient: '" + this.mID + "'.");
			} else {
				parent.ensureHrefResolved(pSVGFilterMap);
				this.mParent = parent;
				this.mSVGAttributes.setParentSVGAttributes(this.mParent.mSVGAttributes);
			}
		}

		public void applyFilterElements(Paint pPaint) {
			this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_X, true);
			List<ISVGFilterElement> svgFilterElements = this.mSVGFilterElements;
			for(int i = 0; i < svgFilterElements.Count; i++) {
				svgFilterElements[i].Apply(pPaint);
			}
		}

		public void addFilterElement(ISVGFilterElement pSVGFilterElement) {
			this.mSVGFilterElements.Add(pSVGFilterElement);
		}
	}
}
