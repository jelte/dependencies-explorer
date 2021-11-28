using System.Collections.Generic;
using System.IO;
using System.Linq;
using DependenciesExplorer.Editor.Data;
using DependenciesExplorer.Editor.UI.Elements.Table;
using UnityEngine;
using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class FileTableView : TableView
    {
        public new class UxmlFactory : UxmlFactory< FileTableView, UxmlTraits > { }

        public FileTableView() : base()
        {
        }

        protected override void OnPostDisplaySetup(GeometryChangedEvent evt)
        {
            base.OnPostDisplaySetup(evt);
            AddColumn("Bundle", CreateLabelCell, ((element, o) => ((Label) element).text = ((BundleAsset) o).Bundle.Name ));
            AddColumn("File", CreateLabelCell, ((element, o) => ((Label) element).text = Path.GetFileName(((BundleAsset) o).Path )));
            AddColumn("References", CreateLabelCell, ((element, o) => ((Label) element).text = Path.GetFileName(((BundleAsset) o).Reference )));
        }

        private VisualElement CreateLabelCell()
        {
            var label = new Label();
            label.style.paddingLeft = 3;
            label.style.paddingRight = 3;
            label.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
            return label;
        }

        public void Filter(IEnumerable<object> selection) => Filter<BundleAsset>(value => selection.Any(selected => value.Bundle.Name == (string) selected));
    }
}