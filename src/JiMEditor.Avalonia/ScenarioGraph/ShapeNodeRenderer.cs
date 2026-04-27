using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using FlowGraph.Avalonia.Rendering.NodeRenderers;
using FlowGraph.Core;

namespace JiMEditor.Avalonia.ScenarioGraph;

/// <summary>
/// Custom shape renderer built on FlowGraph's default renderer pipeline, preserving built-in hit-testing/selection behavior.
/// </summary>
public sealed class ShapeNodeRenderer : DefaultNodeRenderer
{
	readonly NodeShape _shape;
	readonly IBrush _fill;
	readonly IBrush _stroke;
	readonly IBrush _textBrush = Brushes.White;
	readonly double _fontSize;
	readonly FontWeight _fontWeight;
	readonly double _defaultWidth;
	readonly double _defaultHeight;
	readonly double _minWidth;
	readonly double _minHeight;

	public ShapeNodeRenderer(
		NodeShape shape,
		string fillHex,
		string strokeHex,
		double fontSize = 12,
		FontWeight? fontWeight = null,
		double? defaultWidth = null,
		double? defaultHeight = null,
		double? minWidth = null,
		double? minHeight = null )
	{
		_shape = shape;
		_fill = new SolidColorBrush( Color.Parse( fillHex ) );
		_stroke = new SolidColorBrush( Color.Parse( strokeHex ) );
		_fontSize = fontSize;
		_fontWeight = fontWeight ?? FontWeight.Normal;

		// Shape-specific defaults:
		// - Regular flat-top hex ratio: width / height = 2 / sqrt(3) ≈ 1.1547
		// - Square/circle/diamond are square bounds.
		if ( _shape == NodeShape.Hexagon )
		{
			_defaultHeight = defaultHeight ?? 104;
			_defaultWidth = defaultWidth ?? ( _defaultHeight * ( 2d / System.Math.Sqrt( 3d ) ) );
			_minHeight = minHeight ?? 78;
			_minWidth = minWidth ?? ( _minHeight * ( 2d / System.Math.Sqrt( 3d ) ) );
		}
		else if ( _shape == NodeShape.TriggerArrow )
		{
			_defaultWidth = defaultWidth ?? 132;
			_defaultHeight = defaultHeight ?? 84;
			_minWidth = minWidth ?? 96;
			_minHeight = minHeight ?? 62;
		}
		else if ( _shape == NodeShape.Rectangle )
		{
			_defaultWidth = defaultWidth ?? 132;
			_defaultHeight = defaultHeight ?? 84;
			_minWidth = minWidth ?? 96;
			_minHeight = minHeight ?? 62;
		}
		else
		{
			_defaultWidth = defaultWidth ?? 96;
			_defaultHeight = defaultHeight ?? _defaultWidth;
			_minWidth = minWidth ?? 72;
			_minHeight = minHeight ?? _minWidth;
		}
	}

	protected override Control CreateNodeContent( Node node, NodeRenderContext context )
	{
		var nodeWidth = node.Width ?? _defaultWidth;
		var nodeHeight = node.Height ?? _defaultHeight;
		var (shapeWidth, shapeHeight) = ComputeShapeBounds( nodeWidth, nodeHeight );

		// Shapes/text are non-hit-testable so pointer reaches a full-node target; FlowGraph's chrome still handles drag.
		var underlay = new Border
		{
			Background = Brushes.Transparent,
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
			IsHitTestVisible = true,
		};

		var surface = new Grid
		{
			Width = shapeWidth,
			Height = shapeHeight,
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
		};

		var shape = CreateShape( shapeWidth, shapeHeight );

		shape.Fill = _fill;
		shape.Stroke = _stroke;
		shape.StrokeThickness = 1.5;
		shape.IsHitTestVisible = false;

		var text = new TextBlock
		{
			Text = node.Label ?? node.Type,
			Foreground = _textBrush,
			FontSize = _fontSize,
			FontWeight = _fontWeight,
			HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
			VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
			TextAlignment = TextAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
			Margin = new Thickness( 10, 6 ),
			IsHitTestVisible = false,
		};

		surface.Children.Add( shape );
		surface.Children.Add( text );

		var root = new Grid
		{
			Width = nodeWidth,
			Height = nodeHeight,
			ClipToBounds = true,
		};
		root.Children.Add( underlay );
		root.Children.Add( surface );
		return root;
	}

	(double Width, double Height) ComputeShapeBounds( double nodeWidth, double nodeHeight )
	{
		if ( _shape == NodeShape.TriggerArrow )
			return (nodeWidth, nodeHeight);
		if ( _shape == NodeShape.Rectangle )
			return (nodeWidth, nodeHeight);

		// Keep regular flat-top hex ratio while fitting inside the node box.
		if ( _shape == NodeShape.Hexagon )
		{
			const double hexRatio = 2d / 1.7320508075688772d; // 2 / sqrt(3)
			var wByHeight = nodeHeight * hexRatio;
			var width = System.Math.Min( nodeWidth, wByHeight );
			var height = width / hexRatio;
			return (width, height);
		}

		// Square/circle/diamond render in a square that must fit inside both axes.
		var side = System.Math.Min( nodeWidth, nodeHeight );
		return (side, side);
	}

	public override double? GetWidth( Node node, FlowGraph.Avalonia.FlowCanvasSettings settings ) => _defaultWidth;
	public override double? GetHeight( Node node, FlowGraph.Avalonia.FlowCanvasSettings settings ) => _defaultHeight;
	public override double? GetMinWidth( Node node, FlowGraph.Avalonia.FlowCanvasSettings settings ) => _minWidth;
	public override double? GetMinHeight( Node node, FlowGraph.Avalonia.FlowCanvasSettings settings ) => _minHeight;

	Shape CreateShape( double width, double height ) =>
		_shape switch
		{
			NodeShape.Hexagon => new Polygon
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
			},
			NodeShape.Circle => new Ellipse(),
			NodeShape.Diamond => new Polygon
			{
				Points =
				[
					new global::Avalonia.Point( width * 0.5, 0 ),
					new global::Avalonia.Point( width, height * 0.5 ),
					new global::Avalonia.Point( width * 0.5, height ),
					new global::Avalonia.Point( 0, height * 0.5 ),
				],
			},
			NodeShape.TriggerArrow => new Polygon
			{
				Points =
				[
					new global::Avalonia.Point( 0, 0 ),
					new global::Avalonia.Point( width * 0.78, 0 ),
					new global::Avalonia.Point( width, height * 0.5 ),
					new global::Avalonia.Point( width * 0.78, height ),
					new global::Avalonia.Point( 0, height ),
				],
			},
			_ => new Rectangle(),
		};

	public enum NodeShape
	{
		Rectangle,
		Square,
		Hexagon,
		Circle,
		Diamond,
		TriggerArrow,
	}
}
