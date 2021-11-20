using System;
using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundleGraphView : GraphView
    {
        public event Action<Bundle> onSelectionChange;

        private Dictionary< string, BundleNodeView > _nodes = new Dictionary<string, BundleNodeView>();
        private IEnumerable<object> _current;
        private Bundle _selectedBundle;

        private MiniMap _miniMap;

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

            _miniMap = new MiniMap {graphView = this};
            _miniMap.anchored = true;
            _miniMap.style.width = 200;
            _miniMap.style.height = 175;
            _miniMap.style.right = 10;
            _miniMap.style.bottom = 10;

            Add(_miniMap);
        }

        public void OnSelectionChange(IEnumerable<object> selection)
        {
            DeleteElements(graphElements);
            _nodes.Clear();

            _current = selection;
            foreach (var item in _current)
            {
                if (!(item is Bundle bundle))
                    continue;

                AddDependencies(AddBundle(bundle, new Vector2(30, contentRect.center.y)));
            }
        }

        private void AddDependencies(BundleNodeView node)
        {
            var bundle = node.Bundle;
            var outCount = bundle.Out.Count( b => !_nodes.ContainsKey(b.Key.Name) );
            var start = node.Position + new Vector2( 100, outCount < 2 ? 0 : (-50 * (outCount / 2f))) ;
            var i = 0;

            var children = new List<BundleNodeView>();

            foreach (var a in bundle.Out.Where( b => !_nodes.ContainsKey(b.Key.Name) ))
            {
                var child = AddBundle(a.Key, start + Vector2.up * (i * 50));
                if ( !bidirectionalDependecies ) AddElement(node.Out.ConnectTo(child.In));
                children.Add(child);
                i++;
            }

            foreach (var a in bundle.Out)
            {
                if ( !_nodes.TryGetValue(a.Key.Name, out var outNode) ) continue;
                if ( bidirectionalDependecies ) AddElement(node.Out.ConnectTo(outNode.In));
            }

            foreach (var child in children)
            {
                AddDependencies(child);
            }
            node.RefreshPorts();
        }
        private BundleNodeView AddBundle( Bundle bundle, Vector2 position )
        {
            if (_nodes.TryGetValue(bundle.Name, out var node)) return node;

            node = new BundleNodeView(bundle, OnNodeSelectionChange) {Position = position};
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
                edge.RemoveFromClassList("hide");
                node.RemoveFromClassList("hide");
            }
            else
            {
                edge.AddToClassList("hide");
                node.AddToClassList("hide");
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
    }
}