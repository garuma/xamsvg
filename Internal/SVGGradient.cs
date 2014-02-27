using System;
using System.Collections.Generic;
using Android.Graphics;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Utils;

	public class SVGGradient
	{
		string mID;
		string mHref;
		private SVGGradient mParent;

		private Shader mShader;

		SVGAttributes mSVGAttributes;
		bool mLinear;
		private Matrix mMatrix;

		private List<SVGGradientStop> mSVGGradientStops;
		private float[] mSVGGradientStopsPositions;
		private int[] mSVGGradientStopsColors;
		private bool mSVGGradientStopsBuilt;

		public SVGGradient(string pID, bool pLinear, Attributes pAttributes) {
			this.mID = pID;
			this.mHref = SVGParserUtils.parseHref(pAttributes);
			this.mLinear = pLinear;
			this.mSVGAttributes = new SVGAttributes(pAttributes, true);
		}

		public bool hasHref() {
			return this.mHref != null;
		}

		public string getHref() {
			return this.mHref;
		}

		public string getID() {
			return this.mID;
		}

		public bool hasHrefResolved() {
			return this.mHref == null || this.mParent != null;
		}

		public Shader getShader() {
			return this.mShader;
		}

		public Shader createShader() {
			if(this.mShader != null) {
				return this.mShader;
			}

			if(!this.mSVGGradientStopsBuilt) {
				this.buildSVGGradientStopsArrays();
			}

			Shader.TileMode tileMode = this.getTileMode();
			if(this.mLinear) {
				float x1 =this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_X1, true, 0f);
				float x2 = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_X2, true, 0f);
				float y1 = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_Y1, true, 0f);
				float y2 = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_Y2, true, 0f);

				this.mShader = new LinearGradient(x1, y1, x2, y2, this.mSVGGradientStopsColors, this.mSVGGradientStopsPositions, tileMode);
			} else {
				float centerX = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_X, true, 0f);
				float centerY = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_CENTER_Y, true, 0f);
				float radius = this.mSVGAttributes.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS, true, 0f);

				this.mShader = new RadialGradient(centerX, centerY, radius, this.mSVGGradientStopsColors, this.mSVGGradientStopsPositions, tileMode);
			}
			this.mMatrix = this.getTransform();
			if (this.mMatrix != null) {
				this.mShader.SetLocalMatrix(this.mMatrix);
			}

			return this.mShader;
		}

		private Shader.TileMode getTileMode() {
			string spreadMethod = this.mSVGAttributes.getStringAttribute(SVGConstants.ATTRIBUTE_SPREADMETHOD, true);
			if(spreadMethod == null || SVGConstants.ATTRIBUTE_SPREADMETHOD_VALUE_PAD.Equals(spreadMethod)) {
				return Shader.TileMode.Clamp;
			} else if(SVGConstants.ATTRIBUTE_SPREADMETHOD_VALUE_REFLECT.Equals(spreadMethod)) {
				return Shader.TileMode.Mirror;
			} else if(SVGConstants.ATTRIBUTE_SPREADMETHOD_VALUE_REPEAT.Equals(spreadMethod)) {
				return Shader.TileMode.Repeat;
			} else {
				throw new SVGParseException("Unexpected spreadmethod: '" + spreadMethod + "'.");
			}
		}

		private Matrix getTransform() {
			if(this.mMatrix != null) {
				return this.mMatrix;
			} else {
				string transfromString = this.mSVGAttributes.getStringAttribute(SVGConstants.ATTRIBUTE_GRADIENT_TRANSFORM, false);
				if(transfromString != null) {
					this.mMatrix = SVGTransformParser.parseTransform(transfromString);
					return this.mMatrix;
				} else {
					if(this.mParent != null) {
						return this.mParent.getTransform();
					} else {
						return null;
					}
				}
			}
		}

		public void ensureHrefResolved(Dictionary<String, SVGGradient> pSVGGradientMap) {
			if(!this.hasHrefResolved()) {
				this.resolveHref(pSVGGradientMap);
			}
		}

		private void resolveHref(Dictionary<String, SVGGradient> pSVGGradientMap) {
			SVGGradient parent = pSVGGradientMap[this.mHref];
			if(parent == null) {
				throw new SVGParseException("Could not resolve href: '" + this.mHref + "' of SVGGradient: '" + this.mID + "'.");
			} else {
				parent.ensureHrefResolved(pSVGGradientMap);
				this.mParent = parent;
				this.mSVGAttributes.setParentSVGAttributes(this.mParent.mSVGAttributes);
				if(this.mSVGGradientStops == null) {
					this.mSVGGradientStops = this.mParent.mSVGGradientStops;
					this.mSVGGradientStopsColors = this.mParent.mSVGGradientStopsColors;
					this.mSVGGradientStopsPositions = this.mParent.mSVGGradientStopsPositions;
				}
			}
		}

		private void buildSVGGradientStopsArrays() {
			this.mSVGGradientStopsBuilt = true;
			List<SVGGradientStop> svgGradientStops = this.mSVGGradientStops;

			int svgGradientStopCount = svgGradientStops.Count;
			this.mSVGGradientStopsColors = new int[svgGradientStopCount];
			this.mSVGGradientStopsPositions = new float[svgGradientStopCount];

			for (int i = 0; i < svgGradientStopCount; i++) {
				SVGGradientStop svgGradientStop = svgGradientStops[i];
				this.mSVGGradientStopsColors[i] = svgGradientStop.mColor;
				this.mSVGGradientStopsPositions[i] = svgGradientStop.mOffset;
			}
		}

		public void addSVGGradientStop(SVGGradientStop pSVGGradientStop) {
			if(this.mSVGGradientStops == null) {
				this.mSVGGradientStops = new List<SVGGradientStop>();
			}
			this.mSVGGradientStops.Add(pSVGGradientStop);
		}
	}

	public struct SVGGradientStop {

		public float mOffset;
		public int mColor;

		public SVGGradientStop(float pOffset, int pColor) {
			this.mOffset = pOffset;
			this.mColor = pColor;
		}
	}
}