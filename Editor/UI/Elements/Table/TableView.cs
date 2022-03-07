using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DependenciesExplorer.Editor.UI.Elements.Table;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements.Table
{
    public class TableView : VisualElement
    {
        public event Action<IEnumerable<object>> onItemsChosen;

        private Toolbar m_header;
        private ListView m_list;
        private List<Column> m_columns = new List<Column>();
        private IList m_itemsSource;

        public IList itemSource
        {
            get => m_list.itemsSource;
            set
            {
                m_itemsSource = value;
                m_list.itemsSource = value;
            }
        }

        public new class UxmlFactory : UxmlFactory<TableView, UxmlTraits>
        {
        }

        public TableView() : base()
        {
            RegisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
        }

        protected virtual void OnPostDisplaySetup(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
            m_list = this.Q<ListView>();
            m_list.makeItem = MakeRow;
            m_list.bindItem = BindRow;
            m_list.selectionType = SelectionType.Multiple;
            m_list.onSelectionChange += value => onItemsChosen?.Invoke(value);

            m_header = this.Q<Toolbar>();
        }

        public void Rebuild()
        {
#if UNITY_2022_1_OR_NEWER
            m_list.Rebuild();
#else
	        m_list.Refresh();
#endif
        }

        private VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            foreach (var column in m_columns)
            {
                var element = column.makeItem.Invoke();
                element.style.width = m_header[row.childCount].layout.width;
                if (row.childCount > 0)
                {
                    element.style.borderLeftWidth = 1;
                    element.style.borderLeftColor = new StyleColor(new Color(55, 55, 55));
                }
                row.Add(element);
            }


            return row;
        }

        private void BindRow(VisualElement element, int index)
        {
            for (var i = 0; i < element.childCount; i++)
                m_columns[i].bindItem.Invoke(element.ElementAt(i), index);
        }

        public void AddColumn(string name, Func<VisualElement> makeItem, Action<VisualElement, object> bindItem)
        {
            var column = new Column()
            {
                name = name,
                makeItem = makeItem,
                bindItem = (element, index) => bindItem.Invoke(element, m_list.itemsSource[index])
            };
            m_columns.Add(column);
            m_header.Add(new Header(name));
        }

        public void Filter<T>(Func<T, bool> validation)
        {
            m_list.itemsSource = ((List<T>) m_itemsSource).Where(value => validation.Invoke(value)).ToList();
#if UNITY_2022_1_OR_NEWER
            m_list.Rebuild();
#else
            m_list.Refresh();
#endif
        }

        public class Header : VisualElement
        {
            public Header(string name)
            {
                style.paddingLeft = 3;
                style.paddingRight = 3;
                style.flexGrow = 1;
                style.alignContent = new StyleEnum<Align>(Align.Center );


                var label = new Label();
                label.text = name;
                label.style.flexGrow = 1;
                label.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>( FontStyle.Bold );
                Add(label);
            }
        }
    }
}