using System;

namespace DependenciesExplorer.Editor.Data
{
    public struct BundleAsset : IComparable<BundleAsset>
    {
        public string Path;
        public Bundle Bundle;
        public string Reference;
        
        public int CompareTo(BundleAsset other)
        {
            var bundle = Bundle.Name.CompareTo(other.Bundle.Name);
            if (bundle != 0) return bundle;
            var path = Path.CompareTo(other.Path);
            if (path != 0) return path;
            return Reference.CompareTo(other.Reference);
        }
    }
}