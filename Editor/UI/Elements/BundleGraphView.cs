using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundleGraphView : GraphView
    {
        public event Action<Bundle> onSelectionChange;

        private Dictionary< string, BundleNodeView > _nodes = new Dictionary<string, BundleNodeView>();
        private IEnumerable<object> _current;
        private Bundle _selectedBundle;

        private MiniMap _miniMap;
        private Reader _reader;

        public bool bidirectionalDependecies { get; set; }

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

            _miniMap = new MiniMap {graphView = this, anchored = true};
            _miniMap.style.width = 200;
            _miniMap.style.height = 175;
            _miniMap.style.right = 10;
            _miniMap.style.bottom = 10;

            Add(_miniMap);
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
            MarkDirtyRepaint();
            EditorCoroutineUtility.StartCoroutineOwnerless(RePosition(bundleNode));
        }

        private IEnumerator RePosition(BundleNodeView node)
        {
            yield return null;
            Reposition(node, new Vector2(30, contentRect.center.y), node.layout.width);
        }

        private void Reposition(BundleNodeView node, Vector2 position, float width)
        {
            if (node.Position.magnitude > float.Epsilon) return;
            node.Position = position;
            var height = node.layout.height + 10;
            var outCount = node.Out.connections.Count();

            var maxWidth = 0f;
            if ( node.Out.connections.Count() > 0 )
                maxWidth = node.Out.connections.Max(edge => edge.input.node.layout.width);
            var start = position + new Vector2( width + 25f, outCount < 2 ? 0 : (-height * (outCount / 2f))) ;

            var i = 0;
            foreach (var connection in node.Out.connections)
                Reposition(connection.input.node as BundleNodeView , start + Vector2.up * (i++ * height), maxWidth);
        }

        private void AddDependencies(BundleNodeView node, bool allNodes = false)
        {
            var bundle = node.Bundle;

            var children = new List<BundleNodeView>();
            foreach (var a in bundle.Out.Where( b => !_nodes.ContainsKey(b.Key.Name) ))
            {
                var child = AddBundle(a.Key);
                if ( !bidirectionalDependecies ) AddElement(node.Out.ConnectTo(child.In));
                children.Add(child);
            }

            foreach (var a in bundle.Out)
            {
                if ( !_nodes.TryGetValue(a.Key.Name, out var outNode) ) continue;
                if ( bidirectionalDependecies ) AddElement(node.Out.ConnectTo(outNode.In));
            }

            foreach (var child in children)
                AddDependencies(child);
            node.RefreshPorts();
        }

        private BundleNodeView AddBundle( Bundle bundle, Vector2 position = default)
        {
            if (_nodes.TryGetValue(bundle.Name, out var node)) return node;

            node = new BundleNodeView(bundle, position, OnNodeSelectionChange);
            _nodes.Add(bundle.Name, node);
            AddElement(node);

            return node;
        }

        private void OnNodeSelectionChange(Bundle bundle)
        {
            if (bundle == _selectedBundle) return;
            _selectedBundle = bundle;

            if (bidirectionalDependecies)
            {
                onSelectionChange?.Invoke(bundle);
                return;
            }

            if (_nodes.TryGetValue(bundle.Name, out var node))
            {
                ToggleNode(node);

                foreach (var edge in node.Out.connections)
                {
                    ToggleEdge(edge, true);
                }
            }

            onSelectionChange?.Invoke(bundle);
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

            if ( bidirectionalDependecies)
	            foreach ( var node in _nodes.Values )
                    AddDependencies( node );
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