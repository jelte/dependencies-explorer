using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace DependenciesExplorer.Editor.Data
{
	public class Indexer
	{
		private static Dictionary< string, string[] > _indexed;
		private static Dictionary< string, List< string > > _dependenciesCache;
		private static bool _indexingFinished = false;
		private static bool _indexing = false;

		[MenuItem("Tools/Indexer")]
		public static void Index()
		{
			if ( _indexing ) return;
			_indexing = true;
			_indexed ??= new Dictionary< string, string[] >();
			_dependenciesCache ??= new Dictionary< string, List< string > >();
			_indexed.Clear();
			_dependenciesCache.Clear();

			EditorCoroutineUtility.StartCoroutineOwnerless( Run_Indexer() );
		}

		public static IEnumerator Run_Indexer()
		{
			// Create a new progress indicator
			int progressId = Progress.Start("Indexer", "Indexing Dependencies", Progress.Options.Managed);

			var cancelled = false;
			Progress.RegisterCancelCallback( progressId, () => cancelled = true );
			var paused = false;
			Progress.RegisterPauseCallback( progressId, pause => paused = pause );

			var assetPaths = AssetDatabase.GetAllAssetPaths();
			var i = 0;
			Progress.ShowDetails( true );
			var stopwatch = Stopwatch.StartNew();
			foreach ( var assetPath in assetPaths )
			{
				Progress.Report(progressId, ( float )( i + 1 ) / assetPaths.Length, assetPath );
				Index(assetPath);
				if ( cancelled ) break;
				if ( paused ) yield return new WaitUntil(() => !paused);
				if ( stopwatch.ElapsedMilliseconds > 100L )
				{
					yield return null;
					stopwatch.Restart();
				}
				i++;
			}

			// The task is finished. Remove the associated progress indicator.
			Progress.Remove(progressId);
			_indexing = false;
			_indexingFinished = !cancelled;
		}

		private static void Index( string assetPath )
		{
			var guid = AssetDatabase.AssetPathToGUID( assetPath );
			if ( _indexed.ContainsKey( guid ) ) return;
			var dependencies = AssetDatabase.GetDependencies( assetPath, false );
			_indexed.Add( guid, dependencies );
			foreach ( var depPath in dependencies )
			{
				if ( depPath == assetPath ) continue;
				var depGuid = AssetDatabase.AssetPathToGUID( depPath );
				if ( !_dependenciesCache.TryGetValue( depGuid, out var files ) )
				{
					files = new List< string >();
					_dependenciesCache.Add( depGuid, files );
				}
				files.Add( guid  );
			}
		}

		public static bool TryFind( string guid, out List<string> dependencies )
		{
			if ( !_indexingFinished )
			{
				dependencies = null;
				return false;
			}
			return _dependenciesCache.TryGetValue( guid, out dependencies );
		}
	}
}