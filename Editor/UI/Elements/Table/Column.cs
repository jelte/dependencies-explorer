using System;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements.Table
{
    public class Column : VisualElement
    {
        public string Name;
        public Func<VisualElement> makeItem;
        public Action<VisualElement, int> bindItem;
        
        public new class UxmlFactory : UxmlFactory<Column, UxmlTraits>
        {
        }
    }
}