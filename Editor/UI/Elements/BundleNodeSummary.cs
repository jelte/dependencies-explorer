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
    public class BundleNodeSummary : Node, IBundleNode
    {
        private Action<BundleNodeSummary> _onSelected;
        
        public Bundle Bundle;

        public Port In;
        public Port Out;

        public string Path;
        public bool Open;

        public Vector2 Position
        {
            get => new Vector2( style.left.value.value, style.top.value.value);
            set
            {
                style.left = value.x;
                style.top = value.y;
            }
        }
        Port IBundleNode.Out => Out;
        Bundle IBundleNode.Bundle => Bundle;

        string IBundleNode.Path
        {
            get => Path;
            set => Path = value;
        }

        /// <summary>
        ///   <para>Node's constructor.</para>
        /// </summary>
        /// <param name="nodeOrientation">The orientation.</param>
        public BundleNodeSummary( Bundle bundle, Action<BundleNodeSummary> onSelected )
        {
            Bundle = bundle;
            _onSelected = onSelected;
            
            In = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            In.portName = string.Empty;
            In.portColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.2f);
            In.SetEnabled(false);
            In.AddToClassList("hidden");
            Out = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            Out.portName = string.Empty;
            Out.portColor = Color.red;
            Out.SetEnabled(false);
            Out.AddToClassList("hidden");
            
            for (  var i = childCount - 1; i >= 0; i-- )
                RemoveAt(i);
            var pill = new Pill(In, Out);
            pill.left = In;
            pill.right = Out;
            pill.Q<Label>("title-label").text = $"{Bundle.Dependencies.Length} / {Bundle.ExpandedDependencies.Length}";
            Add(pill);
            RefreshPorts();
        }
        
        public override void OnSelected()
        {
            base.OnSelected();
            _onSelected?.Invoke(this);
        }
    }

}