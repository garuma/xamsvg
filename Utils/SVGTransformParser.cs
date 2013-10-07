using System;
using Android.Graphics;
using Android.Util;
using System.Text.RegularExpressions;

namespace XamSvg.Internals.Utils
{
	public class SVGTransformParser
	{
		// ===========================================================
		// Constants
		// ===========================================================

		private static Regex MULTITRANSFORM_PATTERN = new Regex ("(\\w+\\([\\d\\s\\-eE,]*\\))");

		public static Matrix parseTransform(String pString) {
			if(pString == null) {
				return null;
			}

			/* If ')' is contained only once, we have a simple/single transform.
			 * Otherwise, we have to split multi-transforms like this:
			 * "translate(-10,-20) scale(2) rotate(45) translate(5,10)". */
			bool singleTransform = pString.IndexOf(')') == pString.LastIndexOf(')');
			if(singleTransform) {
				return SVGTransformParser.parseSingleTransform(pString);
			} else {
				return SVGTransformParser.parseMultiTransform(pString);
			}
		}

		private static Matrix parseMultiTransform(String pString) {
			var matcher = MULTITRANSFORM_PATTERN.Match(pString);
			Matrix matrix = new Matrix();
			while((matcher = matcher.NextMatch ()).Success) {
				matrix.PreConcat(SVGTransformParser.parseSingleTransform(matcher.Groups[1].Value));
			}
			return matrix;
		}

		private static Matrix parseSingleTransform(String pString) {
			try {
				if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_MATRIX)) {
					return SVGTransformParser.parseTransformMatrix(pString);
				} else if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_TRANSLATE)) {
					return SVGTransformParser.parseTransformTranslate(pString);
				} else if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SCALE)) {
					return SVGTransformParser.parseTransformScale(pString);
				} else if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SKEW_X)) {
					return SVGTransformParser.parseTransformSkewX(pString);
				} else if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SKEW_Y)) {
					return SVGTransformParser.parseTransformSkewY(pString);
				} else if (pString.StartsWith(SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_ROTATE)) {
					return SVGTransformParser.parseTransformRotate(pString);
				} else {
					throw new SVGParseException("Unexpected transform type: '" + pString + "'.");
				}
			} catch (SVGParseException e) {
				throw new SVGParseException("Could not parse transform: '" + pString + "'.", e);
			}
		}

		public static Matrix parseTransformRotate(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_ROTATE.Length + 1;
			var svgNumberParserFloatResult = pString.Substring(start, pString.IndexOf(')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCountMinimum(svgNumberParserFloatResult, 1);

			float angle = svgNumberParserFloatResult[0];
			float cx = 0;
			float cy = 0;
			if (svgNumberParserFloatResult.Length > 2) {
				cx = svgNumberParserFloatResult[1];
				cy = svgNumberParserFloatResult[2];
			}
			Matrix matrix = new Matrix();
			matrix.PostTranslate(cx, cy);
			matrix.PostRotate(angle);
			matrix.PostTranslate(-cx, -cy);
			return matrix;
		}

		private static Matrix parseTransformSkewY(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SKEW_Y.Length + 1;
			var svgNumberParserFloatResult = pString.Substring(start, pString.IndexOf(')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCountMinimum(svgNumberParserFloatResult, 1);

			float angle = svgNumberParserFloatResult[0];
			Matrix matrix = new Matrix();
			matrix.PostSkew(0, (float) Math.Tan(angle));
			return matrix;
		}

		private static Matrix parseTransformSkewX(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SKEW_X.Length + 1;
			var svgNumberParserFloatResult = pString.Substring(start, pString.IndexOf(')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCountMinimum(svgNumberParserFloatResult, 1);

			float angle = svgNumberParserFloatResult[0];
			Matrix matrix = new Matrix();
			matrix.PostSkew((float) Math.Tan(angle), 0);
			return matrix;
		}

		private static Matrix parseTransformScale(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_SCALE.Length + 1;
			var svgNumberParserFloatResult = pString.Substring (start, pString.IndexOf (')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCountMinimum(svgNumberParserFloatResult, 1);
			float sx = svgNumberParserFloatResult[0];
			float sy = 0;
			if (svgNumberParserFloatResult.Length > 1) {
				sy = svgNumberParserFloatResult[1];
			}
			Matrix matrix = new Matrix();
			matrix.PostScale(sx, sy);
			return matrix;
		}

		private static Matrix parseTransformTranslate(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_TRANSLATE.Length + 1;
			var svgNumberParserFloatResult = pString.Substring(start, pString.IndexOf(')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCountMinimum(svgNumberParserFloatResult, 1);
			float tx = svgNumberParserFloatResult[0];
			float ty = 0;
			if (svgNumberParserFloatResult.Length > 1) {
				ty = svgNumberParserFloatResult[1];
			}
			Matrix matrix = new Matrix();
			matrix.PostTranslate(tx, ty);
			return matrix;
		}

		private static Matrix parseTransformMatrix(String pString) {
			var start = SVGConstants.ATTRIBUTE_TRANSFORM_VALUE_MATRIX.Length + 1;
			var svgNumberParserFloatResult = pString.Substring(start, pString.IndexOf(')') - start).ParseFloats ();
			SVGTransformParser.assertNumberParserResultNumberCount(svgNumberParserFloatResult, 6);
			Matrix matrix = new Matrix();
			matrix.SetValues(new float[]{
					// Row 1
					svgNumberParserFloatResult[0],
					svgNumberParserFloatResult[2],
					svgNumberParserFloatResult[4],
					// Row 2
					svgNumberParserFloatResult[1],
					svgNumberParserFloatResult[3],
					svgNumberParserFloatResult[5],
					// Row 3
					0,
					0,
					1,
			});
			return matrix;
		}

		private static void assertNumberParserResultNumberCountMinimum(float[] pSVGNumberParserFloatResult, int pNumberParserResultNumberCountMinimum) {
			int svgNumberParserFloatResultNumberCount = pSVGNumberParserFloatResult.Length;
			if(svgNumberParserFloatResultNumberCount < pNumberParserResultNumberCountMinimum) {
				throw new SVGParseException("Not enough data. Minimum Expected: '" + pNumberParserResultNumberCountMinimum + "'. Actual: '" + svgNumberParserFloatResultNumberCount + "'.");
			}
		}

		private static void assertNumberParserResultNumberCount(float[] pSVGNumberParserFloatResult, int pNumberParserResultNumberCount) {
			int svgNumberParserFloatResultNumberCount = pSVGNumberParserFloatResult.Length;
			if(svgNumberParserFloatResultNumberCount != pNumberParserResultNumberCount) {
				throw new SVGParseException("Unexpected number count. Expected: '" + pNumberParserResultNumberCount + "'. Actual: '" + svgNumberParserFloatResultNumberCount + "'.");
			}
		}
	}
}
