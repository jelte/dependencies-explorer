using System;
using System.Collections.Generic;

namespace DependenciesExplorer.Editor.Data
{
    public struct Bundle : IEquatable< Bundle >
    {
        public string Name;

        public string Category
        {
            get
            {
                var index = Name.IndexOf("-", StringComparison.Ordinal);
                if (index <= 0) index = Name.IndexOf("_", StringComparison.Ordinal);
                return Name.Substring(0,  index - 1);
            }
        }

        public string[] Dependencies;
        public string[] ExpandedDependencies;
        public string[] ImplicitAssets;
        public string[] ExplicitAssets;
        public Dictionary<string, List<string>> ExternalAssets;
        public Dictionary<Bundle, Dictionary<string, List<string>>> In;
        public Dictionary<Bundle, Dictionary<string, List<string>>> Out;

        public List<BundleAsset> AssetsIn;
        public List<BundleAsset> AssetsOut;

        public static bool operator ==(Bundle a, Bundle b) => a.Equals(b);
        public static bool operator !=(Bundle a, Bundle b) => !(a.Equals(b));

        public bool Equals(Bundle other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Bundle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}