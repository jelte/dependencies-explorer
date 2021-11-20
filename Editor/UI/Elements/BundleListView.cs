using DependenciesExplorer.Editor.Data;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class BundleListView : ListView
    {
        private class Element : VisualElement
        {
            private Label _label;

            public Element()
            {
                _label = new Label();
                Add(_label);
            }

            public void BindData(Bundle bundle)
            {
                _label.text = bundle.Name;
            }
        }

        public new class UxmlFactory : UxmlFactory<BundleListView, UxmlTraits>
        {
        }

        public BundleListView() : base()
        {
            makeItem = () => new Element();;
            bindItem = BindItem;
        }

        private void BindItem(VisualElement element, int index)
        {
            (element as Element)?.BindData((Bundle) itemsSource[index]);
        }
    }
}