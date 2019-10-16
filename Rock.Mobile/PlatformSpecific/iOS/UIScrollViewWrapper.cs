#if __IOS__
using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
    /// <summary>
    /// Designed to forward touch events to the parent
    /// </summary>
	partial class UIScrollViewWrapper : UIScrollView
	{
        public UIViewController Parent { get; set; }

		public UIScrollViewWrapper (IntPtr handle) : base (handle)
		{
		}

        public UIScrollViewWrapper () : base ()
        {
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if ( Parent != null )
            {
                Parent.TouchesBegan( touches, evt );
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            if ( Parent != null )
            {
                Parent.TouchesEnded( touches, evt );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if ( Parent != null )
            {
                Parent.TouchesMoved( touches, evt );
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            if ( Parent != null )
            {
                Parent.TouchesCancelled( touches, evt );
            }
        }
	}
}
#endif