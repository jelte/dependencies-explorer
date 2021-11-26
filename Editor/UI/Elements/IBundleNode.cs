using DependenciesExplorer.Editor.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public interface IBundleNode
    {
        Bundle Bundle { get; }
        Vector2 Position { get; set; }
        Rect layout { get; }
        Port Out { get; }
        
        string Path { get; set; }
        bool RefreshPorts();
    }
}