// Ported from: JiMEditor/Views/TranslationItemEditorWindow.xaml.cs (2026-04-06) — Avalonia; original text = read-only markup; translation = PseudoHtmlRichEditor.
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JiMEditor.Avalonia;
using JiME;
using JiME.TextFormat;

namespace JiMEditor.Avalonia.Views;

public partial class TranslationItemEditorWindow : Window
{
	string _originalText = "";

	public TranslationItem translationItem { get; }
	public TranslationItem defaultTranslation { get; }

	public TranslationItemEditorWindow()
	{
		InitializeComponent();
		translationItem = null!;
		defaultTranslation = null!;
	}

	public TranslationItemEditorWindow( TranslationItem translationItem, TranslationItem defaultTranslation )
		: this()
	{
		DataContext = this;
		this.translationItem = translationItem;
		this.defaultTranslation = defaultTranslation;
		_originalText = translationItem.text;

		Loaded += OnLoaded;
	}

	void OnLoaded( object? sender, RoutedEventArgs e )
	{
		Loaded -= OnLoaded;
		var original = defaultTranslation.text ?? "";
		OriginalMarkupTextBox.Text = original;
		OriginalPreviewTextBlock.PseudoHtml = original;
		TranslationRichEditor.Text = translationItem.text ?? "";
		ScenarioDirty.WatchInputs( this );
	}

	void OkButton_Click( object? sender, RoutedEventArgs e )
	{
		var normalized = JiMePseudoHtmlFormat.NormalizePseudoHtml( TranslationRichEditor.Text.Trim() );
		translationItem.text = normalized;

		if ( translationItem.text != _originalText )
		{
			translationItem.NotifyPropertyChanged( nameof( translationItem.text ) );
			translationItem.updatedWhileEditing = true;
		}

		Close();
	}
}
