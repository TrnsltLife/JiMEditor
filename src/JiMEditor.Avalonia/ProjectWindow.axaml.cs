// Ported from: JiMEditor/ProjectWindow.xaml.cs (2026-04-04)

using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using JiME;

namespace JiMEditor.Avalonia;

public partial class ProjectWindow : Window, INotifyPropertyChanged
{
	PropertyChangedEventHandler? _inpc;

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => _inpc += value;
		remove => _inpc -= value;
	}

	public ObservableCollection<ProjectItem> projectCollection { get; } = new();

	public bool scrollVisible { get; set; }

	public ProjectWindow()
	{
		InitializeComponent();
		Utils.Init();
		DataContext = this;
		scrollVisible = false;
		projectLV.ItemsSource = projectCollection;

		foreach (var pi in ProjectFolderScanner.GetProjects())
			projectCollection.Add(pi);

		editorVersion.Text = "v." + Utils.appVersion;
		formatVersion.Text = "Scenario Format Version: v." + Utils.formatVersion;
	}

	void Notify(string name) => _inpc?.Invoke(this, new PropertyChangedEventArgs(name));

	void ProjectList_PointerEntered(object? sender, PointerEventArgs e)
	{
		scrollVisible = true;
		Notify(nameof(scrollVisible));
	}

	void ProjectList_PointerExited(object? sender, PointerEventArgs e)
	{
		scrollVisible = false;
		Notify(nameof(scrollVisible));
	}

	void CancelButton_Click(object? sender, RoutedEventArgs e)
	{
		if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
			d.Shutdown();
	}

	void ScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (sender is not ScrollViewer scv)
			return;
		var delta = e.Delta.Y;
		scv.Offset = new Vector(scv.Offset.X, scv.Offset.Y - delta * 20);
		e.Handled = true;
	}

	void ScenarioButton_Click(object? sender, RoutedEventArgs e)
	{
		// TODO-Avalonia-Reenable: new MainWindow(Guid.Empty)
	}

	void RandomScenarioButton_Click(object? sender, RoutedEventArgs e)
	{
		// TODO-Avalonia-Reenable: ProceduralGeneratorWindow
	}

	void CampaignButton_Click(object? sender, RoutedEventArgs e)
	{
		// TODO-Avalonia-Reenable: CampaignWindow
	}

	void ProjectLV_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		// TODO-Avalonia-Reenable: open MainWindow / CampaignWindow from FileManager loads
		if (projectLV.SelectedItem is ProjectItem item)
			System.Diagnostics.Debug.WriteLine($"Selected: {item.Title} ({item.projectType})");
	}

	void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			BeginMoveDrag(e);
	}
}
