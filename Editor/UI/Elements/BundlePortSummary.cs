using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundlePortSummary : Port
    {
        private Action<BundlePortSummary> _onSelected;
        
        public Bundle Bundle;

        public static Port Create<TEdge>(
            Bundle bundle,
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
            where TEdge : Edge, new()
        {
            var ele = new BundlePortSummary(bundle, orientation, direction, capacity, type)
            {
                m_EdgeConnector = (EdgeConnector) new EdgeConnector<TEdge>(new DefaultEdgeConnectorListener())
            };
            ele.AddManipulator((IManipulator) ele.m_EdgeConnector);
            return ele;
        }
        
        /// <summary>
        ///   <para>Node's constructor.</para>
        /// </summary>
        /// <param name="nodeOrientation">The orientation.</param>
        protected BundlePortSummary( Bundle bundle, Orientation portOrientation,
            Direction portDirection,
            Port.Capacity portCapacity,
            System.Type type ) : base(portOrientation, portDirection, portCapacity, type)
        {
            Bundle = bundle;
           
            for (  var i = childCount - 1; i >= 0; i-- )
                this.ElementAt(i).AddToClassList("hidden");

            if (bundle.ExpandedDependencies.Length == 0 && bundle.Dependencies.Length == 0) return;
            
            var label = new Label();
            label.text = $"{bundle.Dependencies.Length} / {bundle.ExpandedDependencies.Length}";
            Add(label);
        }
        
        public override void OnSelected()
        {
            base.OnSelected();
            _onSelected?.Invoke(this);
        }

        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;
      
            public DefaultEdgeConnectorListener()
            {
              this.m_EdgesToCreate = new List<Edge>();
              this.m_EdgesToDelete = new List<GraphElement>();
              this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
            }
      
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }
      
            public void OnDrop(UnityEditor.Experimental.GraphView.GraphView graphView, Edge edge)
            {
              this.m_EdgesToCreate.Clear();
              this.m_EdgesToCreate.Add(edge);
              this.m_EdgesToDelete.Clear();
              if (edge.input.capacity == Port.Capacity.Single)
              {
                foreach (Edge connection in edge.input.connections)
                {
                  if (connection != edge)
                    this.m_EdgesToDelete.Add((GraphElement) connection);
                }
              }
              if (edge.output.capacity == Port.Capacity.Single)
              {
                foreach (Edge connection in edge.output.connections)
                {
                  if (connection != edge)
                    this.m_EdgesToDelete.Add((GraphElement) connection);
                }
              }
              if (this.m_EdgesToDelete.Count > 0)
                graphView.DeleteElements((IEnumerable<GraphElement>) this.m_EdgesToDelete);
              List<Edge> edgesToCreate = this.m_EdgesToCreate;
              if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
              foreach (Edge edge1 in edgesToCreate)
              {
                graphView.AddElement((GraphElement) edge1);
                edge.input.Connect(edge1);
                edge.output.Connect(edge1);
              }
            }
        }
    }

}