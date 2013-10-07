using System;
using Android.Graphics;
using Android.Util;

namespace XamSvg.Internals.Utils
{
	public class SVGRectParser
	{

		public static void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint, RectF pRect) {
			float x = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_X, 0f);
			float y = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_Y, 0f);
			float width = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_WIDTH, 0f);
			float height = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_HEIGHT, 0f);

			pRect.Set(x, y, x + width, y + height);

			float? rX = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS_X);
			float? rY = pSVGProperties.getFloatAttribute(SVGConstants.ATTRIBUTE_RADIUS_Y);

			bool rXSpecified = rX != null && rX >= 0;
			bool rYSpecified = rY != null && rY >= 0;

			bool rounded = rXSpecified || rYSpecified;
			float rx;
			float ry;
			if(rXSpecified && rYSpecified) {
				rx = Math.Min(rX.Value, width * 0.5f);
				ry = Math.Min(rY.Value, height * 0.5f);
			} else if(rXSpecified) {
				ry = rx = Math.Min(rX.Value, width * 0.5f);
			} else if(rYSpecified) {
				rx = ry = Math.Min(rY.Value, height * 0.5f);
			} else {
				rx = 0;
				ry = 0;
			}

			bool fill = pSVGPaint.setFill(pSVGProperties);
			if (fill) {
				if(rounded) {
					pCanvas.DrawRoundRect(pRect, rx, ry, pSVGPaint.getPaint());
				} else {
					pCanvas.DrawRect(pRect, pSVGPaint.getPaint());
				}
			}

			bool stroke = pSVGPaint.setStroke(pSVGProperties);
			if (stroke) {
				if(rounded) {
					pCanvas.DrawRoundRect(pRect, rx, ry, pSVGPaint.getPaint());
				} else {
					pCanvas.DrawRect(pRect, pSVGPaint.getPaint());
				}
			}

			if(fill || stroke) {
				pSVGPaint.ensureComputedBoundsInclude(x, y, width, height);
			}
		}

	}
}
