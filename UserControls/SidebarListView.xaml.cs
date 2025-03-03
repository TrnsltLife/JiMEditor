using System;
using System.Windows.Controls;
using System.ComponentModel;

namespace JiME.UserControls
{
	/// <summary>
	/// Interaction logic for SidebarListView.xaml
	/// </summary>
	public partial class SidebarListView : UserControl, INotifyPropertyChanged
	{
		public string Title { get; set; }
		public Array ListData { get; set; }

		public EventHandler onAddEvent, onRemoveEvent, onSettingsEvent, onDuplicateEvent, onImportEvent, onExportEvent;
		public bool _showDuplicateButton = false;
		public string _duplicateButtonVisibility = "Collapsed";

		public bool _showImportButton = false;
		public string _importButtonVisibility = "Visible";

		public bool _showExportButton = false;
		public string _exportButtonVisibility = "Collapsed";

        public event Action<string> NewItemSelectedEvent;

		public bool ShowDuplicateButton
		{
			get => _showDuplicateButton;
			set
			{
				_showDuplicateButton = value;
				_duplicateButtonVisibility = _showDuplicateButton ? "Visible" : "Collapsed";
				PropChanged("ShowDuplicateButton");
			}
		}

		public string DuplicateButtonVisibility
        {
			get => _duplicateButtonVisibility;
			set
			{
				_duplicateButtonVisibility = value;
				PropChanged("DuplicateButtonVisibility");
			}
        }

		public bool ShowImportButton
		{
			get => _showImportButton;
			set
			{
				_showImportButton = value;
				_importButtonVisibility = _showImportButton ? "Visible" : "Collapsed";
				PropChanged("ShowImportButton");
			}
		}

		public string ImportButtonVisibility
		{
			get => _importButtonVisibility;
			set
			{
				_importButtonVisibility = value;
				PropChanged("ImportButtonVisibility");
			}
		}

		public bool ShowExportButton
		{
			get => _showExportButton;
			set
			{
				_showExportButton = value;
				_exportButtonVisibility = _showExportButton ? "Visible" : "Collapsed";
				PropChanged("ShowExportButton");
			}
		}

		public string ExportButtonVisibility
		{
			get => _exportButtonVisibility;
			set
			{
				_exportButtonVisibility = value;
				PropChanged("ExportButtonVisibility");
			}
		}

		public SidebarListView()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void AddInteraction_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onAddEvent?.Invoke( sender, e );
		}

		private void Settings_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onSettingsEvent?.Invoke( sender, e );
		}

		private void RemoveInteraction_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onRemoveEvent?.Invoke( sender, e );
		}

        private void DataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var newSelection = e.AddedItems[0];
                if (newSelection is IInteraction i)
                {
                    NewItemSelectedEvent?.Invoke(i.dataName);
                }
                else if (newSelection is ICommonData c)
                {
                    NewItemSelectedEvent?.Invoke(c.dataName);
                }
            }
        }

        private void Duplicate_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			onDuplicateEvent?.Invoke(sender, e);
		}

		private void Import_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			onImportEvent?.Invoke(sender, e);
		}

		private void Export_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			onExportEvent?.Invoke(sender, e);
		}


		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
