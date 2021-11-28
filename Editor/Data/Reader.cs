
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using UnityEditor;

	namespace DependenciesExplorer.Editor.Data
	{
		public class Reader
		{
			public Dictionary<string, Bundle> Bundles = new Dictionary<string, Bundle>();

			public void ReadFile(string file)
			{
				Bundles.Clear();

				var lines = File.ReadAllLines(file);
				for (int i = 0; i < lines.Length; i++)
				{
					EditorUtility.DisplayProgressBar("Processing", $"Line {i + 1}/{lines.Length}", (float) (i + 1) / lines.Length);
					ProcessLine(lines, ref i);
				}

				var count = Bundles.Count;
				var j = 0;
				foreach (var node in Bundles.Values)
				{
					EditorUtility.DisplayProgressBar("Processing", $"Linking dependencies {j + 1}/{count}", (float) (j + 1) / count);
					foreach (var externalAsset in node.ExternalAssets)
					{
						if (!TryFindBundleByAsset(externalAsset.Key, node.Dependencies, out var other)) continue;
						if (!node.Out.TryGetValue(other, out var outFiles))
						{
							outFiles = new Dictionary<string, List<string>>();
							node.Out.Add(other, outFiles);
						}
						outFiles.Add(externalAsset.Key, externalAsset.Value);
						foreach (var depFile in externalAsset.Value)
						{
							if (!other.In.TryGetValue(node, out var inFiles))
							{
								inFiles = new Dictionary<string, List<string>>();
								other.In.Add(node, inFiles);
							}

							if (!inFiles.TryGetValue(externalAsset.Key, out var files))
							{
								files = new List<string>();
								inFiles.Add(externalAsset.Key, files);
							}
							files.Add(depFile);
						}

						foreach (var dependent in externalAsset.Value)
						{
							node.AssetsOut.Add( new BundleAsset { Bundle = other, Path = externalAsset.Key, Reference = dependent });
							other.AssetsIn.Add( new BundleAsset { Bundle = node, Path = dependent, Reference = externalAsset.Key });
						}
					}

					foreach (var dependency in node.Dependencies)
					{
						if (!Bundles.TryGetValue(dependency, out var bundle)) continue;
						if (!node.Out.TryGetValue(bundle, out var outFiles))
						{
							outFiles = new Dictionary<string, List<string>>();
							node.Out.Add(bundle, outFiles);
						}
					}
				}

				foreach (var node in Bundles.Values)
				{
					node.AssetsOut.Sort();
					node.AssetsIn.Sort();
				}

				EditorUtility.ClearProgressBar();
			}

			private bool TryFindBundleByAsset(string asset, string[] bundles, out Bundle result)
			{
				result = default;
				foreach (var bundleName in bundles)
				{
					if ( !Bundles.TryGetValue( bundleName, out var bundle ) ) continue;
					if (!bundle.ExplicitAssets.Contains(asset) && !bundle.ImplicitAssets.Contains(asset)) continue;
					result = bundle;
					return true;
				}

				return false;

			}

			private void ProcessLine(string[] lines, ref int i)
			{
				var match = Regex.Match(lines[i], "^\\tArchive (.*)\\.bundle", RegexOptions.IgnoreCase);
				if (!match.Success) return;
				i += 1;
				ProcessBundle(match.Groups[1].Value, lines, ref i);
			}

			private void ProcessBundle(string bundleName, string[] lines, ref int i)
			{
				if (TryProcessBundleDependencies(lines[i], out var dependencies)) i++;
				if (TryProcessExpandedBundleDependencies(lines[i], out var expandedDependencies)) i++;
				var externalAssets = new Dictionary<string, List<string>>();
				HandleExplicitAssets(lines, ref i, externalAssets, out var explicitAssets);
				HandleImplicitAssets(lines, ref i, externalAssets, out var implicitAssets);
				expandedDependencies.AddRange(dependencies);

				Bundles.Add(bundleName, new Bundle
				{
					Name = bundleName,
					Dependencies = dependencies,
					ExpandedDependencies = expandedDependencies.Distinct().ToArray(),
					ExplicitAssets = explicitAssets,
					ImplicitAssets = implicitAssets,
					ExternalAssets = externalAssets,
					In = new Dictionary<Bundle, Dictionary<string, List<string>>>(),
					Out = new Dictionary<Bundle, Dictionary<string, List<string>>>(),
					AssetsOut = new List<BundleAsset>(),
					AssetsIn = new List<BundleAsset>(),
				});
			}

			private static void HandleImplicitAssets(string[] lines, ref int i, Dictionary<string, List<string>> externalAssets, out string[] assets)
			{
				var assetsL = new List<string>();
				while (i < lines.Length && !lines[i].StartsWith("Group") && !lines[i].StartsWith("\tArchive"))
				{
					var line = lines[i];
					var file = Regex.Match(line, "\\t\\t\\t\\t\\t(.*) \\(Size", RegexOptions.IgnoreCase);
					if (file.Success)
					{
						assetsL.Add(file.Groups[1].Value.Trim());
						i++;
						continue;
					}
					var external = Regex.Match(line, "\\t\\t\\t\\t\\t\\tReferencing Assets: (.*)", RegexOptions.IgnoreCase);
					if (external.Success)
					{
						var lastFile = assetsL.LastOrDefault();
						foreach (var externalAsset in external.Groups[1].Value.Split(',').Select(f => f.Trim()))
						{
							if (!externalAssets.TryGetValue(externalAsset, out var linkedAssets))
							{
								linkedAssets = new List<string>();
								externalAssets.Add(externalAsset, linkedAssets);
							}

							linkedAssets.Add(lastFile);
						}
						i += 1;
						continue;
					}
					break;
				}

				assets = assetsL.ToArray();
			}

			private static void HandleExplicitAssets(string[] lines, ref int i, Dictionary<string, List<string>> externalAssets, out string[] assets)
			{
				var assetsL = new List<string>();
				while (i < lines.Length && !lines[i].EndsWith("Files:"))
				{
					var line = lines[i];
					var file = Regex.Match(line, "\\t\\t\\t(.*) \\(Total Size", RegexOptions.IgnoreCase);
					if (file.Success) assetsL.Add(file.Groups[1].Value.Trim());

					var external = Regex.Match(line, "\\t\\t\\t\\tExternal References: (.*)", RegexOptions.IgnoreCase);
					if (external.Success)
					{
						var lastFile = assetsL.Last();
						foreach (var externalAsset in external.Groups[1].Value.Split(',').Select(f => f.Trim()))
						{
							if (!externalAssets.TryGetValue(externalAsset, out var linkedAssets))
							{
								linkedAssets = new List<string>();
								externalAssets.Add(externalAsset, linkedAssets);
							}

							linkedAssets.Add(lastFile);
						}
					}
					i += 1;
				}

				assets = assetsL.ToArray();
			}

			private static bool TryProcessExpandedBundleDependencies(string line, out List<string> dependencies)
			{
				dependencies = null;
				var match = Regex.Match(line, "^\\t\\tExpanded Bundle Dependencies: (.*)", RegexOptions.IgnoreCase);
				if (!match.Success) return false;
				dependencies = match.Groups[1].Value.Split(',')
					.Where(value => !string.IsNullOrEmpty(value.Trim()))
					.Select(value => value.Trim().Replace(".bundle", "").ToLower())
					.ToList();
				return true;
			}

			private static bool TryProcessBundleDependencies(string line, out string[] dependencies)
			{
				dependencies = null;
				var result = Regex.Match(line, "^\\t\\tBundle Dependencies: (.*)", RegexOptions.IgnoreCase);
				if (!result.Success) return false;
				dependencies = result.Groups[1].Value.Split(',')
					.Where(value => !string.IsNullOrEmpty(value.Trim()))
					.Select(value => value.Trim().Replace(".bundle", "").ToLower())
					.ToArray();
				return true;
			}
		}
	}