using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

#nullable disable
namespace GorillaSubscreen.Tools;

public class AssetLoader
{
  private static AssetBundle loadedBundle;
  private static readonly Dictionary<string, Object> loadedAssets = new Dictionary<string, Object>();
  private static Task bundleLoadTask;

  public static async Task<T> LoadAsset<T>(string assetName) where T : Object
  {
    Object cached;
    if (AssetLoader.loadedAssets.TryGetValue(assetName, out cached) && cached is T)
      return (T) cached;
    if (Object.op_Equality((Object) AssetLoader.loadedBundle, (Object) null))
    {
      if (AssetLoader.bundleLoadTask == null)
        AssetLoader.bundleLoadTask = AssetLoader.LoadAssetBundle();
      await AssetLoader.bundleLoadTask;
    }
    TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
    AssetBundleRequest request = AssetLoader.loadedBundle.LoadAssetAsync<T>(assetName);
    ((AsyncOperation) request).completed += (Action<AsyncOperation>) (_ => completionSource.TrySetResult(request.asset as T));
    T obj = await completionSource.Task;
    T loadedAsset = obj;
    obj = default (T);
    if (Object.op_Inequality((Object) loadedAsset, (Object) null))
      AssetLoader.loadedAssets[assetName] = (Object) loadedAsset;
    return loadedAsset;
  }

  private static async Task LoadAssetBundle()
  {
    TaskCompletionSource<AssetBundle> completionSource = new TaskCompletionSource<AssetBundle>();
    Assembly assembly = typeof (AssetLoader).Assembly;
    string[] resources = assembly.GetManifestResourceNames();
    Debug.Log((object) $"[GS]: Found {resources.Length} embedded resources:");
    string[] strArray = resources;
    for (int index = 0; index < strArray.Length; ++index)
    {
      string res = strArray[index];
      Debug.Log((object) ("[GS]: - " + res));
      res = (string) null;
    }
    strArray = (string[]) null;
    string resourceName = "GorillaSubscreen.Content.gorillasub";
    Stream stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
      Debug.LogError((object) $"[GS]: Embedded asset bundle '{resourceName}' not found.");
      Debug.LogError((object) "[GS]: Make sure the file is set as 'Embedded Resource' in Visual Studio.");
      throw new FileNotFoundException($"Embedded asset bundle '{resourceName}' not found.");
    }
    Debug.Log((object) $"[GS]: Successfully opened stream for '{resourceName}' ({stream.Length} bytes)");
    AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
    ((AsyncOperation) request).completed += (Action<AsyncOperation>) (_ =>
    {
      if (Object.op_Equality((Object) request.assetBundle, (Object) null))
        Debug.LogError((object) "[GS]: AssetBundle.LoadFromStreamAsync returned null");
      completionSource.TrySetResult(request.assetBundle);
    });
    AssetBundle assetBundle = await completionSource.Task;
    AssetLoader.loadedBundle = assetBundle;
    assetBundle = (AssetBundle) null;
    if (Object.op_Equality((Object) AssetLoader.loadedBundle, (Object) null))
      throw new Exception("[GS]: Failed to load asset bundle from stream");
    Debug.Log((object) "[GS]: Asset bundle loaded successfully");
    assembly = (Assembly) null;
    resources = (string[]) null;
    resourceName = (string) null;
    stream = (Stream) null;
  }
}
