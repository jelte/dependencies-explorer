using System;
using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public sealed class BundleNodeView : Node
    {
        private Action<Bundle> _onSelected;

        private static Color[] _colors =
        {
            Color.blue,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.yellow,
            new Color(241/255f, 196/255f, 15/255f),
            new Color(26/255f, 188/255f, 156/255f),
            new Color(52/255f, 152/255f, 219/255f),
            new Color(236/255f, 240/255f, 241/255f),
            new Color(231/255f, 76/255f, 60/255f),
            new Color(142/255f, 68/255f, 173/255f),
            new Color(253/255f, 167/255f, 223/255f),
            new Color(196/255f, 229/255f, 56/255f),
            new Color(131/255f, 52/255f, 113/255f),
            new Color(255/255f, 218/255f, 121/255f),
        };

        public static Dictionary<string, Color> AssignedColors = new Dictionary<string, Color>();

        public Bundle Bundle;

        public Port In;
        public Port Out;

        public Vector2 Position
        {
            get => new Vector2( style.left.value.value, style.top.value.value);
            set
            {
                style.left = value.x;
                style.top = value.y;
            }
        }

        public BundleNodeView(Bundle bundle, Vector2 position, Action<Bundle> onSelected)
        {
            title = Bundle.Name;
            tooltip = Bundle.Name;
            Bundle = bundle;
            Position = position;
            _onSelected = onSelected;

            if (!AssignedColors.TryGetValue(bundle.Category, out var color))
            {
                color = _colors[AssignedColors.Count % _colors.Length];
                AssignedColors.Add(bundle.Category, color);
            }
            //topContainer.style.backgroundColor = new StyleColor(color);
            elementTypeColor = color;

            //titleContainer.style.height = 0;
            //style.width = 75;

            In = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            In.AddToClassList("hidden");
            In.portName = string.Empty;
            In.portColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.2f);
            In.SetEnabled(false);
            Out = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            Out.AddToClassList("hidden");
            Out.portName = string.Empty;
            Out.portColor = Color.red;
            Out.SetEnabled(false);

            for (  var i = childCount - 1; i >= 0; i-- )
                RemoveAt(i);
            var pill = new Pill(In, Out);
            pill.Q<Label>("title-label").text = Bundle.Name;
            pill.style.color = new StyleColor(color);
            Add( pill );


            RefreshPorts();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            _onSelected?.Invoke(Bundle);
        }
    }
}