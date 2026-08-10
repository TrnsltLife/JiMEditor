// Mac MVP step 7: Avalonia StorageProvider replacements for Microsoft.Win32 Open/SaveFileDialog.
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace JiME
{
	/// <summary>Synchronous file pickers for legacy JiME managers (WPF dialog call shape).</summary>
	public static class FileDialogs
	{
		public static string? PickSaveFile(
			string title,
			string? suggestedFileName,
			string defaultExtension,
			string filterName,
			string filterPattern,
			string? initialDirectory )
		{
			return RunUiSync( () => PickSaveFileAsync(
				title, suggestedFileName, defaultExtension, filterName, filterPattern, initialDirectory ) );
		}

		public static string? PickOpenFile(
			string title,
			string defaultExtension,
			string filterName,
			string filterPattern,
			string? initialDirectory )
		{
			return RunUiSync( () => PickOpenFileAsync(
				title, defaultExtension, filterName, filterPattern, initialDirectory ) );
		}

		static async Task<string?> PickSaveFileAsync(
			string title,
			string? suggestedFileName,
			string defaultExtension,
			string filterName,
			string filterPattern,
			string? initialDirectory )
		{
			var owner = GetOwnerWindow();
			if ( owner == null )
				return null;

			var start = await TryGetFolderAsync( owner, initialDirectory );
			var ext = NormalizeExtension( defaultExtension );
			var file = await owner.StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions
			{
				Title = title,
				SuggestedFileName = suggestedFileName,
				SuggestedStartLocation = start,
				DefaultExtension = ext.TrimStart( '.' ),
				FileTypeChoices = new[]
				{
					new FilePickerFileType( filterName ) { Patterns = new[] { filterPattern } }
				}
			} );
			return ToLocalPath( file );
		}

		static async Task<string?> PickOpenFileAsync(
			string title,
			string defaultExtension,
			string filterName,
			string filterPattern,
			string? initialDirectory )
		{
			var owner = GetOwnerWindow();
			if ( owner == null )
				return null;

			_ = defaultExtension;
			var start = await TryGetFolderAsync( owner, initialDirectory );
			var files = await owner.StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions
			{
				Title = title,
				AllowMultiple = false,
				SuggestedStartLocation = start,
				FileTypeFilter = new[]
				{
					new FilePickerFileType( filterName ) { Patterns = new[] { filterPattern } }
				}
			} );
			return files.Count > 0 ? ToLocalPath( files[0] ) : null;
		}

		static async Task<IStorageFolder?> TryGetFolderAsync( TopLevel owner, string? path )
		{
			if ( string.IsNullOrWhiteSpace( path ) || !Directory.Exists( path ) )
				return null;
			return await owner.StorageProvider.TryGetFolderFromPathAsync( path );
		}

		static string NormalizeExtension( string defaultExtension )
		{
			if ( string.IsNullOrWhiteSpace( defaultExtension ) )
				return "";
			return defaultExtension.StartsWith( '.' ) ? defaultExtension : "." + defaultExtension;
		}

		static string? ToLocalPath( IStorageFile? file )
		{
			if ( file == null )
				return null;
			var uri = file.Path;
			if ( uri.IsAbsoluteUri && uri.IsFile )
				return uri.LocalPath;
			return uri.ToString();
		}

		static T RunUiSync<T>( Func<Task<T>> work )
		{
			if ( !Dispatcher.UIThread.CheckAccess() )
			{
				return Dispatcher.UIThread
					.InvokeAsync( () => RunUiSync( work ) )
					.GetAwaiter()
					.GetResult();
			}

			T result = default!;
			var done = false;
			Exception? fault = null;

			work().ContinueWith( t =>
			{
				if ( t.IsFaulted )
					fault = t.Exception?.InnerException ?? t.Exception;
				else if ( t.IsCompletedSuccessfully )
					result = t.Result;
				done = true;
			}, TaskScheduler.Default );

			while ( !done )
			{
				Dispatcher.UIThread.RunJobs();
				Thread.Sleep( 1 );
			}

			if ( fault != null )
				throw fault;

			return result;
		}

		static Window? GetOwnerWindow()
		{
			if ( Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
			{
				if ( desktop.Windows.Count > 0 )
				{
					var active = desktop.Windows.FirstOrDefault( w => w.IsActive );
					if ( active != null )
						return active;
					if ( desktop.MainWindow != null )
						return desktop.MainWindow;
					return desktop.Windows[^1];
				}
				return desktop.MainWindow;
			}
			return null;
		}
	}
}
