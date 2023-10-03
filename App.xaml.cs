using JiME.Views;
using System.Threading;
using System.Windows;

namespace JiME
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static MagWizWindow magWizWindow;
		public static Thread magWizWindowThread;

		private void Application_Startup( object sender, StartupEventArgs e )
		{
			//PresentationTraceSources.Refresh();
			//PresentationTraceSources.DataBindingSource.Listeners.Add( new ConsoleTraceListener() );
			//PresentationTraceSources.DataBindingSource.Listeners.Add( new DebugTraceListener() );
			//PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;

#if !DEBUG
			Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
#endif
		}

		public static void OpenMagWizWindow()
        {
			// https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
			if(magWizWindowThread != null)
            {
				CloseMagWizWindow();
            }

			magWizWindowThread = new Thread(new ThreadStart(MagWizThreadStartingPoint));
			magWizWindowThread.SetApartmentState(ApartmentState.STA);
			magWizWindowThread.IsBackground = true;
			magWizWindowThread.Start();
		}

		public static void CloseMagWizWindow()
        {
			if (magWizWindowThread != null)
			{
				magWizWindowThread.Abort();
				magWizWindow = null;
				magWizWindowThread = null;
			}
        }

		private static void MagWizThreadStartingPoint()
		{
			magWizWindow = new MagWizWindow();
			magWizWindow.Show();
			System.Windows.Threading.Dispatcher.Run();
		}

		//generic, app-wide error handler to catch any unhandled exceptions
		private void Dispatcher_UnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e )
		{
			if ( e.Exception.InnerException != null )
			{
				string[] inner;
				inner = e.Exception.InnerException.ToString().Split( new string[] { "at" }, 2, System.StringSplitOptions.None );

				MessageBox.Show(
					"An unhandled exception occurred: \r\n\r\n"
					+ inner[0]
					+ "\r\n\r\nStack Trace:\r\n"
					+ inner[1], "App Exception", MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown();
				e.Handled = true;
			}
			else
			{
				MessageBox.Show(
					"An unhandled exception occurred: \r\n\r\n"
					+ e.Exception.Message
					+ "\r\n\r\nStack Trace:\r\n"
					+ e.Exception.StackTrace, "App Exception", MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown();
				e.Handled = true;
			}
		}
	}
}
