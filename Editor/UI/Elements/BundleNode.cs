using System;
using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundleNode : GraphElement, ICollectibleElement
    {
        /// <summary>
        ///   <para>Title bar container.</para>
        /// </summary>
        public VisualElement titleContainer { get; private set; }

        /// <summary>
        ///   <para>Input container used for input ports.</para>
        /// </summary>
        public VisualElement inputContainer { get; private set; }

        /// <summary>
        ///   <para>Outputs container, used for output ports.</para>
        /// </summary>
        public VisualElement outputContainer { get; private set; }

        private readonly Label m_TitleLabel;

        /// <summary>
        ///   <para>Node's title element.</para>
        /// </summary>
        public override string title
        {
            get => this.m_TitleLabel != null ? this.m_TitleLabel.text : string.Empty;
            set
            {
                if (this.m_TitleLabel == null)
                    return;
                this.m_TitleLabel.text = value;
            }
        }

        public Bundle Bundle;

        public Port In;
        public Port Out;

        /// <summary>
        ///   <para>Node's constructor.</para>
        /// </summary>
        /// <param name="nodeOrientation">The orientation.</param>
        public BundleNode( string name )
        {
            var pill = new Pill();
            Add(pill);
            pill.text = name;

            In = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            In.portName = string.Empty;
            In.portColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.2f);
            In.SetEnabled(false);
            Out = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            Out.portName = string.Empty;
            Out.portColor = Color.red;
            Out.SetEnabled(false);
            pill.left = In;
            pill.right = Out;
        }

        public virtual Port InstantiatePort(
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
        {
            return Port.Create<Edge>(orientation, direction, capacity, type);
        }

        public override Rect GetPosition()
        {
            if (resolvedStyle.position != Position.Absolute)
                return this.layout;
            double left = (double) this.resolvedStyle.left;
            double top = (double) this.resolvedStyle.top;
            Rect layout = this.layout;
            double width = (double) layout.width;
            layout = this.layout;
            double height = (double) layout.height;
            return new Rect((float) left, (float) top, (float) width, (float) height);
        }

        /// <summary>
        ///   <para>Set node position.</para>
        /// </summary>
        /// <param name="newPos">New position.</param>
        public override void SetPosition(Rect newPos)
        {
            style.position = Position.Absolute;
            style.left = newPos.x;
            style.top = newPos.y;
        }

        private void CollectConnectedEdges(HashSet<GraphElement> edgeSet)
        {
            edgeSet.UnionWith(this.inputContainer.Children().OfType<Port>().SelectMany<Port, Edge>((Func<Port, IEnumerable<Edge>>) (c => c.connections)).Where<Edge>((Func<Edge, bool>) (d => (uint) (d.capabilities & Capabilities.Deletable) > 0U)).Cast<GraphElement>());
            edgeSet.UnionWith(this.outputContainer.Children().OfType<Port>().SelectMany<Port, Edge>((Func<Port, IEnumerable<Edge>>) (c => c.connections)).Where<Edge>((Func<Edge, bool>) (d => (uint) (d.capabilities & Capabilities.Deletable) > 0U)).Cast<GraphElement>());
        }

        public virtual void CollectElements(
            HashSet<GraphElement> collectedElementSet,
            Func<GraphElement, bool> conditionFunc)
        {
            this.CollectConnectedEdges(collectedElementSet);
        }
    }

}