using System.IO;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using DependenciesExplorer.Editor.UI.Elements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor
{
    public class DependenciesExplorerWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private Reader _reader;
        private BundleListView _bundleListView;
        private BundleGraphView _graphView;
        private InspectorView _inspector;
        private ToolbarSearchField _search;
        private ToolbarButton _btnOpen;
        private SplitView _splitList;
        private SplitView _splitInspector;
        private ToolbarToggle _tglBidirectional;

        [MenuItem("Window/Asset Management/Addressables/Dependencies Explorer")]
        public static void ShowExample()
        {
            var wnd = GetWindow<DependenciesExplorerWindow>();
            wnd.titleContent = new GUIContent("Dependencies Explorer");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            root.Add(m_VisualTreeAsset.Instantiate());

            _splitList = root.Q<SplitView>("bundleListSplit");
            _splitInspector = root.Q<SplitView>("inspectorSplit");
            _bundleListView = root.Q<BundleListView>();
            _graphView = root.Q<BundleGraphView>();
            _inspector = root.Q<InspectorView>();
            _search = root.Q<ToolbarSearchField>();
            _btnOpen = root.Q<ToolbarButton>("open");
            _tglBidirectional = root.Q<ToolbarToggle>("bidirectionalDependencies");

            _bundleListView.onSelectionChange += selection =>
            {
                _splitInspector.CollapseChild(_splitInspector.fixedPaneIndex);
                _graphView.OnSelectionChange(selection);
            };
            _graphView.onSelectionChange += (n) =>
            {
                _splitInspector.UnCollapse();
                _inspector.OnSelectionChange(n);
            };

            _btnOpen.clicked += OnBtnOpenClicked;

            BundleNodeView.AssignedColors.Clear();
            _search.RegisterValueChangedCallback(OnSearchTextChanged);

            _tglBidirectional.RegisterValueChangedCallback(OnBidirectionalChanged);
            Repaint();
        }

        private void OnBidirectionalChanged(ChangeEvent<bool> evt)
        {
            _graphView.bidirectionalDependecies = evt.newValue;
            if ( _bundleListView.selectedItems.Count() > 0 )
				_graphView.OnSelectionChange(_bundleListView.selectedItems);
            else
				_graphView.Reset(_reader);
        }

        private void OnBtnOpenClicked()
        {
            var path = EditorUtility.OpenFilePanel("Open",  Path.Combine( Application.dataPath, "..", "Library", "com.unity.addressables" ), "txt");

            _reader ??= new Reader();
            _reader.ReadFile( path );

            _bundleListView.itemsSource = _reader.Bundles.Values.OrderBy( bundle => bundle.Name ).ToArray();
            _bundleListView.Refresh();
            _graphView.Reset( _reader );
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            if (_reader == null) return;
            var value = evt.newValue;

            _splitList.UnCollapse();
            _bundleListView.ClearSelection();
            _bundleListView.itemsSource = _reader.Bundles.Values.Where( v => v.Name.Contains(value) ).ToArray();
            _bundleListView.Refresh();
        }
    }

}