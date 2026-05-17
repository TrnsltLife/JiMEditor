using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;
using System.Reflection;
using FlowGraph.Avalonia;
using FlowGraph.Avalonia.Rendering.NodeRenderers;
using FlowGraph.Core;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>Simple geometric renderer for chapter/tile nodes (hex or square).</summary>
public sealed class PolygonNodeRenderer : INodeRenderer
{
	readonly bool _hexagon;
	readonly bool _forceSquare;
	readonly IBrush _fill;
	readonly IBrush _stroke;
	readonly IBrush _textBrush;
	readonly IBrush _selectedStroke = new SolidColorBrush( Color.Parse( "#FFD86B" ) );

	public PolygonNodeRenderer( bool hexagon, bool forceSquare, string fillHex, string strokeHex )
	{
		_hexagon = hexagon;
		_forceSquare = forceSquare;
		_fill = new SolidColorBrush( Color.Parse( fillHex ) );
		_stroke = new SolidColorBrush( Color.Parse( strokeHex ) );
		_textBrush = Brushes.White;
	}

	public Control CreateNodeVisual( Node node, NodeRenderContext context )
	{
		var width = node.Width ?? context.Settings.NodeWidth;
		var height = node.Height ?? context.Settings.NodeHeight;
		if ( _forceSquare )
			height = width;
		var scale = ReadContextScale( context );
		width *= scale;
		height *= scale;

		var root = new Grid
		{
			Width = width,
			Height = height,
			DataContext = node,
		};

		Shape shape = _hexagon
			? new Polygon
			{
				Points =
				[
					new global::Avalonia.Point( width * 0.25, 0 ),
					new global::Avalonia.Point( width * 0.75, 0 ),
					new global::Avalonia.Point( width, height * 0.5 ),
					new global::Avalonia.Point( width * 0.75, height ),
					new global::Avalonia.Point( width * 0.25, height ),
					new global::Avalonia.Point( 0, height * 0.5 ),
				],
			}
			: new Rectangle
			{
				RadiusX = 2,
				RadiusY = 2,
			};

		shape.Fill = _fill;
		shape.Stroke = _stroke;
		shape.StrokeThickness = 1.5;
		shape.Tag = "shape";
		shape.IsHitTestVisible = false;

		var text = new TextBlock
		{
			Text = node.Label ?? node.Type,
			Foreground = _textBrush,
			FontSize = 12 * scale,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			TextAlignment = TextAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
			Margin = new Thickness( 10 * scale, 6 * scale ),
			IsHitTestVisible = false,
			Tag = "label",
		};

		root.Children.Add( shape );
		root.Children.Add( text );
		return root;
	}

	public void UpdateSelection( Control visual, Node node, NodeRenderContext context )
	{
		if ( visual is not Grid g )
			return;
		if ( g.Children.Count == 0 || g.Children[0] is not Shape shape )
			return;
		shape.Stroke = node.IsSelected ? _selectedStroke : _stroke;
		shape.StrokeThickness = node.IsSelected ? 2.5 : 1.5;
	}

	public void UpdateSize( Control visual, Node node, NodeRenderContext context, double width, double height )
	{
		if ( _forceSquare )
			height = width;
		var scale = ReadContextScale( context );
		width *= scale;
		height *= scale;
		visual.Width = width;
		visual.Height = height;
		if ( visual is not Grid g )
			return;
		if ( g.Children.Count == 0 || g.Children[0] is not Shape shape )
			return;
		if ( shape is not Polygon poly )
			return;

		poly.Points =
		[
			new global::Avalonia.Point( width * 0.25, 0 ),
			new global::Avalonia.Point( width * 0.75, 0 ),
			new global::Avalonia.Point( width, height * 0.5 ),
			new global::Avalonia.Point( width * 0.75, height ),
			new global::Avalonia.Point( width * 0.25, height ),
			new global::Avalonia.Point( 0, height * 0.5 ),
		];

		if ( g.Children.Count > 1 && g.Children[1] is TextBlock text )
		{
			text.FontSize = 12 * scale;
			text.Margin = new Thickness( 10 * scale, 6 * scale );
		}
	}

	public double? GetWidth( Node node, FlowCanvasSettings settings ) => _forceSquare ? 112 : null;
	public double? GetHeight( Node node, FlowCanvasSettings settings ) => _forceSquare ? 112 : null;
	public double? GetMinWidth( Node node, FlowCanvasSettings settings ) => _forceSquare ? 88 : null;
	public double? GetMinHeight( Node node, FlowCanvasSettings settings ) => _forceSquare ? 88 : null;

	static double ReadContextScale( NodeRenderContext context )
	{
		// FlowGraph doesn't document NodeRenderContext members in XML; probe likely scale/zoom names safely.
		var t = context.GetType();
		foreach ( var name in new[] { "Zoom", "Scale", "CanvasScale", "ViewScale" } )
		{
			var p = t.GetProperty( name, BindingFlags.Instance | BindingFlags.Public );
			if ( p?.PropertyType == typeof( double ) && p.GetValue( context ) is double d && d > 0 )
				return d;
		}
		return 1.0;
	}
}
