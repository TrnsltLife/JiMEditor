using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphX.Common.Enums;
using GraphX.Controls;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using JiME.Visualization;

namespace JiME.Views
{
    /// <summary>
    /// This is custom GraphArea representation using custom data types.
    /// GraphArea is the visual panel component responsible for drawing visuals (vertices and edges).
    /// It is also provides many global preferences and methods that makes GraphX so customizable and user-friendly.
    /// </summary>
    public class GraphArea : GraphArea<DataVertex, DataEdge, Graph>
    {
        public GraphArea()
        {
            ControlFactory = new GraphControlFactory(ControlFactory.FactoryRootArea);

            SetupLogic();

            // Not sure if these extra labels could be used? need to be tested
            VertexLabelFactory = new GraphVertexLabelFactory();
            EdgeLabelFactory = new GraphEdgeLabelFactory();
        }

        private void SetupLogic()
        {
            // SETUP GRAPHAREA
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new GraphLogic();
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree;
            //Now we can set parameters for selected algorithm using AlgorithmFactory property. This property provides methods for
            //creating all available algorithms and algo parameters.
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Tree);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            //((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;
            var layoutParams = (SimpleTreeLayoutParameters)logicCore.DefaultLayoutAlgorithmParams;
            layoutParams.LayerGap = 100;
            layoutParams.VertexGap = 200;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            //Default parameters are created automaticaly when new default algorithm is set and previous params were NULL
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            //Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            // Add some separation on parallel edges
            logicCore.EnableParallelEdges = true;
            logicCore.ParallelEdgeDistance = 20;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            //Finally assign logic core to GraphArea object
            LogicCore = logicCore;

            // Also adjust some GraphArea parameters

            //This method sets the dash style for edges. It is applied to all edges in Area.EdgesList. You can also set dash property for
            //each edge individually using EdgeControl.DashStyle property.
            //For ex.: Area.EdgesList[0].DashStyle = GraphX.EdgeDashStyle.Dash;
            SetEdgesDashStyle(EdgeDashStyle.Solid);

            //This method sets edges arrows visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowArrows = true;
            ShowAllEdgesArrows(true);

            //This method sets edges labels visibility. It is also applied to all edges in Area.EdgesList. You can also set property for
            //each edge individually using property, for ex: Area.EdgesList[0].ShowLabel = true;
            ShowAllEdgesLabels(true);
        }

        public void ShowGraph(Graph dataGraph)
        {
            // Just update the graph in the logic core
            LogicCore.Graph = dataGraph;
            GenerateGraph(true, true);
        }

        public System.Windows.Rect? FindGraphNodeRect(string dataName, IEnumerable<DataVertex.Type> preferredTypes)
        {
            // Try to find first vertex that matches the data name and is one of the preferred types if given
            var vertex = LogicCore.Graph.Vertices
                .Where(v => v.Text == dataName)
                .Where(v => preferredTypes == null || preferredTypes.Contains(v.VertexType))
                .FirstOrDefault();
            if (vertex != null)
            {
                // Single vertex found -> locate it in the graph
                if (this.VertexList.ContainsKey(vertex))
                {
                    var control = this.VertexList[vertex];
                    var top = System.Windows.Controls.Canvas.GetTop(control);
                    var left = System.Windows.Controls.Canvas.GetLeft(control);
                    var topLeftScreen = new System.Windows.Point(left, top);
                    var widthAndHeight = new System.Windows.Size(control.ActualWidth, control.ActualHeight);
                    return new System.Windows.Rect(topLeftScreen, widthAndHeight);
                }
            }

            // Not found
            return null;
        }
    }
}
