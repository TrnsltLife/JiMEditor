// Ported from: JiMEditor/Views/ImageEditorWindow.xaml.cs (2026-04-12)
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using JiMEditor.Avalonia;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class CoverImageEditorWindow : Window
{
	const int MaxImageWidth = 2560;
	const int MaxImageHeight = 1440;

	public bool Accepted { get; private set; }
	public string? CoverImage { get; private set; }

	public CoverImageEditorWindow()
	{
		InitializeComponent();
		Loaded += ( _, _ ) => ScenarioDirty.WatchInputs( this );
	}

	public CoverImageEditorWindow( string? initialCoverImage ) : this()
	{
		CoverImage = string.IsNullOrWhiteSpace( initialCoverImage ) ? null : initialCoverImage;
		UpdatePreview();
	}

	void UpdatePreview()
	{
		if ( string.IsNullOrWhiteSpace( CoverImage ) )
		{
			ImagePreview.Source = null;
			EmptyStateText.IsVisible = true;
			ImageSizeText.Text = "";
			return;
		}

		try
		{
			var bytes = Convert.FromBase64String( CoverImage );
			using var stream = new MemoryStream( bytes );
			var bitmap = new Bitmap( stream );
			ImagePreview.Source = bitmap;
			ImageSizeText.Text = $"{bitmap.PixelSize.Width} x {bitmap.PixelSize.Height}";
			EmptyStateText.IsVisible = false;
		}
		catch
		{
			ImagePreview.Source = null;
			EmptyStateText.Text = "Unable to preview image data.";
			EmptyStateText.IsVisible = true;
			ImageSizeText.Text = "";
		}
	}

	async void ChooseImageButton_Click( object? sender, RoutedEventArgs e )
	{
		try
		{
			var files = await StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions
			{
				Title = "Open Image",
				AllowMultiple = false,
				FileTypeFilter = new[]
				{
					new FilePickerFileType( "Image Files" ) { Patterns = new[] { "*.jpg", "*.jpeg", "*.png" } }
				}
			} );
			if ( files.Count == 0 )
				return;

			var bytes = await ReadAllBytesAsync( files[0] );
			var scaledBytes = ScaleDownIfNeeded( bytes, MaxImageWidth, MaxImageHeight );
			CoverImage = Convert.ToBase64String( scaledBytes );
			UpdatePreview();
			ScenarioDirty.CurrentScenario?.MarkDirty();
		}
		catch ( Exception ex )
		{
			MessageBox.Show(
				"Could not load image.\r\n\r\nException:\r\n" + ex.Message,
				"Image Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error );
		}
	}

	void ClearImageButton_Click( object? sender, RoutedEventArgs e )
	{
		CoverImage = null;
		UpdatePreview();
		ScenarioDirty.CurrentScenario?.MarkDirty();
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = true;
		Close();
	}

	void CancelButton_Click( object? sender, RoutedEventArgs e )
	{
		Accepted = false;
		Close();
	}

	static async Task<byte[]> ReadAllBytesAsync( IStorageFile file )
	{
		await using var input = await file.OpenReadAsync();
		using var ms = new MemoryStream();
		await input.CopyToAsync( ms );
		return ms.ToArray();
	}

	static byte[] ScaleDownIfNeeded( byte[] imageBytes, int maxWidth, int maxHeight )
	{
		using var sourceStream = new MemoryStream( imageBytes );
		using var sourceBitmap = new Bitmap( sourceStream );

		var sourceWidth = sourceBitmap.PixelSize.Width;
		var sourceHeight = sourceBitmap.PixelSize.Height;
		if ( sourceWidth <= maxWidth && sourceHeight <= maxHeight )
			return imageBytes;

		double scale = Math.Min( (double)maxWidth / sourceWidth, (double)maxHeight / sourceHeight );
		int targetWidth = Math.Max( 1, (int)Math.Round( sourceWidth * scale ) );
		int targetHeight = Math.Max( 1, (int)Math.Round( sourceHeight * scale ) );

		using var resized = new RenderTargetBitmap( new PixelSize( targetWidth, targetHeight ) );
		using ( var dc = resized.CreateDrawingContext( false ) )
		{
			dc.DrawImage(
				sourceBitmap,
				new Rect( 0, 0, sourceWidth, sourceHeight ),
				new Rect( 0, 0, targetWidth, targetHeight ) );
		}

		using var outStream = new MemoryStream();
		resized.Save( outStream );
		return outStream.ToArray();
	}
}
