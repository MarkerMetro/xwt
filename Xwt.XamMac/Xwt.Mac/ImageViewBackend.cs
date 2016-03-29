// 
// ImageViewBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Xwt.Backends;
using Xwt.Drawing;
using CoreGraphics;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.ObjCRuntime;
using MonoMac.AppKit;
#else
using ObjCRuntime;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class ImageViewBackend: ViewBackend<NSImageView,IWidgetEventSink>, IImageViewBackend
	{
		public ImageViewBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new CustomNSImageView (EventSink, ApplicationContext);
		}

		protected override Size GetNaturalSize ()
		{
			NSImage img = Widget.Image;
			return img == null ? Size.Zero : img.Size.ToXwtSize ();
		}

		public void SetImage (ImageDescription image)
		{
			if (image.IsNull) {
				Widget.Image = null;
				return;
			}

			Widget.Image = image.ToNSImage ();
			Widget.SetFrameSize (Widget.Image.Size);
		}
	}
	
	class CustomNSImageView: NSImageView, IViewObject
	{
		IWidgetEventSink eventSink;
		ApplicationContext context;
		NSTrackingArea trackingArea;	// Captures Mouse Entered, Exited, and Moved events

		public CustomNSImageView(IWidgetEventSink eventSink, ApplicationContext context) 
		{
			this.eventSink = eventSink;
			this.context = context;
		}

		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			CGRect viewBounds = this.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (viewBounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseDown (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Left;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void MouseUp (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = (PointerButton) (int)theEvent.ButtonNumber + 1;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			context.InvokeUserCode (delegate {
				eventSink.OnMouseEntered ();
			});
		}

		public override void MouseExited (NSEvent theEvent)
		{
			context.InvokeUserCode (delegate {
				eventSink.OnMouseExited ();
			});
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
		}
			
		public override void KeyDown (NSEvent theEvent)
		{
			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			context.InvokeUserCode (delegate {
				eventSink.OnKeyPressed (keyArgs);
			});
			if (keyArgs.Handled)
				return;

			var textArgs = new TextInputEventArgs (theEvent.Characters);
			if (!String.IsNullOrEmpty(theEvent.Characters))
				context.InvokeUserCode (delegate {
					eventSink.OnTextInput (textArgs);
				});
			if (textArgs.Handled)
				return;

			base.KeyDown (theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			var keyArgs = theEvent.ToXwtKeyEventArgs ();
			context.InvokeUserCode (delegate {
				eventSink.OnKeyReleased (keyArgs);
			});
			if (!keyArgs.Handled)
				base.KeyUp (theEvent);
		}
	}
}

