using System;
using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Utils;

	public class SVGProperties
	{
		SVGStyleSet mSVGStyleSet;
		Attributes mAttributes;
		SVGProperties mParentSVGProperties;

		public SVGProperties(SVGProperties pParentSVGProperties, Attributes pAttributes, bool pAttributesDeepCopy) {
			this.mAttributes = (pAttributesDeepCopy) ? new Attributes(pAttributes) : pAttributes;
			this.mAttributes = pAttributes;
			this.mParentSVGProperties = pParentSVGProperties;
			var styleAttr = pAttributes.GetStringAttribute (SVGConstants.ATTRIBUTE_STYLE);
			if (styleAttr != null)
				mSVGStyleSet = new SVGStyleSet (styleAttr);
		}

		public string getStringProperty(string pPropertyName, string pDefaultValue)
		{
			return getStringProperty(pPropertyName) ?? pDefaultValue;
		}

		public string getStringProperty(string pPropertyName)
		{
			return getStringProperty(pPropertyName, true);
		}

		public string getStringProperty(string pPropertyName, bool pAllowParentSVGProperties)
		{
			string s = null;
			if (this.mSVGStyleSet != null)
				s = mSVGStyleSet.GetStyle (pPropertyName);
			if (s == null)
				s = mAttributes.GetStringAttribute (pPropertyName);
			if (s == null && pAllowParentSVGProperties)
				return mParentSVGProperties == null ? null : mParentSVGProperties.getStringProperty(pPropertyName);
			else
				return s;
		}

		public float? getFloatProperty(string pPropertyName)
		{
			return SVGParserUtils.extractFloatAttribute(this.getStringProperty(pPropertyName));
		}

		public float getFloatProperty(string pPropertyName, float pDefaultValue) {
			float? f = this.getFloatProperty(pPropertyName);
			if (f == null) {
				return pDefaultValue;
			} else {
				return f.Value;
			}
		}

		public string GetStringAttribute(string pAttributeName)
		{
			return mAttributes.GetStringAttribute (pAttributeName);
		}

		public string GetStringAttribute(string pAttributeName, string pDefaultValue)
		{
			return mAttributes.GetStringAttribute (pAttributeName, pDefaultValue);
		}

		public float? getFloatAttribute(string pAttributeName)
		{
			return mAttributes.GetFloatAttribute (pAttributeName);
		}

		public float getFloatAttribute(string pAttributeName, float pDefaultValue)
		{
			return mAttributes.GetFloatAttribute (pAttributeName, pDefaultValue).Value;
		}

		public static bool IsUrlProperty (string pProperty)
		{
			return pProperty.StartsWith("url(#");
		}

		public static bool isRgbProperty (string pProperty)
		{
			return pProperty.StartsWith("rgb(");
		}

		public static bool isHexProperty (string pProperty)
		{
			return pProperty.StartsWith("#");
		}
	}
}