using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Android.Graphics;
using Android.Util;

using Log = Android.Util.Log;
using Attributes = System.Collections.Generic.Dictionary<string, string>;

namespace XamSvg.Internals
{
	using Utils;
	using Filters;

	public class SVGHandler
	{
		Canvas mCanvas;
		Picture mPicture;
		SVGPaint mSVGPaint;

		bool mBoundsMode;
		RectF mBounds;

		Stack<SVGGroup> mSVGGroupStack = new Stack<SVGGroup>();
		SVGPathParser mSVGPathParser = new SVGPathParser();

		SVGGradient mCurrentSVGGradient;
		SVGFilter mCurrentSVGFilter;

		bool mHidden;

		/** Multi purpose dummy rectangle. */
		RectF mRect = new RectF();

		public SVGHandler(Picture pPicture, ISvgColorMapper pSVGColorMapper) {
			this.mPicture = pPicture;
			this.mSVGPaint = new SVGPaint(pSVGColorMapper);
		}

		public void Parse (XmlReader reader)
		{
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element) {
					var attrs = Enumerable.Range (0, reader.AttributeCount).ToDictionary (i => {
						reader.MoveToAttribute (i);
						return reader.Name;
					}, i => {
						reader.MoveToAttribute (i);
						return reader.Value;
					});
					reader.MoveToElement ();
					startElement (reader.LocalName, reader.Name, attrs);
				} else if (reader.NodeType == XmlNodeType.EndElement) {
					endElement (reader.LocalName, reader.Name);
				}
			}
		}

		public RectF getBounds() {
			return this.mBounds;
		}

		public RectF getComputedBounds() {
			return this.mSVGPaint.getComputedBounds();
		}

		public void startElement(String pLocalName, String pQualifiedName, Attributes pAttributes) {
			/* Ignore everything but rectangles in bounds mode. */
			if (this.mBoundsMode) {
				this.parseBounds(pLocalName, pAttributes);
				return;
			}
			if (pLocalName.Equals(SVGConstants.TAG_SVG)) {
				this.parseSVG(pAttributes);
			} else if(pLocalName.Equals(SVGConstants.TAG_DEFS)) {
				// Ignore
			} else if(pLocalName.Equals(SVGConstants.TAG_GROUP)) {
				this.parseGroup(pAttributes);
			} else if(pLocalName.Equals(SVGConstants.TAG_LINEARGRADIENT)) {
				this.parseLinearGradient(pAttributes);
			}  else if(pLocalName.Equals(SVGConstants.TAG_RADIALGRADIENT)) {
				this.parseRadialGradient(pAttributes);
			} else if(pLocalName.Equals(SVGConstants.TAG_STOP)) {
				this.parseGradientStop(pAttributes);
			} else if(pLocalName.Equals(SVGConstants.TAG_FILTER)) {
				this.parseFilter(pAttributes);
			} else if(pLocalName.Equals(SVGConstants.TAG_FILTER_ELEMENT_FEGAUSSIANBLUR)) {
				this.parseFilterElementGaussianBlur(pAttributes);
			} else if(!this.mHidden) {
				if(pLocalName.Equals(SVGConstants.TAG_RECTANGLE)) {
					this.parseRect(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_LINE)) {
					this.parseLine(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_CIRCLE)) {
					this.parseCircle(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_ELLIPSE)) {
					this.parseEllipse(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_POLYLINE)) {
					this.parsePolyline(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_POLYGON)) {
					this.parsePolygon(pAttributes);
				} else if(pLocalName.Equals(SVGConstants.TAG_PATH)) {
					this.parsePath(pAttributes);
				} else {
					Log.Debug ("SVGParser", "Unexpected SVG tag: '" + pLocalName + "'.");
				}
			} else {
				Log.Debug ("SVGParser", "Unexpected SVG tag: '" + pLocalName + "'.");
			}
		}

		public void endElement(String pLocalName, String pQualifiedName)
		{
			if (pLocalName.Equals(SVGConstants.TAG_SVG)) {
				this.mPicture.EndRecording();
			} else if (pLocalName.Equals(SVGConstants.TAG_GROUP)) {
				this.parseGroupEnd();
			}
		}

		void parseSVG(Attributes pAttributes) {
			int width = (int)Math.Ceiling(pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_WIDTH, 0f).Value);
			int height = (int)Math.Ceiling(pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_HEIGHT, 0f).Value);
			this.mCanvas = this.mPicture.BeginRecording(width, height);
		}

		void parseBounds(string pLocalName, Attributes pAttributes) {
			if (pLocalName.Equals(SVGConstants.TAG_RECTANGLE)) {
				float x = pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_X, 0f).Value;
				float y = pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_Y, 0f).Value;
				float width = pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_WIDTH, 0f).Value;
				float height = pAttributes.GetFloatAttribute (SVGConstants.ATTRIBUTE_HEIGHT, 0f).Value;
				this.mBounds = new RectF(x, y, x + width, y + height);
			}
		}

		void parseFilter(Attributes pAttributes) {
			this.mCurrentSVGFilter = this.mSVGPaint.parseFilter(pAttributes);
		}

		void parseFilterElementGaussianBlur(Attributes pAttributes) {
			ISVGFilterElement svgFilterElement = this.mSVGPaint.parseFilterElementGaussianBlur(pAttributes);
			this.mCurrentSVGFilter.addFilterElement(svgFilterElement);
		}

		void parseLinearGradient(Attributes pAttributes) {
			this.mCurrentSVGGradient = this.mSVGPaint.parseGradient(pAttributes, true);
		}

		void parseRadialGradient(Attributes pAttributes) {
			this.mCurrentSVGGradient = this.mSVGPaint.parseGradient(pAttributes, false);
		}

		void parseGradientStop(Attributes pAttributes) {
			SVGGradientStop svgGradientStop = this.mSVGPaint.parseGradientStop(this.getSVGPropertiesFromAttributes(pAttributes));
			this.mCurrentSVGGradient.addSVGGradientStop(svgGradientStop);
		}

		void parseGroup(Attributes pAttributes) {
			/* Check to see if this is the "bounds" layer. */
			if ("bounds".Equals(pAttributes.GetStringAttribute (SVGConstants.ATTRIBUTE_ID))) {
				this.mBoundsMode = true;
			}

			SVGGroup parentSVGGroup = (this.mSVGGroupStack.Count > 0) ? this.mSVGGroupStack.Peek() : null;
			bool hasTransform = this.pushTransform(pAttributes);

			this.mSVGGroupStack.Push(new SVGGroup(parentSVGGroup, this.getSVGPropertiesFromAttributes(pAttributes, true), hasTransform));

			this.updateHidden();
		}

		void parseGroupEnd() {
			if (this.mBoundsMode) {
				this.mBoundsMode = false;
			}

			/* Pop group transform if there was one pushed. */
			if(this.mSVGGroupStack.Pop().hasTransform()) {
				this.popTransform();
			}
			this.updateHidden();
		}

		void updateHidden() {
			if(this.mSVGGroupStack.Count == 0) {
				this.mHidden = false;
			} else {
				this.mSVGGroupStack.Peek().isHidden();
			}
		}

		void parsePath(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			this.mSVGPathParser.parse(svgProperties, this.mCanvas, this.mSVGPaint);
			if(pushed) {
				this.popTransform();
			}
		}

		void parsePolygon(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGPolygonParser.parse(svgProperties, this.mCanvas, this.mSVGPaint);
			if(pushed) {
				this.popTransform();
			}
		}

		void parsePolyline(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGPolylineParser.parse(svgProperties, this.mCanvas, this.mSVGPaint);
			if(pushed) {
				this.popTransform();
			}
		}

		void parseEllipse(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGEllipseParser.parse(svgProperties, this.mCanvas, this.mSVGPaint, this.mRect);
			if(pushed) {
				this.popTransform();
			}
		}

		void parseCircle(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGCircleParser.Parse(svgProperties, this.mCanvas, this.mSVGPaint);
			if(pushed) {
				this.popTransform();
			}
		}

		void parseLine(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGLineParser.parse(svgProperties, this.mCanvas, this.mSVGPaint);
			if(pushed) {
				this.popTransform();
			}
		}

		void parseRect(Attributes pAttributes) {
			SVGProperties svgProperties = this.getSVGPropertiesFromAttributes(pAttributes);
			bool pushed = this.pushTransform(pAttributes);
			SVGRectParser.parse(svgProperties, this.mCanvas, this.mSVGPaint, this.mRect);
			if(pushed) {
				this.popTransform();
			}
		}

		SVGProperties getSVGPropertiesFromAttributes(Attributes pAttributes) {
			return this.getSVGPropertiesFromAttributes(pAttributes, false);
		}

		SVGProperties getSVGPropertiesFromAttributes(Attributes pAttributes, bool pDeepCopy) {
			if(this.mSVGGroupStack.Count > 0) {
				return new SVGProperties(this.mSVGGroupStack.Peek().getSVGProperties(), pAttributes, pDeepCopy);
			} else {
				return new SVGProperties(null, pAttributes, pDeepCopy);
			}
		}

		bool pushTransform(Attributes pAttributes) {
			string transform = pAttributes.GetStringAttribute (SVGConstants.ATTRIBUTE_TRANSFORM);
			if(transform == null) {
				return false;
			} else {
				Matrix matrix = SVGTransformParser.parseTransform(transform);
				this.mCanvas.Save();
				this.mCanvas.Concat(matrix);
				return true;
			}
		}

		void popTransform() {
			this.mCanvas.Restore();
		}

	}
}
