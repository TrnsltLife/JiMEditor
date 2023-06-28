using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JiME
{
	/// <summary>
	/// Describes pages in Scenario introText and resolutionObserver
	/// </summary>
	public class TextBookData : Translatable, INotifyPropertyChanged, ICommonData
	{
		override public string TranslationKeyName() { return dataName; }
		override public string PreviousTranslationKeyName() { return dataName; }

		override protected void DefineTranslationAccessors()
		{
			List<TranslationAccessor> list = new List<TranslationAccessor>()
			{
				new TranslationAccessor("resolution.{0}.text", () => this.pages[0])
			};
			translationAccessors = list;
		}

		string _dataName, _triggerName;

		public string dataName
		{
			get { return _dataName; }
			set
			{
				if ( value != _dataName )
				{
					_dataName = value;
					Prop( "dataName" );
				}
			}
		}//description in editor
		public List<string> pages { get; set; }
		public string triggerName
		{
			get { return _triggerName; }
			set
			{
				if ( value != _triggerName )
				{
					_triggerName = value;
					Prop( "triggerName" );
				}
			}
		}
		public Guid GUID { get; set; }
		public bool isEmpty { get; set; }

		public TextBookData( string sname = "Default Text" )
		{
			dataName = sname;
			pages = new List<string>();
			triggerName = "None";
			GUID = Guid.NewGuid();
			isEmpty = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void Prop( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public TextBookData Clone()
        {
			TextBookData book = new TextBookData();
			book.dataName = "Copy of " + this.dataName;
			book.triggerName = this.triggerName;
			book.GUID = Guid.NewGuid();
			book.isEmpty = this.isEmpty;
			book.pages = new List<string>();
			foreach(var page in this.pages)
            {
				book.pages.Add(page);
            }
			return book;
        }
	}

	public class TextBookController
	{
		public List<string> pages;

		public int index { get; private set; }
		public int pageCount { get { return pages.Count; } private set { } }

		public TextBookController()
		{
			pages = new List<string>();
			index = 0;
		}

		/// <summary>
		/// Set text of current page
		/// </summary>
		public void SetContent( string text )
		{
			pages[index] = text;
		}

		/// <summary>
		/// Import pages and set page controller text to page 1
		/// </summary>
		public void ImportPages( List<string> import )
		{
			index = 0;
			if ( import.Count > 0 )
			{
				pages = import;
				SetContent( pages[0] );
			}
		}

		/// <summary>
		/// Get a page's text
		/// </summary>
		public string GetPage( int idx )
		{
			if ( idx < pages.Count )
				return pages[idx];
			else
				return string.Empty;
		}

		/// <summary>
		/// Add new page and set current page index to it
		/// </summary>
		public void AddPage()
		{
			pages.Add( "" );
			index = pages.Count - 1;
		}

		/// <summary>
		/// Removes the current page, returns next page text
		/// </summary>
		public string RemovePage()
		{
			if ( pages.Count > 1 )
			{
				pages.RemoveAt( index );
				index = Math.Min( index, pages.Count - 1 );
			}
			return pages[index];
		}

		/// <summary>
		/// Increases page counter and returns current page text
		/// </summary>
		public string Next()
		{
			index = Math.Min( pages.Count - 1, index + 1 );
			return pages[index];
		}

		public string Previous()
		{
			index = Math.Max( 0, index - 1 );
			return pages[index];
		}
	}
}
