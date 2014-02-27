using System;
using System.Collections.Generic;

using Android.Graphics;

using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Utils;
	using Filters;

	public class SVGPaint {

		Paint mPaint = new Paint();

		ISvgColorMapper mSVGColorMapper;

		/** Multi purpose dummy rectangle. */
		RectF mRect = new RectF();
		RectF mComputedBounds = new RectF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

		Dictionary<string, SVGGradient> mSVGGradientMap = new Dictionary<string, SVGGradient>();
		Dictionary<string, SVGFilter> mSVGFilterMap = new Dictionary<string, SVGFilter>();

		public SVGPaint(ISvgColorMapper pSVGColorMapper) {
			this.mSVGColorMapper = pSVGColorMapper;
		}
		
		public Paint getPaint() {
			return this.mPaint;
		}

		public RectF getComputedBounds() {
			return this.mComputedBounds;
		}

		public void resetPaint(Paint.Style pStyle) {
			this.mPaint.Reset();
			this.mPaint.AntiAlias = true; // TODO AntiAliasing could be made optional through some SVGOptions object.
			this.mPaint.SetStyle(pStyle);
		}

		/**
		 *  TODO Would it be better/cleaner to throw a SVGParseException when sth could not be parsed instead of simply returning false?
		 */
		public bool setFill(SVGProperties pSVGProperties) {
			if(this.isDisplayNone(pSVGProperties) || this.isFillNone(pSVGProperties)) {
				return false;
			}

			this.resetPaint(Paint.Style.Fill);

			string fillProperty = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_FILL);
			if(fillProperty == null) {
				if(pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STROKE) == null) {
					/* Default is black fill. */
					this.mPaint.Color = Color.Black; // TODO Respect color mapping?
					return true;
				} else {
					return false;
				}
			} else {
				return this.applyPaintProperties(pSVGProperties, true);
			}
		}

		public bool setStroke(SVGProperties pSVGProperties) {
			if(this.isDisplayNone(pSVGProperties) || this.isStrokeNone(pSVGProperties)) {
				return false;
			}

			this.resetPaint(Paint.Style.Stroke);

			return this.applyPaintProperties(pSVGProperties, false);
		}

		bool isDisplayNone(SVGProperties pSVGProperties) {
			return SVGConstants.VALUE_NONE.Equals(pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_DISPLAY));
		}

		bool isFillNone(SVGProperties pSVGProperties) {
			return SVGConstants.VALUE_NONE.Equals(pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_FILL));
		}

		bool isStrokeNone(SVGProperties pSVGProperties) {
			return SVGConstants.VALUE_NONE.Equals(pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STROKE));
		}

		public bool applyPaintProperties(SVGProperties pSVGProperties, bool pModeFill) {
			if(this.setColorProperties(pSVGProperties, pModeFill)) {
				if(pModeFill) {
					return this.applyFillProperties(pSVGProperties);
				} else {
					return this.applyStrokeProperties(pSVGProperties);
				}
			} else {
				return false;
			}
		}

		bool setColorProperties(SVGProperties pSVGProperties, bool pModeFill) { // TODO throw SVGParseException
			string colorProperty = pSVGProperties.getStringProperty(pModeFill ? SVGConstants.ATTRIBUTE_FILL : SVGConstants.ATTRIBUTE_STROKE);
			if(colorProperty == null) {
				return false;
			}

			string filterProperty = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_FILTER);
			if(filterProperty != null) {
				if(SVGProperties.IsUrlProperty(filterProperty)) {
					string filterID = SVGParserUtils.extractIDFromURLProperty(filterProperty);

					this.getFilter(filterID).applyFilterElements(this.mPaint);
				} else {
					return false;
				}
			}

			if(SVGProperties.IsUrlProperty(colorProperty)) {
				string gradientID = SVGParserUtils.extractIDFromURLProperty(colorProperty);

				this.mPaint.SetShader(this.getGradientShader(gradientID));
				return true;
			} else {
				int? color = this.parseColor(colorProperty);
				if(color != null) {
					this.applyColor(pSVGProperties, color.Value, pModeFill);
					return true;
				} else {
					return false;
				}
			}
		}

		bool applyFillProperties(SVGProperties pSVGProperties) {
			return true;
		}

		bool applyStrokeProperties(SVGProperties pSVGProperties) {
			float? width = pSVGProperties.getFloatProperty(SVGConstants.ATTRIBUTE_STROKE_WIDTH);
			if (width != null) {
				this.mPaint.StrokeWidth = width.Value;
			}
			string linecap = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STROKE_LINECAP);
			if (SVGConstants.ATTRIBUTE_STROKE_LINECAP_VALUE_ROUND.Equals(linecap)) {
				this.mPaint.StrokeCap = Paint.Cap.Round;
			} else if (SVGConstants.ATTRIBUTE_STROKE_LINECAP_VALUE_SQUARE.Equals(linecap)) {
				this.mPaint.StrokeCap = Paint.Cap.Square;
			} else if (SVGConstants.ATTRIBUTE_STROKE_LINECAP_VALUE_BUTT.Equals(linecap)) {
				this.mPaint.StrokeCap = Paint.Cap.Butt;
			}
			string linejoin = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STROKE_LINEJOIN_VALUE_);
			if (SVGConstants.ATTRIBUTE_STROKE_LINEJOIN_VALUE_MITER.Equals(linejoin)) {
				this.mPaint.StrokeJoin = Paint.Join.Miter;
			} else if (SVGConstants.ATTRIBUTE_STROKE_LINEJOIN_VALUE_ROUND.Equals(linejoin)) {
				this.mPaint.StrokeJoin = Paint.Join.Round;
			} else if (SVGConstants.ATTRIBUTE_STROKE_LINEJOIN_VALUE_BEVEL.Equals(linejoin)) {
				this.mPaint.StrokeJoin = Paint.Join.Bevel;
			}
			return true;
		}

		void applyColor(SVGProperties pSVGProperties, int pColor, bool pModeFill) {
			int c = (int)((ColorUtils.COLOR_MASK_32BIT_ARGB_RGB & pColor) | ColorUtils.COLOR_MASK_32BIT_ARGB_ALPHA);
			this.mPaint.Color = new Color (c);
			this.mPaint.Alpha = SVGPaint.parseAlpha(pSVGProperties, pModeFill);
		}

		static int parseAlpha(SVGProperties pSVGProperties, bool pModeFill) {
			float? opacity = pSVGProperties.getFloatProperty(SVGConstants.ATTRIBUTE_OPACITY);
			if(opacity == null) {
				opacity = pSVGProperties.getFloatProperty(pModeFill ? SVGConstants.ATTRIBUTE_FILL_OPACITY : SVGConstants.ATTRIBUTE_STROKE_OPACITY);
			}
			if(opacity == null) {
				return 255;
			} else {
				return (int) (255 * opacity);
			}
		}

		public void ensureComputedBoundsInclude(float pX, float pY) {
			if (pX < this.mComputedBounds.Left) {
				this.mComputedBounds.Left = pX;
			}
			if (pX > this.mComputedBounds.Right) {
				this.mComputedBounds.Right = pX;
			}
			if (pY < this.mComputedBounds.Top) {
				this.mComputedBounds.Top = pY;
			}
			if (pY > this.mComputedBounds.Bottom) {
				this.mComputedBounds.Bottom = pY;
			}
		}

		public void ensureComputedBoundsInclude(float pX, float pY, float pWidth, float pHeight) {
			this.ensureComputedBoundsInclude(pX, pY);
			this.ensureComputedBoundsInclude(pX + pWidth, pY + pHeight);
		}

		public void ensureComputedBoundsInclude(Path pPath) {
			pPath.ComputeBounds(this.mRect, false);
			this.ensureComputedBoundsInclude(this.mRect.Left, this.mRect.Top);
			this.ensureComputedBoundsInclude(this.mRect.Right, this.mRect.Bottom);
		}

		Color parseColor(string pString, Color pDefault) {
			var color = this.parseColor(pString);
			if(color == null) {
				return this.applySVGColorMapper(pDefault);
			} else {
				return color.Value;
			}
		}

		Color? parseColor(string pString) {
			/* TODO Test if explicit pattern matching is faster:
			 * 
			 * RGB:		/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/
			 * #RRGGBB:	/^(\w{2})(\w{2})(\w{2})$/
			 * #RGB:	/^(\w{1})(\w{1})(\w{1})$/
			 */

			Color? parsedColor;
			if(pString == null) {
				parsedColor = null;
			} else if(SVGProperties.isHexProperty(pString)) {
				parsedColor = SVGParserUtils.extractColorFromHexProperty(pString);
			} else if(SVGProperties.isRgbProperty(pString)) {
				parsedColor = SVGParserUtils.extractColorFromRGBProperty(pString);
			} else {
				Color? colorByName = ColorUtils.GetColorByName(pString.Trim());
				if(colorByName != null) {
					parsedColor = colorByName;
				} else {
					parsedColor = SVGParserUtils.extraColorIntegerProperty(pString);
				}
			}
			return this.applySVGColorMapper(parsedColor.Value);
		}

		Color applySVGColorMapper(Color pColor) {
			if(this.mSVGColorMapper == null) {
				return pColor;
			} else {
				return this.mSVGColorMapper.MapColor(pColor);
			}
		}
		
		public SVGFilter parseFilter (Attributes pAttributes) {
			string id = pAttributes.GetStringAttribute(SVGConstants.ATTRIBUTE_ID);
			if(id == null) {
				return null;
			}

			SVGFilter svgFilter = new SVGFilter(id, pAttributes);
			this.mSVGFilterMap[id] = svgFilter;
			return svgFilter;
		}

		public SVGGradient parseGradient(Attributes pAttributes, bool pLinear) {
			string id = pAttributes.GetStringAttribute(SVGConstants.ATTRIBUTE_ID);
			if(id == null) {
				return null;
			}

			SVGGradient svgGradient = new SVGGradient(id, pLinear, pAttributes);
			this.mSVGGradientMap[id] = svgGradient;
			return svgGradient;
		}

		public SVGGradientStop parseGradientStop(SVGProperties pSVGProperties) {
			float offset = pSVGProperties.getFloatProperty(SVGConstants.ATTRIBUTE_OFFSET, 0f);
			string stopColor = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STOP_COLOR);
			int rgb = this.parseColor(stopColor.Trim(), Color.Black);
			int alpha = this.parseGradientStopAlpha(pSVGProperties);
			return new SVGGradientStop(offset, alpha | rgb);
		}

		int parseGradientStopAlpha(SVGProperties pSVGProperties) {
			string opacityStyle = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_STOP_OPACITY);
			if(opacityStyle != null) {
				float alpha = float.Parse (opacityStyle);
				int alphaInt = (int)Math.Round(255 * alpha);
				return (alphaInt << 24);
			} else {
				unchecked {
					return (int)ColorUtils.COLOR_MASK_32BIT_ARGB_ALPHA;
				}
			}
		}

		Shader getGradientShader(string pGradientShaderID) {
			SVGGradient svgGradient = this.mSVGGradientMap[pGradientShaderID];
			if(svgGradient == null) {
				throw new SVGParseException("No SVGGradient found for id: '" + pGradientShaderID + "'.");
			} else {
				Shader gradientShader = svgGradient.getShader();
				if(gradientShader != null) {
					return gradientShader;
				} else {
					svgGradient.ensureHrefResolved(this.mSVGGradientMap);
					return svgGradient.createShader();
				}
			}
		}

		SVGFilter getFilter(string pSVGFilterID) {
			SVGFilter svgFilter = this.mSVGFilterMap[pSVGFilterID];
			if(svgFilter == null) {
				return null; // TODO Better a SVGParseException here?
			} else {
				svgFilter.ensureHrefResolved(this.mSVGFilterMap);
				return svgFilter;
			}
		}

		public SVGFilterElementGaussianBlur parseFilterElementGaussianBlur(Attributes pAttributes) {
			float standardDeviation = pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_FILTER_ELEMENT_FEGAUSSIANBLUR_STANDARDDEVIATION).Value;
			return new SVGFilterElementGaussianBlur(standardDeviation);
		}

	}
}
