// Mac Dock icon for unpackaged runs (`./run.sh` / `dotnet run`). ApplicationIcon in the csproj is Windows-only.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Platform;

namespace JiMEditor.Avalonia;

static class MacDockIcon
{
	const string RingAsset = "avares://JiMEditor.Avalonia/Assets/ring.png";

	public static void ApplyRingIcon()
	{
		if ( !OperatingSystem.IsMacOS() )
			return;
		try
		{
			using var stream = AssetLoader.Open( new Uri( RingAsset ) );
			var tmp = Path.Combine( Path.GetTempPath(), "JiMEditor-Avalonia-dock-ring.png" );
			using ( var fs = File.Create( tmp ) )
				stream.CopyTo( fs );
			Native.SetApplicationIconFromPngFile( tmp );
		}
		catch
		{
			// Cosmetic only; window chrome still uses Window.Icon in App.axaml.
		}
	}

	[SupportedOSPlatform( "macos" )]
	static class Native
	{
		const string ObjC = "/usr/lib/libobjc.A.dylib";

		[DllImport( ObjC )]
		static extern nint objc_getClass( string name );

		[DllImport( ObjC )]
		static extern nint sel_registerName( string name );

		[DllImport( ObjC, EntryPoint = "objc_msgSend" )]
		static extern nint objc_msgSend( nint receiver, nint selector );

		[DllImport( ObjC, EntryPoint = "objc_msgSend" )]
		static extern nint objc_msgSend( nint receiver, nint selector, nint arg );

		public static void SetApplicationIconFromPngFile( string pngPath )
		{
			var nsPath = NsString( pngPath );
			if ( nsPath == 0 )
				return;

			var imageClass = objc_getClass( "NSImage" );
			var alloc = objc_msgSend( imageClass, sel_registerName( "alloc" ) );
			var image = objc_msgSend( alloc, sel_registerName( "initWithContentsOfFile:" ), nsPath );
			if ( image == 0 )
				return;

			var app = objc_msgSend( objc_getClass( "NSApplication" ), sel_registerName( "sharedApplication" ) );
			if ( app == 0 )
				return;
			objc_msgSend( app, sel_registerName( "setApplicationIconImage:" ), image );
		}

		static nint NsString( string value )
		{
			var utf8 = Marshal.StringToCoTaskMemUTF8( value );
			try
			{
				return objc_msgSend(
					objc_getClass( "NSString" ),
					sel_registerName( "stringWithUTF8String:" ),
					utf8 );
			}
			finally
			{
				Marshal.FreeCoTaskMem( utf8 );
			}
		}
	}
}
