using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphX.Common.Models;

namespace JiME.Visualization.Models
{
    /* DataVertex is the data class for the vertices. It contains all custom vertex data specified by the user.
         * This class also must be derived from VertexBase that provides properties and methods mandatory for
         * correct GraphX operations.
         * Some of the useful VertexBase members are:
         *  - ID property that stores unique positive identfication number. Property must be filled by user.
         *  
         */

    public class DataVertex : VertexBase
    {
        public string Text { get; private set; }

        public Type VertexType { get; private set; }

        public object Source { get; private set; }

        #region Calculated or static props

        public override string ToString()
        {
            return Text;
        }

        #endregion

        /// <summary>
        /// Default parameterless constructor for this class
        /// (required for YAXLib serialization)
        /// </summary>
        public DataVertex() : this(string.Empty, Type.Start, null)
        {
        }

        public DataVertex(string text, Type type, object source)
        {
            Text = text;
            VertexType = type;
            Source = source;
        }

        public enum Type
        {
            Start,
            Trigger,
            Objective,
            Resolution,
            Interaction,
            InteractionGroup,

            /// <summary>
            /// TileSet
            /// </summary>
            Chapter,
            Tile,
            Token,
            ThreatLevel
        }
    }
}
