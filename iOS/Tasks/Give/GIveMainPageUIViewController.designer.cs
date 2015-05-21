// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iOS
{
	[Register ("GIveMainPageUIViewController")]
	partial class GIveMainPageUIViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel GiveBanner { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView GiveBannerLayer { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (GiveBanner != null) {
				GiveBanner.Dispose ();
				GiveBanner = null;
			}
			if (GiveBannerLayer != null) {
				GiveBannerLayer.Dispose ();
				GiveBannerLayer = null;
			}
		}
	}
}
