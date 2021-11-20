using System.Collections.Generic;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class InspectorView : VisualElement
    {
        private Label _lblName;
        private ListView _lstDependenciesBundles;
        private ListView _lstDependenciesFiles;
        private ListView _lstFiles;

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

                _lstFiles.Clear();
                _lstDependenciesFiles.itemsSource = pair.Value.Select( pair => pair ).ToArray();
                _lstFiles.itemsSource = null;
                _lstDependenciesFiles.Rebuild();
                _lstFiles.Rebuild();
                return;
            }
        }

        private void OnDependencyFileSelectionChange(IEnumerable<object> selection)
        {
            foreach (var selected in selection)
            {
                if (!(selected is KeyValuePair<string, List<string>> pair)) continue;

                _lstFiles.Clear();
                _lstFiles.itemsSource = pair.Value;
                _lstFiles.Rebuild();
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
            }

            _lstDependenciesBundles.Clear();
            _lstDependenciesFiles.Clear();
            _lstFiles.Clear();
            _lstDependenciesBundles.itemsSource = bundle.Out.OrderBy( pair => pair.Key.Name ).ToArray();
            _lstDependenciesFiles.itemsSource = null;
            _lstFiles.itemsSource = null;
            _lstDependenciesBundles.Rebuild();
            _lstDependenciesFiles.Rebuild();
            _lstFiles.Rebuild();
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