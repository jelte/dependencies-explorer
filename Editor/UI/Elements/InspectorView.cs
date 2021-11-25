using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class InspectorView : VisualElement
    {
        private Label _lblName;
        private ListView _lstDependenciesBundles;
        private ListView _lstDependenciesFiles;
        private ListView _lstFiles;
        private ListView _lstLinksTo;

        private List<int> _empty = new List< int >();

        public new class UxmlFactory : UxmlFactory< InspectorView, UxmlTraits > { }

        public InspectorView()
        {
        }


        private void BindDependencyFileItem(VisualElement element, int index)
        {
            (element as FileElement)?.BindData(((KeyValuePair<string, List<string>>) _lstDependenciesFiles.itemsSource[index]).Key);
        }

        private void BindFileItem(VisualElement element, int index)
        {
            (element as FileElement)?.BindData((string) _lstFiles.itemsSource[index]);
        }

        private void OnBundleSelectionChanged(IEnumerable<object> selection)
        {
            foreach (var selected in selection)
            {
                if (!(selected is KeyValuePair<Bundle, Dictionary<string,List<string>>> pair)) continue;

                _lstDependenciesFiles.itemsSource = pair.Value.Select( pair => pair ).ToArray();
                _lstFiles.itemsSource = _empty;
                _lstLinksTo.itemsSource = _empty;
                _lstDependenciesFiles.Refresh();
                return;
            }
        }

        private void OnDependencyFileSelectionChange(IEnumerable<object> selection)
        {
	        Selection.objects = selection.Select( value => AssetDatabase.LoadAssetAtPath< Object >( (( KeyValuePair<string, List<string>> )value).Key ) ).ToArray();
            foreach (var selected in selection)
            {
                if (!(selected is KeyValuePair<string, List<string>> pair)) continue;

                _lstLinksTo.itemsSource = _empty;
                _lstFiles.Clear();
                _lstFiles.itemsSource = pair.Value;
                _lstFiles.Refresh();


                var guid = AssetDatabase.AssetPathToGUID( pair.Key );
                Indexer.TryFind( guid, out var deps );

                _lstLinksTo.Clear();
                _lstLinksTo.itemsSource = deps;
                _lstLinksTo.Refresh();
                return;
            }
        }

        private void BindBundleItem(VisualElement element, int index)
        {
            (element as BundleElement)?.BindData(((KeyValuePair<Bundle,Dictionary<string, List<string>>>) _lstDependenciesBundles.itemsSource[index]).Key);
        }

        public void OnSelectionChange(Bundle bundle)
        {
            _lblName ??= this.Q<Label>("name");
            _lblName.text = bundle.Name;

            if (_lstDependenciesBundles == null)
            {
                _lstDependenciesBundles = this.Q<ListView>("depBundles");
                _lstDependenciesBundles.makeItem = () => new BundleElement();
                _lstDependenciesBundles.bindItem = BindBundleItem;
                _lstDependenciesBundles.onSelectionChange += OnBundleSelectionChanged;

                _lstDependenciesFiles = this.Q<ListView>("depFiles");
                _lstDependenciesFiles.makeItem = () => new FileElement();
                _lstDependenciesFiles.bindItem = BindDependencyFileItem;
                _lstDependenciesFiles.onSelectionChange += OnDependencyFileSelectionChange;

                _lstFiles = this.Q<ListView>("files");
                _lstFiles.makeItem = () => new FileElement();
                _lstFiles.bindItem = BindFileItem;
                _lstFiles.onSelectionChange += OnFileSelectionChange;

                _lstLinksTo = this.Q< ListView >( "linksTo" );
                _lstLinksTo.makeItem = () => new FileElement();
                _lstLinksTo.bindItem = BindLinkToItem;
            }

            _lstDependenciesBundles.Clear();
            _lstDependenciesBundles.itemsSource = bundle.Out.OrderBy( pair => pair.Key.Name ).ToArray();
            _lstDependenciesFiles.itemsSource = _empty;
            _lstFiles.itemsSource = _empty;
            _lstLinksTo.itemsSource = _empty;
            _lstDependenciesBundles.Refresh();
        }

        private void BindLinkToItem( VisualElement element, int index )
        {
	        (element as FileElement)?.BindData( AssetDatabase.GUIDToAssetPath( _lstLinksTo.itemsSource[ index ] as string ) );
        }

        private void OnFileSelectionChange( IEnumerable< object > selection )
        {
			Selection.objects = selection.Select( value => AssetDatabase.LoadAssetAtPath< Object >( ( string )value ) ).ToArray();
	        foreach (var selected in selection)
	        {
		        if (!(selected is string path)) continue;

		        var guid = AssetDatabase.AssetPathToGUID( path );
		        Indexer.TryFind( guid, out var deps );

		        _lstLinksTo.Clear();
		        _lstLinksTo.itemsSource = deps;
		        _lstLinksTo.Refresh();
		        return;
	        }
        }

        private class BundleElement : VisualElement
        {
            private Label _label;

            public BundleElement()
            {
                _label = new Label();
                Add(_label);
            }

            public void BindData(Bundle bundle)
            {
                _label.text = bundle.Name;
            }
        }

        private class FileElement : VisualElement
        {
            private Label _label;

            public FileElement()
            {
                _label = new Label();
                Add(_label);
            }

            public void BindData(string file)
            {
                _label.text = file;
            }
        }
    }
}