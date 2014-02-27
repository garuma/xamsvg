using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Util;

namespace XamSvg.Internals.Utils
{
	public class SVGPathParser
	{
		String mString;
		int mLength;
		int mPosition;
		char mCurrentChar;

		Path mPath;
		char? mCommand;
		int mCommandStart = 0;
		Queue<float> mCommandParameters = new Queue<float>();

		float mSubPathStartX;
		float mSubPathStartY;
		float mLastX;
		float mLastY;
		float mLastCubicBezierX2;
		float mLastCubicBezierY2;
		float mLastQuadraticBezierX2;
		float mLastQuadraticBezierY2;

		RectF mArcRect = new RectF();

		public void parse(SVGProperties pSVGProperties, Canvas pCanvas, SVGPaint pSVGPaint) {
			Path path = this.parse(pSVGProperties);

			bool fill = pSVGPaint.setFill(pSVGProperties);
			if (fill) {
				pCanvas.DrawPath(path, pSVGPaint.getPaint());
			}

			bool stroke = pSVGPaint.setStroke(pSVGProperties);
			if (stroke) {
				pCanvas.DrawPath(path, pSVGPaint.getPaint());
			}

			if(fill || stroke) {
				pSVGPaint.ensureComputedBoundsInclude(path);
			}
		}

		static float RadToDeg (float rad)
		{
			return (float)(rad * 180 / Math.PI);
		}

		static float DegToRad (float deg)
		{
			return (float)(deg * Math.PI / 180);
		}

		/**
		 * Uppercase rules are absolute positions, lowercase are relative.
		 * Types of path rules:
		 * <p/>
		 * <ol>
		 * <li>M/m - (x y)+ - Move to (without drawing)
		 * <li>Z/z - (no params) - Close path (back to starting point)
		 * <li>L/l - (x y)+ - Line to
		 * <li>H/h - x+ - Horizontal ine to
		 * <li>V/v - y+ - Vertical line to
		 * <li>C/c - (x1 y1 x2 y2 x y)+ - Cubic bezier to
		 * <li>S/s - (x2 y2 x y)+ - Smooth cubic bezier to (shorthand that assumes the x2, y2 from previous C/S is the x1, y1 of this bezier)
		 * <li>Q/q - (x1 y1 x y)+ - Quadratic bezier to
		 * <li>T/t - (x y)+ - Smooth quadratic bezier to (assumes previous control point is "reflection" of last one w.r.t. to current point)
		 * <li>A/a - ... - Arc to</li>
		 * </ol>
		 * <p/>
		 * Numbers are separate by whitespace, comma or nothing at all (!) if they are self-delimiting, (ie. begin with a - sign)
		 */
		Path parse(SVGProperties pSVGProperties) {
			String pathString = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_PATHDATA);
			if(pathString == null) {
				return null;
			}

			this.mString = pathString.Trim();
			this.mLastX = 0;
			this.mLastY = 0;
			this.mLastCubicBezierX2 = 0;
			this.mLastCubicBezierY2 = 0;
			this.mCommand = null;
			this.mCommandParameters.Clear();
			this.mPath = new Path();
			if(this.mString.Length == 0) {
				return this.mPath;
			}

			String fillrule = pSVGProperties.getStringProperty(SVGConstants.ATTRIBUTE_FILLRULE);
			if(fillrule != null) {
				if(SVGConstants.ATTRIBUTE_FILLRULE_VALUE_EVENODD.Equals(fillrule)) {
					this.mPath.SetFillType(Path.FillType.EvenOdd);
				} else {
					this.mPath.SetFillType(Path.FillType.Winding);
				}

				/*
				 *  TODO Check against:
				 *  http://www.w3.org/TR/SVG/images/painting/fillrule-nonzero.svg / http://www.w3.org/TR/SVG/images/painting/fillrule-nonzero.png
				 *  http://www.w3.org/TR/SVG/images/painting/fillrule-evenodd.svg / http://www.w3.org/TR/SVG/images/painting/fillrule-evenodd.png
				 */
			}

			this.mCurrentChar = this.mString[0];

			this.mPosition = 0;
			this.mLength = this.mString.Length;
			while (this.mPosition < this.mLength) {
				try {
					this.skipWhitespace();
					if (char.IsLetter(this.mCurrentChar) && (this.mCurrentChar != 'e') && (this.mCurrentChar != 'E')) {
						this.processCommand();

						this.mCommand = this.mCurrentChar;
						this.mCommandStart = this.mPosition;
						this.advance();
					} else {
						float parameter = this.nextFloat();
						this.mCommandParameters.Enqueue(parameter);
					}
				} catch(Exception t) {
					throw new ArgumentException("Error parsing: '" + this.mString.Substring(this.mCommandStart, this.mPosition) + "'. Command: '" + this.mCommand + "'. Parameters: '" + this.mCommandParameters.Count + "'.", t);
				}
			}
			this.processCommand();
			return this.mPath;
		}

		void processCommand() {
			if (this.mCommand != null) {
				// Process command
				this.generatePathElement();
				this.mCommandParameters.Clear();
			}
		}

		void generatePathElement() {
			bool wasCubicBezierCurve = false;
			bool wasQuadraticBezierCurve = false;
			switch (this.mCommand) { // TODO Extract to constants
				case 'm':
					this.generateMove(false);
					break;
				case 'M':
					this.generateMove(true);
					break;
				case 'l':
					this.generateLine(false);
					break;
				case 'L':
					this.generateLine(true);
					break;
				case 'h':
					this.generateHorizontalLine(false);
					break;
				case 'H':
					this.generateHorizontalLine(true);
					break;
				case 'v':
					this.generateVerticalLine(false);
					break;
				case 'V':
					this.generateVerticalLine(true);
					break;
				case 'c':
					this.generateCubicBezierCurve(false);
					wasCubicBezierCurve = true;
					break;
				case 'C':
					this.generateCubicBezierCurve(true);
					wasCubicBezierCurve = true;
					break;
				case 's':
					this.generateSmoothCubicBezierCurve(false);
					wasCubicBezierCurve = true;
					break;
				case 'S':
					this.generateSmoothCubicBezierCurve(true);
					wasCubicBezierCurve = true;
					break;
				case 'q':
					this.generateQuadraticBezierCurve(false);
					wasQuadraticBezierCurve = true;
					break;
				case 'Q':
					this.generateQuadraticBezierCurve(true);
					wasQuadraticBezierCurve = true;
					break;
				case 't':
					this.generateSmoothQuadraticBezierCurve(false);
					wasQuadraticBezierCurve = true;
					break;
				case 'T':
					this.generateSmoothQuadraticBezierCurve(true);
					wasQuadraticBezierCurve = true;
					break;
				case 'a':
					this.generateArc(false);
					break;
				case 'A':
					this.generateArc(true);
					break;
				case 'z':
				case 'Z':
					this.generateClose();
					break;
				default:
					throw new InvalidOperationException ("Unexpected SVG command: " + this.mCommand);
			}
			if (!wasCubicBezierCurve) {
				this.mLastCubicBezierX2 = this.mLastX;
				this.mLastCubicBezierY2 = this.mLastY;
			}
			if (!wasQuadraticBezierCurve) {
				this.mLastQuadraticBezierX2 = this.mLastX;
				this.mLastQuadraticBezierY2 = this.mLastY;
			}
		}

		void assertParameterCountMinimum(int pParameterCount) {
			if (this.mCommandParameters.Count < pParameterCount) {
				throw new InvalidOperationException("Incorrect parameter count: '" + this.mCommandParameters.Count + "'. Expected at least: '" + pParameterCount + "'.");
			}
		}

		void assertParameterCount(int pParameterCount) {
			if (this.mCommandParameters.Count != pParameterCount) {
				throw new InvalidOperationException("Incorrect parameter count: '" + this.mCommandParameters.Count + "'. Expected: '" + pParameterCount + "'.");
			}
		}

		void generateMove(bool pAbsolute) {
			this.assertParameterCountMinimum(2);
			float x = this.mCommandParameters.Dequeue();
			float y = this.mCommandParameters.Dequeue();
			/** Moves the line from mLastX,mLastY to x,y. */
			if (pAbsolute) {
				this.mPath.MoveTo(x, y);
				this.mLastX = x;
				this.mLastY = y;
			} else {
				this.mPath.RMoveTo(x, y);
				this.mLastX += x;
				this.mLastY += y;
			}
			this.mSubPathStartX = this.mLastX;
			this.mSubPathStartY = this.mLastY;
			if(this.mCommandParameters.Count >= 2) {
				this.generateLine(pAbsolute);
			}
		}

		void generateLine(bool pAbsolute) {
			this.assertParameterCountMinimum(2);
			/** Draws a line from mLastX,mLastY to x,y. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 2) {
					float x = this.mCommandParameters.Dequeue();
					float y = this.mCommandParameters.Dequeue();
					this.mPath.LineTo(x, y);
					this.mLastX = x;
					this.mLastY = y;
				}
			} else {
				while(this.mCommandParameters.Count >= 2) {
					float x = this.mCommandParameters.Dequeue();
					float y = this.mCommandParameters.Dequeue();
					this.mPath.RLineTo(x, y);
					this.mLastX += x;
					this.mLastY += y;
				}
			}
		}

		void generateHorizontalLine(bool pAbsolute) {
			this.assertParameterCountMinimum(1);
			/** Draws a horizontal line to the point defined by mLastY and x. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 1) {
					float x = this.mCommandParameters.Dequeue();
					this.mPath.LineTo(x, this.mLastY);
					this.mLastX = x;
				}
			} else {
				while(this.mCommandParameters.Count >= 1) {
					float x = this.mCommandParameters.Dequeue();
					this.mPath.RLineTo(x, 0);
					this.mLastX += x;
				}
			}
		}

		void generateVerticalLine(bool pAbsolute) {
			this.assertParameterCountMinimum(1);
			/** Draws a vertical line to the point defined by mLastX and y. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 1) {
					float y = this.mCommandParameters.Dequeue();
					this.mPath.LineTo(this.mLastX, y);
					this.mLastY = y;
				}
			} else {
				while(this.mCommandParameters.Count >= 1) {
					float y = this.mCommandParameters.Dequeue();
					this.mPath.RLineTo(0, y);
					this.mLastY += y;
				}
			}
		}

		void generateCubicBezierCurve(bool pAbsolute) {
			this.assertParameterCountMinimum(6);
			/** Draws a cubic bezier curve from current pen point to x,y.
			 * x1,y1 and x2,y2 are start and end control points of the curve. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 6) {
					float x1 = this.mCommandParameters.Dequeue();
					float y1 = this.mCommandParameters.Dequeue();
					float x2 = this.mCommandParameters.Dequeue();
					float y2 = this.mCommandParameters.Dequeue();
					float x = this.mCommandParameters.Dequeue();
					float y = this.mCommandParameters.Dequeue();
					this.mPath.CubicTo(x1, y1, x2, y2, x, y);
					this.mLastCubicBezierX2 = x2;
					this.mLastCubicBezierY2 = y2;
					this.mLastX = x;
					this.mLastY = y;
				}
			} else {
				while(this.mCommandParameters.Count >= 6) {
					float x1 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y1 = this.mCommandParameters.Dequeue() + this.mLastY;
					float x2 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y2 = this.mCommandParameters.Dequeue() + this.mLastY;
					float x = this.mCommandParameters.Dequeue() + this.mLastX;
					float y = this.mCommandParameters.Dequeue() + this.mLastY;
					this.mPath.CubicTo(x1, y1, x2, y2, x, y);
					this.mLastCubicBezierX2 = x2;
					this.mLastCubicBezierY2 = y2;
					this.mLastX = x;
					this.mLastY = y;
				}
			}
		}

		void generateSmoothCubicBezierCurve(bool pAbsolute) {
			this.assertParameterCountMinimum(4);
			/** Draws a cubic bezier curve from the last point to x,y.
			 * x2,y2 is the end control point.
			 * The start control point is is assumed to be the same as
			 * the end control point of the previous curve. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 4) {
					float x1 = 2 * this.mLastX - this.mLastCubicBezierX2;
					float y1 = 2 * this.mLastY - this.mLastCubicBezierY2;
					float x2 = this.mCommandParameters.Dequeue();
					float y2 = this.mCommandParameters.Dequeue();
					float x = this.mCommandParameters.Dequeue();
					float y = this.mCommandParameters.Dequeue();
					this.mPath.CubicTo(x1, y1, x2, y2, x, y);
					this.mLastCubicBezierX2 = x2;
					this.mLastCubicBezierY2 = y2;
					this.mLastX = x;
					this.mLastY = y;
				}
			} else {
				while(this.mCommandParameters.Count >= 4) {
					float x1 = 2 * this.mLastX - this.mLastCubicBezierX2;
					float y1 = 2 * this.mLastY - this.mLastCubicBezierY2;
					float x2 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y2 = this.mCommandParameters.Dequeue() + this.mLastY;
					float x = this.mCommandParameters.Dequeue() + this.mLastX;
					float y = this.mCommandParameters.Dequeue() + this.mLastY;
					this.mPath.CubicTo(x1, y1, x2, y2, x, y);
					this.mLastCubicBezierX2 = x2;
					this.mLastCubicBezierY2 = y2;
					this.mLastX = x;
					this.mLastY = y;
				}
			}
		}

		void generateQuadraticBezierCurve(bool pAbsolute) {
			this.assertParameterCountMinimum(4);
			/** Draws a quadratic bezier curve from mLastX,mLastY x,y. x1,y1 is the control point.. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 4) {
					float x1 = this.mCommandParameters.Dequeue();
					float y1 = this.mCommandParameters.Dequeue();
					float x2 = this.mCommandParameters.Dequeue();
					float y2 = this.mCommandParameters.Dequeue();
					this.mPath.QuadTo(x1, y1, x2, y2);
					this.mLastQuadraticBezierX2 = x2;
					this.mLastQuadraticBezierY2 = y2;
					this.mLastX = x2;
					this.mLastY = y2;
				}
			} else {
				while(this.mCommandParameters.Count >= 4) {
					float x1 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y1 = this.mCommandParameters.Dequeue() + this.mLastY;
					float x2 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y2 = this.mCommandParameters.Dequeue() + this.mLastY;
					this.mPath.QuadTo(x1, y1, x2, y2);
					this.mLastQuadraticBezierX2 = x2;
					this.mLastQuadraticBezierY2 = y2;
					this.mLastX = x2;
					this.mLastY = y2;
				}
			}
		}

		void generateSmoothQuadraticBezierCurve(bool pAbsolute) {
			this.assertParameterCountMinimum(2);
			/** Draws a quadratic bezier curve from mLastX,mLastY to x,y.
			 * The control point is assumed to be the same as the last control point used. */
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 2) {
					float x1 = 2 * this.mLastX - this.mLastQuadraticBezierX2;
					float y1 = 2 * this.mLastY - this.mLastQuadraticBezierY2;
					float x2 = this.mCommandParameters.Dequeue();
					float y2 = this.mCommandParameters.Dequeue();
					this.mPath.QuadTo(x1, y1, x2, y2);
					this.mLastQuadraticBezierX2 = x2;
					this.mLastQuadraticBezierY2 = y2;
					this.mLastX = x2;
					this.mLastY = y2;
				}
			} else {
				while(this.mCommandParameters.Count >= 2) {
					float x1 = 2 * this.mLastX - this.mLastQuadraticBezierX2;
					float y1 = 2 * this.mLastY - this.mLastQuadraticBezierY2;
					float x2 = this.mCommandParameters.Dequeue() + this.mLastX;
					float y2 = this.mCommandParameters.Dequeue() + this.mLastY;
					this.mPath.QuadTo(x1, y1, x2, y2);
					this.mLastQuadraticBezierX2 = x2;
					this.mLastQuadraticBezierY2 = y2;
					this.mLastX = x2;
					this.mLastY = y2;
				}
			}
		}

		void generateArc(bool pAbsolute) {
			this.assertParameterCountMinimum(7);
			if(pAbsolute) {
				while(this.mCommandParameters.Count >= 7) {
					float rx = this.mCommandParameters.Dequeue();
					float ry = this.mCommandParameters.Dequeue();
					float theta = this.mCommandParameters.Dequeue();
					bool largeArcFlag = ((int)this.mCommandParameters.Dequeue()) == 1;
					bool sweepFlag = ((int)this.mCommandParameters.Dequeue()) == 1;
					float x = this.mCommandParameters.Dequeue();
					float y = this.mCommandParameters.Dequeue();

					this.generateArc(rx, ry, theta, largeArcFlag, sweepFlag, x, y);

					this.mLastX = x;
					this.mLastY = y;
				}
			} else {
				while(this.mCommandParameters.Count >= 7) {
					float rx = this.mCommandParameters.Dequeue();
					float ry = this.mCommandParameters.Dequeue();
					float theta = this.mCommandParameters.Dequeue();
					bool largeArcFlag = ((int)this.mCommandParameters.Dequeue()) == 1;
					bool sweepFlag = ((int)this.mCommandParameters.Dequeue()) == 1;
					float x = this.mCommandParameters.Dequeue() + this.mLastX;
					float y = this.mCommandParameters.Dequeue() + this.mLastY;

					this.generateArc(rx, ry, theta, largeArcFlag, sweepFlag, x, y);

					this.mLastX = x;
					this.mLastY = y;
				}
			}
		}

		/**
		 * Based on: org.apache.batik.ext.awt.geom.ExtendedGeneralPath.computeArc(...)
		 * @see <a href="http://www.w3.org/TR/SVG/implnote.html#ArcConversionEndpointToCenter">http://www.w3.org/TR/SVG/implnote.html#ArcConversionEndpointToCenter</a>
		 */
		void generateArc(float rx, float ry, float pTheta, bool pLargeArcFlag, bool pSweepFlag, float pX, float pY) {
			/* Compute the half distance between the current and the end point. */
			float dx = (this.mLastX - pX) * 0.5f;
			float dy = (this.mLastY - pY) * 0.5f;

			/* Convert theta to radians. */
			float thetaRad = DegToRad(pTheta % 360f);
			float cosAngle = (float)Math.Cos(thetaRad);
			float sinAngle = (float)Math.Sin(thetaRad);

			/* Step 1 : Compute (x1, y1) */
			float x1 = (cosAngle * dx + sinAngle * dy);
			float y1 = (-sinAngle * dx + cosAngle * dy);

			/* Ensure radii are large enough. */
			float radiusX = Math.Abs(rx);
			float radiusY = Math.Abs(ry);
			float Prx = radiusX * radiusX;
			float Pry = radiusY * radiusY;
			float Px1 = x1 * x1;
			float Py1 = y1 * y1;
			/* Check that radii are large enough. */
			float radiiCheck = Px1/Prx + Py1/Pry;
			if (radiiCheck > 1) {
				radiusX = (float)Math.Sqrt(radiiCheck) * radiusX;
				radiusY = (float)Math.Sqrt(radiiCheck) * radiusY;
				Prx = radiusX * radiusX;
				Pry = radiusY * radiusY;
			}

			/* Step 2 : Compute (cx_dash, cy_dash) */
			float sign = (pLargeArcFlag == pSweepFlag) ? -1 : 1;
			float sq = ((Prx*Pry)-(Prx*Py1)-(Pry*Px1)) / ((Prx*Py1)+(Pry*Px1));
			sq = (sq < 0) ? 0 : sq;
			float coef = (float)(sign * Math.Sqrt(sq));
			float cx_dash = coef * ((radiusX * y1) / radiusY);
			float cy_dash = coef * -((radiusY * x1) / radiusX);

			//- Step 3 : Compute (cx, cy) from (cx_dash, cy_dash) */
			float cx = ((this.mLastX + pX) * 0.5f) + (cosAngle * cx_dash - sinAngle * cy_dash);
			float cy = ((this.mLastY + pY) * 0.5f) + (sinAngle * cx_dash + cosAngle * cy_dash);

			/* Step 4 : Compute the angleStart (angle1) and the sweepAngle (dangle). */
			float ux = (x1 - cx_dash) / radiusX;
			float uy = (y1 - cy_dash) / radiusY;
			float vx = (-x1 - cx_dash) / radiusX;
			float vy = (-y1 - cy_dash) / radiusY;

			/* Compute the startAngle. */
			float p = ux; // (1 * ux) + (0 * uy)
			float n = (float)Math.Sqrt((ux * ux) + (uy * uy));
			sign = (uy < 0) ? -1f : 1f;
			float startAngle = RadToDeg(sign * (float)Math.Acos(p / n));

			/* Compute the sweepAngle. */
			n = (float)Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
			p = ux * vx + uy * vy;
			sign = (ux * vy - uy * vx < 0) ? -1f : 1f;
			float sweepAngle = RadToDeg(sign * (float)Math.Acos(p / n));
			if(!pSweepFlag && sweepAngle > 0) {
				sweepAngle -= 360f;
			} else if (pSweepFlag && sweepAngle < 0) {
				sweepAngle += 360f;
			}
			sweepAngle %= 360f;
			startAngle %= 360f;

			/* Generate bounding rect. */
			float left = cx - radiusX;
			float top = cy - radiusY;
			float right = cx + radiusX;
			float bottom = cy + radiusY;
			this.mArcRect.Set(left, top, right, bottom);

			/* Append the arc to the path. */
			this.mPath.ArcTo(this.mArcRect, startAngle, sweepAngle);
		}

		void generateClose() {
			this.assertParameterCount(0);
			this.mPath.Close();
			this.mLastX = this.mSubPathStartX;
			this.mLastY = this.mSubPathStartY;
		}

		char read() {
			if (mPosition < mLength) {
				mPosition++;
			}
			if (mPosition == mLength) {
				return '\0';
			} else {
				return mString[mPosition];
			}
		}

		public void skipWhitespace() {
			while (mPosition < mLength) {
				if (char.IsWhiteSpace (mString[mPosition])) {
					this.advance();
				} else {
					break;
				}
			}
		}

		public void skipNumberSeparator() {
			while (mPosition < mLength) {
				char c = mString[mPosition];
				switch (c) {
					case ' ':
					case ',':
					case '\n':
					case '\t':
						this.advance();
						break;
					default:
						return;
				}
			}
		}

		public void advance() {
			mCurrentChar = this.read();
		}

		/**
		 * Parses the content of the buffer and converts it to a float.
		 */
		float parseFloat()
		{
			int     mantissa     = 0;
			bool mantPos  = true;

			int     exp      = 0;
			int     expAdj   = 0;
			bool expPos   = true;

			switch (mCurrentChar) {
			case '-':
				mantPos = false;
				break;
			case '+':
				mCurrentChar = this.read ();
				break;
			}

			while (mCurrentChar == '0') {
				mCurrentChar = this.read ();
			}

			if (char.IsDigit (mCurrentChar)) {
				while (char.IsDigit (mCurrentChar)) {
					mantissa = mantissa * 10 + (mCurrentChar - '0');
					mCurrentChar = read ();
				}
			}

			if (mCurrentChar == '.') {
				mCurrentChar = read ();
				while (char.IsDigit (mCurrentChar)) {
					mantissa = mantissa * 10 + (mCurrentChar - '0');
					expAdj++;
					mCurrentChar = read ();
				}
			}

			mantissa = mantPos ? mantissa : -mantissa;

			if (mCurrentChar != 'e' && mCurrentChar != 'E')
				return mantissa / (float)Math.Pow (10, expAdj);

			mCurrentChar = read ();
			switch (mCurrentChar) {
			case '-':
				expPos = false;
				break;
			case '+':
				mCurrentChar = this.read ();
				break;
			}

			while (char.IsDigit (mCurrentChar)) {
				exp = exp * 10 + (mCurrentChar - '0');
				mCurrentChar = read ();
			}

			exp = expPos ? exp : -exp;

			return this.buildFloat (mantissa, exp);
		}

		public float nextFloat() {
			this.skipWhitespace();
			float f = this.parseFloat();
			this.skipNumberSeparator();
			return f;
		}

		public float buildFloat(int pMantissa, int pExponent) {
			if (pExponent < -125 || pMantissa == 0) {
				return 0.0f;
			}

			if (pExponent >=  128) {
				return (pMantissa > 0) ? float.PositiveInfinity : float.NegativeInfinity;
			}

			if (pExponent == 0) {
				return pMantissa;
			}

			if (pMantissa >= (1 << 26)) {
				pMantissa++;  // round up trailing bits if they will be dropped.
			}

			return (float) ((pExponent > 0) ? pMantissa * Math.Pow (10, pExponent) : pMantissa / Math.Pow (10, -pExponent));
		}
	}
}

