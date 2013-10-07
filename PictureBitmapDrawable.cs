using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace XamSvg
{
	public class PictureBitmapDrawable : Drawable
	{
		Bitmap currentBitmap;

		public PictureBitmapDrawable (Picture pic)
		{
			Picture = pic;
		}

		public Picture Picture {
			get;
			private set;
		}

		public override void Draw (Canvas canvas)
		{
			canvas.DrawBitmap (currentBitmap, 0, 0, null);
		}

		public override int Opacity {
			get {
				return (int)Format.Translucent;
			}
		}

		protected override void OnBoundsChange (Rect bounds)
		{
			var width = bounds.Width ();
			var height = bounds.Height ();
			if (currentBitmap == null || currentBitmap.Height != height || currentBitmap.Width != width)
				currentBitmap = SvgFactory.MakeBitmapFromPicture (Picture, width, height);
		}

		public override int IntrinsicWidth {
			get {
				return Picture != null ? Picture.Width : 0;
			}
		}

		public override int IntrinsicHeight {
			get {
				return Picture != null ? Picture.Height : 0;
			}
		}

		public override void SetFilterBitmap (bool filter)
		{
		}

		public override void SetDither (bool dither)
		{
		}

		public override void SetColorFilter (ColorFilter cf)
		{
		}

		public override void SetAlpha (int alpha)
		{
		}
	}
}
