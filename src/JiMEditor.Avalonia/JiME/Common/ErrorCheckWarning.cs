using System;
using System.Collections.Generic;
using System.Linq;

namespace JiME
{
	public enum ErrorCheckTargetKind
	{
		Event,
		Trigger,
		Objective,
		Resolution,
		Threat,
		TileBlock,
		Tile,
		Token,
		Settings,
		TileEditor,
		Activation
	}

	public enum ErrorCheckSection
	{
		General,
		DisabledCollections
	}

	public sealed class ErrorCheckNav
	{
		public ErrorCheckNav( ErrorCheckTargetKind kind, string? name = null, Guid? id = null )
		{
			Kind = kind;
			Name = name;
			Id = id;
		}

		public ErrorCheckTargetKind Kind { get; }
		public string? Name { get; }
		public Guid? Id { get; }
	}

	public sealed class ErrorCheckSpan
	{
		public ErrorCheckSpan( string text, ErrorCheckNav? nav = null )
		{
			Text = text;
			Nav = nav;
		}

		public string Text { get; }
		public ErrorCheckNav? Nav { get; }
	}

	public sealed class ErrorCheckWarning
	{
		public ErrorCheckWarning( params ErrorCheckSpan[] spans )
			: this( ErrorCheckSection.General, spans )
		{
		}

		public ErrorCheckWarning( ErrorCheckSection section, params ErrorCheckSpan[] spans )
		{
			Section = section;
			Spans = spans;
		}

		public ErrorCheckSection Section { get; }
		public IReadOnlyList<ErrorCheckSpan> Spans { get; }

		public string Text => string.Concat( Spans.Select( s => s.Text ) );
	}
}
