using System;
using Android.Graphics;

namespace XamSvg
{
	public class Svg
	{
		public Svg (Picture pPicture, RectF pBounds, RectF pComputedBounds)
		{
			Picture = pPicture;
			Bounds = pBounds;
			ComputedBounds = pComputedBounds;
		}

		public Picture Picture {
			get;
			private set;
		}

		public RectF Bounds {
			get;
			private set;
		}

		public RectF ComputedBounds {
			get;
			private set;
		}
	}
}
