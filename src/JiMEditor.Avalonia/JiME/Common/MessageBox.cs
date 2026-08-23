// Mac MVP step 6: Avalonia replacement for System.Windows.MessageBox (WPF-compatible API).
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using JiMEditor.Avalonia;

namespace JiME
{
	public enum MessageBoxButton
	{
		OK = 0,
		OKCancel = 1,
		YesNoCancel = 3,
		YesNo = 4,
	}

	public enum MessageBoxImage
	{
		None = 0,
		Error = 16,
		Hand = 16,
		Stop = 16,
		Question = 32,
		Exclamation = 48,
		Warning = 48,
		Asterisk = 64,
		Information = 64,
	}

	public enum MessageBoxResult
	{
		None = 0,
		OK = 1,
		Cancel = 2,
		Yes = 6,
		No = 7,
	}

	/// <summary>Cross-platform modal dialogs with the same call shape as WPF <c>System.Windows.MessageBox</c>.</summary>
	public static class MessageBox
	{
		public static MessageBoxResult Show( string messageBoxText ) =>
			Show( messageBoxText, "", MessageBoxButton.OK, MessageBoxImage.None );

		public static MessageBoxResult Show( string messageBoxText, string caption ) =>
			Show( messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None );

		public static MessageBoxResult Show( string messageBoxText, string caption, MessageBoxButton button ) =>
			Show( messageBoxText, caption, button, MessageBoxImage.None );

		public static MessageBoxResult Show(
			string messageBoxText,
			string caption,
			MessageBoxButton button,
			MessageBoxImage icon )
		{
			if ( Dispatcher.UIThread.CheckAccess() )
				return ShowOnUiThread( messageBoxText, caption, button, icon );

			return Dispatcher.UIThread
				.InvokeAsync( () => ShowOnUiThread( messageBoxText, caption, button, icon ) )
				.GetAwaiter()
				.GetResult();
		}

		static MessageBoxResult ShowOnUiThread(
			string messageBoxText,
			string caption,
			MessageBoxButton button,
			MessageBoxImage icon )
		{
			if ( ApplicationEnvironment.IsHeadless )
				return DefaultResult( button );

			var dialog = new MessageDialogWindow( messageBoxText, caption ?? "", button, icon );
			var owner = GetOwnerWindow();

			if ( owner == null )
				return ShowWithoutOwner( dialog, button );

			return ShowDialogSync( dialog, owner );
		}

		static MessageBoxResult ShowWithoutOwner( MessageDialogWindow dialog, MessageBoxButton button )
		{
			var frame = new DispatcherFrame();
			var result = DefaultResult( button );
			dialog.Closed += ( _, _ ) =>
			{
				result = dialog.Result;
				frame.Continue = false;
			};
			dialog.Show();
			Dispatcher.UIThread.PushFrame( frame );
			return result;
		}

		/// <summary>
		/// WPF-compatible blocking show. Uses a nested <see cref="DispatcherFrame"/> so nested modal
		/// dialogs (e.g. MessageBox from inside <c>ShowDialog</c>) stay interactive on macOS.
		/// </summary>
		static MessageBoxResult ShowDialogSync( MessageDialogWindow dialog, Window owner )
		{
			var frame = new DispatcherFrame();
			var result = DefaultResult( dialog.Buttons );
			Exception? fault = null;

			_ = Dispatcher.UIThread.InvokeAsync( async () =>
			{
				try
				{
					result = await dialog.ShowDialog<MessageBoxResult>( owner ).ConfigureAwait( true );
				}
				catch ( Exception ex )
				{
					fault = ex;
				}
				finally
				{
					frame.Continue = false;
				}
			} );

			Dispatcher.UIThread.PushFrame( frame );

			if ( fault != null )
				throw fault;

			return result;
		}

		static MessageBoxResult DefaultResult( MessageBoxButton button ) =>
			button switch
			{
				MessageBoxButton.OKCancel => MessageBoxResult.Cancel,
				MessageBoxButton.YesNo => MessageBoxResult.No,
				MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
				_ => MessageBoxResult.OK,
			};

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

	sealed class MessageDialogWindow : Window
	{
		public MessageBoxButton Buttons { get; }
		public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

		public MessageDialogWindow(
			string message,
			string caption,
			MessageBoxButton buttons,
			MessageBoxImage icon )
		{
			Buttons = buttons;
			Title = string.IsNullOrEmpty( caption ) ? "JiMEditor" : caption;
			Width = 440;
			MinWidth = 320;
			SizeToContent = SizeToContent.Height;
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			CanResize = false;
			ShowInTaskbar = false;

			var messageBlock = new TextBlock
			{
				Text = message ?? "",
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness( 16, 16, 16, 8 ),
			};

			var buttonPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				Spacing = 8,
				Margin = new Thickness( 16, 8, 16, 16 ),
			};

			void AddButton( string label, MessageBoxResult result, bool isDefault = false, bool isCancel = false )
			{
				var b = new Button
				{
					Content = label,
					MinWidth = 72,
					IsDefault = isDefault,
					IsCancel = isCancel,
				};
				b.Click += ( _, _ ) =>
				{
					Result = result;
					Close( result );
				};
				buttonPanel.Children.Add( b );
			}

			switch ( buttons )
			{
				case MessageBoxButton.OKCancel:
					AddButton( "OK", MessageBoxResult.OK, isDefault: true );
					AddButton( "Cancel", MessageBoxResult.Cancel, isCancel: true );
					Result = MessageBoxResult.Cancel;
					break;
				case MessageBoxButton.YesNo:
					AddButton( "Yes", MessageBoxResult.Yes, isDefault: true );
					AddButton( "No", MessageBoxResult.No, isCancel: true );
					Result = MessageBoxResult.No;
					break;
				case MessageBoxButton.YesNoCancel:
					AddButton( "Yes", MessageBoxResult.Yes, isDefault: true );
					AddButton( "No", MessageBoxResult.No );
					AddButton( "Cancel", MessageBoxResult.Cancel, isCancel: true );
					Result = MessageBoxResult.Cancel;
					break;
				default:
					AddButton( "OK", MessageBoxResult.OK, isDefault: true, isCancel: true );
					Result = MessageBoxResult.OK;
					break;
			}

			// Icon enum kept for WPF call-site compatibility; title/message carry the meaning.
			_ = icon;

			Content = new StackPanel
			{
				Children = { messageBlock, buttonPanel },
			};

			Closing += ( _, _ ) =>
			{
				if ( Result == MessageBoxResult.None )
					Result = DefaultCloseResult( buttons );
			};
		}

		static MessageBoxResult DefaultCloseResult( MessageBoxButton button ) =>
			button switch
			{
				MessageBoxButton.OKCancel => MessageBoxResult.Cancel,
				MessageBoxButton.YesNo => MessageBoxResult.No,
				MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
				_ => MessageBoxResult.OK,
			};
	}
}
