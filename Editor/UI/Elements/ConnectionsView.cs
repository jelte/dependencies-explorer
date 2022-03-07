using System;
using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class ConnectionsView : VisualElement
    {
        public event Action<IEnumerable<object>> onItemsChosen;


        private Label m_bundleName;
        private ListView m_list;

        private List<int> _empty = new List< int >();

        public new class UxmlFactory : UxmlFactory< ConnectionsView, UxmlTraits > { }

        public ConnectionsView()
        {
            RegisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
        }

        protected virtual void OnPostDisplaySetup(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
            m_bundleName = this.Q<Label>();

            m_list = this.Q<ListView>();
            m_list.makeItem = MakeRow;
            m_list.bindItem = BindRow;
            m_list.onSelectionChange += value => onItemsChosen?.Invoke(value);
        }

        private void BindRow(VisualElement element, int index)
        {
            ((Label) element).text = ((KeyValuePair<Bundle,Dictionary<string, List<string>>>) m_list.itemsSource[index]).Key.Name;
        }

        private VisualElement MakeRow()
        {
            return new Label();
        }

        public void OnSelectionChange(Bundle bundle, Direction direction)
        {
            m_bundleName.text = bundle.Name;

            m_list.Clear();
            var bundles = direction == Direction.Output ? bundle.Out : bundle.In;
            if (bundles == null)
                m_list.itemsSource = _empty;
            else
                m_list.itemsSource = bundles?.OrderBy( pair => pair.Key.Name )?.ToArray();

#if UNITY_2022_1_OR_NEWER
            m_list.Rebuild();
#else
            m_list.Refresh();
#endif
        }

    }
}