//
// MacDesktopBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Collections.Generic;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class MacDesktopBackend: DesktopBackend
	{
		#region implemented abstract members of DesktopBackend

		internal static MacDesktopBackend Instance;
		internal static Rectangle desktopBounds;

		public MacDesktopBackend ()
		{
			Instance = this;
			CalcDesktopBounds ();
		}

		internal void NotifyScreensChanged ()
		{
			CalcDesktopBounds ();
			OnScreensChanged ();
		}

		static void CalcDesktopBounds ()
		{
			desktopBounds = new Rectangle ();
			foreach (var s in NSScreen.Screens) {
				var r = s.Frame;
				desktopBounds = desktopBounds.Union (new Rectangle (r.X, r.Y, r.Width, r.Height));
			}
		}

		public override IEnumerable<object> GetScreens ()
		{
			return NSScreen.Screens;
		}

		public override bool IsPrimaryScreen (object backend)
		{
			return NSScreen.Screens[0] == (NSScreen) backend;
		}

		public static Rectangle ToDesktopRect (System.Drawing.RectangleF r)
		{
			r.Y = (float)desktopBounds.Height - r.Y - r.Height;
			if (desktopBounds.Y < 0)
				r.Y += (float)desktopBounds.Y;
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public static System.Drawing.RectangleF FromDesktopRect (Rectangle r)
		{
			r.Y = (float)desktopBounds.Height - r.Y - r.Height;
			if (desktopBounds.Y < 0)
				r.Y += (float)desktopBounds.Y;
			return new System.Drawing.RectangleF ((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
		}
		
		public override Rectangle GetScreenBounds (object backend)
		{
			var r = ((NSScreen)backend).Frame;
			return ToDesktopRect (r);
		}

		public override Rectangle GetScreenVisibleBounds (object backend)
		{
			var r = ((NSScreen)backend).VisibleFrame;
			return ToDesktopRect (r);
		}

		public override string GetScreenDeviceName (object backend)
		{
			return ((NSScreen)backend).DeviceDescription ["NSScreenNumber"].ToString ();
		}

		#endregion
	}
}
