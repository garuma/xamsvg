using System;
using System.Linq;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using XamSvg;

namespace XamSvgTests
{
	[Activity (Label = "XamSvgTests", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
	public class MainActivity : Activity
	{
		int[] rawIds;
		int[] drawableViewIds;

		int currentID = 0;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			rawIds = typeof(Resource.Raw)
				.GetFields ()
				.Where (f => f.IsLiteral)
				.Select (f => (int)f.GetRawConstantValue ())
				.ToArray ();
			drawableViewIds = typeof(Resource.Id)
				.GetFields ()
				.Where (f => f.IsLiteral && f.Name.StartsWith ("drawable"))
				.Select (f => (int)f.GetRawConstantValue ())
				.ToArray ();

			LoadImageTest (rawIds[currentID++]);
			var contentView = FindViewById (Android.Resource.Id.Content);
			contentView.Click += (sender, e) => LoadImageTest (rawIds[(currentID++) % rawIds.Length]);
		}

		void LoadImageTest (int rawID)
		{
			foreach (var drawableID in drawableViewIds) {
				var drawable = SvgFactory.GetDrawable (Resources, rawID);
				var v = FindViewById<ImageView> (drawableID);
				v.SetImageDrawable (drawable);
			}

			var bitmap1 = SvgFactory.GetBitmap (Resources, rawID,
			                                    width: 40,
			                                    height: 69);
			var bitmap2 = SvgFactory.GetBitmap (Resources, rawID,
			                                    width: 98,
			                                    height: 98);
			var bitmap3 = SvgFactory.GetBitmap (Resources, rawID,
			                                    width: 47,
			                                    height: 32);
			FindViewById<ImageView> (Resource.Id.bitmap1).SetImageBitmap (bitmap1);
			FindViewById<ImageView> (Resource.Id.bitmap2).SetImageBitmap (bitmap2);
			FindViewById<ImageView> (Resource.Id.bitmap3).SetImageBitmap (bitmap3);
		}
	}
}
