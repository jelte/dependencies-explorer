using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundleGraphView : GraphView
    {
        public event Action<Bundle, Direction> onSelectionChange;

        private Dictionary< string, BundleNodeView > _nodes = new Dictionary<string, BundleNodeView>();
        private IEnumerable<object> _current;
        private Bundle _selectedBundle;

        private MiniMap _miniMap;
        private Reader _reader;

        public bool bidirectionalDependecies { get; set; }
        public Direction Direction { get; set; } = Direction.Output;

        private List<string> _path = new List<string>();
        private string _currentPath;

        public new class UxmlFactory : UxmlFactory<BundleGraphView, UxmlTraits>
        {
        }

        public BundleGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void OnSelectionChange(IEnumerable<object> selection)
        {
            DeleteElements(graphElements.ToList());
            _nodes.Clear();

            _current = selection;
	        var item = _current.FirstOrDefault();
	        if ( !( item is Bundle bundle ) ) return;
            var bundleNode = AddBundle( bundle, Vector2.zero );
	        AddDependencies( bundleNode );
            bundleNode.Position = new Vector2(30, contentRect.center.y);
            MarkDirtyRepaint();
            EditorCoroutineUtility.StartCoroutineOwnerless(RePosition(bundleNode));
            onSelectionChange?.Invoke( bundleNode.Bundle, Direction );
        }

        private IEnumerator RePosition(IBundleNode node)
        {
            yield return null;
            Reposition(node, node.Position, node.layout.width);
        }

        private void Reposition(IBundleNode node, Vector2 position, float width)
        {
            node.Position = position;
            var height = node.layout.height + 10;
            var outCount = node.Out.connections.Count();

            var maxWidth = 0f;
            if ( node.Out.connections.Count() > 0 )
                maxWidth = node.Out.connections.Max(edge => edge.input.node.layout.width);
            var start = position + new Vector2( width + 25f, outCount < 2 ? 0 : (-height * (outCount / 2f))) ;

            var i = 0;
            foreach (var connection in node.Out.connections)
            {
                var onode = connection.input.node as BundleNodeView;
                onode.Position = start + Vector2.up * (i++ * height);
                Reposition(onode, onode.Position, maxWidth);
            }
        }

        private void AddDependencies(BundleNodeView node )
        {
            var bundle = node.Bundle;

            var others = Direction == Direction.Output ? bundle.Out : bundle.In;
            foreach (var a in others )
            {
                var child = AddBundle(a.Key);
                AddElement(node.Out.ConnectTo(child.In));
            }

            node.Open = true;
        }

        private void RemoveNode(GraphElement element, bool inclElement = true )
        {
            if (element is BundleNodeView node)
            {
                var connections = node.Out.connections.ToArray();
                foreach (var connection in connections)
                {
                    RemoveNode(connection.input.node);
                    RemoveElement(connection);
                }
                node.Out.DisconnectAll();
            }

            if (!inclElement) return;
            RemoveElement(element);
        }
        
        private BundleNodeView AddBundle( Bundle bundle, Vector2 position = default)
        {
            var node = new BundleNodeView(bundle, position, Direction, OnNodeSelectionChange);
            
            if (!_nodes.ContainsKey(bundle.Name)) 
                _nodes.Add(bundle.Name, node);
            AddElement(node);

            return node;
        }

        private void OnNodeSelectionChange(BundleNodeView bundleNode)
        {
            RemoveNode(bundleNode, false);
            if (!bundleNode.Open)
            {
                var e = bundleNode.In.connections.ToArray();
                foreach (var connection in e)
                {
                    var siblingEdges = connection.output.connections.ToArray();
                    foreach (var siblingEdge in siblingEdges)
                    {
                        if (!(siblingEdge.input.node is BundleNodeView sibling)) continue;
                        RemoveNode(sibling, false);
                        sibling.Open = false;
                    }
                }
                AddDependencies(bundleNode);
                EditorCoroutineUtility.StartCoroutineOwnerless(RePosition(bundleNode));
            } 
            else 
                bundleNode.Open = false;

            onSelectionChange?.Invoke(bundleNode.Bundle, Direction);
        }

        private void ToggleNode(BundleNodeView node)
        {
            foreach (var edge in node.In.connections)
            {
                if (!(edge.output.node is BundleNodeView outputNode)) continue;

                foreach (var inEdge in outputNode.In.connections)
                    ToggleNode(inEdge.input.node as BundleNodeView);

                foreach (var outEdge in outputNode.Out.connections)
                    ToggleEdge(outEdge, outEdge == edge);
            }

        }

        private void ToggleEdge(Edge edge, bool show)
        {
            var node = edge.input.node;
            edge.SetEnabled(show);
            node.SetEnabled(show);
            if (show)
            {
                edge.RemoveFromClassList("hidden");
                node.RemoveFromClassList("hidden");
            }
            else
            {
                edge.AddToClassList("hidden");
                node.AddToClassList("hidden");
            }

            if (!(node is BundleNodeView bundleNodeView)) return;
            foreach (var outEdge in bundleNodeView.Out.connections)
            {
                ToggleEdge(outEdge, show);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
        }

        public void Reset( Reader reader )
        {
	        DeleteElements(graphElements.ToList());
	        _nodes.Clear();

	        _reader = reader;
	        var i = 0;
	        var radius = 1250f;
	        var count = _reader.Bundles.Count;
	        foreach ( var bundle in _reader.Bundles.Values )
	        {
		        var angle = ( (float) i / count ) * 360f;
		        var position = ( Vector2.one * radius ).Rotate( angle );
		        AddBundle( bundle, contentRect.center  + position );
		        i++;
	        }

            // TODO: Add connections
            /* if ( bidirectionalDependecies)
	            foreach ( var node in _nodes.Values )
                    AddDependencies( node );
                    */
        }

    }

    public static class Vector2Extension {

	    public static Vector2 Rotate(this Vector2 v, float degrees) {
		    float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
		    float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

		    float tx = v.x;
		    float ty = v.y;
		    v.x = (cos * tx) - (sin * ty);
		    v.y = (sin * tx) + (cos * ty);
		    return v;
	    }
    }

}