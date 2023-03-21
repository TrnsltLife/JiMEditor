﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphX.Controls;
using JiME.Visualization.Models;

namespace JiME.Visualization.Views
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

            // TODO: Not sure if these extra labels could be used? need to be tested
            //VertexLabelFactory = new GraphVertexLabelFactory();
            //EdgeLabelFactory = new GraphEdgeLabelFactory();
        }

    }
}