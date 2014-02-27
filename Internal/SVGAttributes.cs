using System;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Utils;

	public class SVGAttributes
	{
		private Attributes mAttributes;
		private SVGAttributes mParentSVGAttributes;

		public SVGAttributes(Attributes pAttributes, bool pAttributesDeepCopy) {
			this.mAttributes = (pAttributesDeepCopy) ? new Attributes(pAttributes) : pAttributes;
			this.mAttributes = pAttributes;
		}

		public SVGAttributes(SVGAttributes pParentSVGAttributes, Attributes pAttributes, bool pAttributesDeepCopy) {
			this.mAttributes = (pAttributesDeepCopy) ? new Attributes(pAttributes) : pAttributes;
			this.mAttributes = pAttributes;
			this.mParentSVGAttributes = pParentSVGAttributes;
		}

		public void setParentSVGAttributes(SVGAttributes pParentSVGAttributes) {
			this.mParentSVGAttributes = pParentSVGAttributes;
		}

		public string getStringAttribute(String pAttributeName, bool pAllowParentSVGAttributes, String pDefaultValue) {
			string s = this.getStringAttribute(pAttributeName, pAllowParentSVGAttributes);
			if (s == null) {
				return pDefaultValue;
			} else {
				return s;
			}
		}

		public string getStringAttribute(String pAttributeName, bool pAllowParentSVGAttributes) {
			string s = mAttributes.GetStringAttribute(pAttributeName);
			if(s == null && pAllowParentSVGAttributes) {
				if(this.mParentSVGAttributes == null) {
					return null;
				} else {
					return this.mParentSVGAttributes.getStringAttribute(pAttributeName, pAllowParentSVGAttributes);
				}
			} else {
				return s;
			}
		}

		public float? getFloatAttribute(string pAttributeName, bool pAllowParentSVGAttributes) {
			return SVGParserUtils.extractFloatAttribute(this.getStringAttribute(pAttributeName, pAllowParentSVGAttributes));
		}

		public float getFloatAttribute(string pAttributeName, bool pAllowParentSVGAttributes, float pDefaultValue) {
			float? f = this.getFloatAttribute(pAttributeName, pAllowParentSVGAttributes);
			if (f == null) {
				return pDefaultValue;
			} else {
				return f.Value;
			}
		}

	}
}
