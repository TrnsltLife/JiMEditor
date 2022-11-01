using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ImageEditorWindow.xaml
	/// </summary>
	public partial class ImageEditorWindow : Window
	{
		public Scenario scenario { get; set; }
		public string coverImage { get; set; }

		public ImageEditorWindow( Scenario s )
		{
			InitializeComponent();
			DataContext = this;

			SourceInitialized += ( x, y ) =>
			{
				this.HideMinimizeAndMaximizeButtons();
			};

			scenario = s;
			coverImage = s.coverImage;

			UpdateInfo();
		}

		private BitmapSource BitmapFromBase64()
		{
			if(coverImage == null || coverImage.Length == 0) { return null; }

			var bytes = Convert.FromBase64String(coverImage);

			using (var stream = new MemoryStream(bytes))
			{
				return BitmapFrame.Create(stream,
					BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
			}
		}

		void UpdateInfo()
		{
			imagePreview.Source = BitmapFromBase64();
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}
		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void imageBtn_Click( object sender, RoutedEventArgs e )
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.DefaultExt = "*.jpg;*.png";
			openFileDialog.Title = "Open Image";
			openFileDialog.Filter = "Image File (*.jpg,.png)|*.jpg;*.png";
			openFileDialog.CheckPathExists = true;
			openFileDialog.CheckFileExists = true;
			string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
			openFileDialog.InitialDirectory = basePath;
			if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{			
				string fileName = openFileDialog.FileName;
				Debug.Log("open filename " + fileName);
				Byte[] bytes = File.ReadAllBytes(fileName);
				String base64 = Convert.ToBase64String(bytes);
				coverImage = base64;
				UpdateInfo();
            }
		}

		private void imageClearBtn_Click(object sender, RoutedEventArgs e)
        {
			coverImage = null;
			UpdateInfo();
        }
	}
}
