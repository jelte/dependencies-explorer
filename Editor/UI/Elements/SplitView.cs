using UnityEngine.UIElements;

namespace DependenciesExplorer.Editor.UI.Elements
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits>
        {
        }

        public SplitView() : base(0, 300, TwoPaneSplitViewOrientation.Horizontal)
        {
            RegisterCallback(new EventCallback<GeometryChangedEvent>(OnPostDisplaySetup));
        }

        private void OnPostDisplaySetup(GeometryChangedEvent evt)
        {
         //   CollapseChild(fixedPaneIndex);
            UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
        }
    }
}