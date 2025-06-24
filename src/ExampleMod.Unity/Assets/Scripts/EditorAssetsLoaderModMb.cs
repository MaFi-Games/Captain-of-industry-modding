using Mafi.Unity;

public class EditorAssetsLoaderModMb : AssetsLoaderMb {

	public override IAlternativeAssetLoader GetAlternativeAssetLoader() {
		return EditorAssetsLoader.Instance;
	}

	/// <summary>
	/// Loads assets directly from the editor if possible, allowing changing assets such as materials or shaders
	/// in editor and have the changes reflect in the running game.
	///
	/// Keep in mind that if an asset was cloned (instantiated) by the caller, changes in Unity Editor won't propagate
	/// and extra code for reloading might be needed.
	/// </summary>
	private class EditorAssetsLoader : IAlternativeAssetLoader {

		public static readonly EditorAssetsLoader Instance = new EditorAssetsLoader();


		public bool ContainsAsset(string assetPath) {
			#if UNITY_EDITOR
			return UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
			#else
			return false;
			#endif
		}

		public bool TryLoadAsset<T>(string assetPath, out T asset) where T : UnityEngine.Object {
			#if UNITY_EDITOR
			asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
			return asset;
			#else
			asset = default(T);
			return false;
			#endif
		}

		private EditorAssetsLoader() { }

	}

}