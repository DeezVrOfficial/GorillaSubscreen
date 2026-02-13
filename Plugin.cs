using BepInEx;
using BepInEx.Configuration;
using GorillaSubscreen.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace GorillaSubscreen;

[BepInPlugin("crystaldev.gorillasubscreenremake", "GorillaSubscreenRemake", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
  private GameObject _GSPrefab;
  private ConfigEntry<string> ChannelName;
  private ConfigEntry<string> ChannelType;
  private string YouTubeApiKey = "AIzaSyBeu5NSVtAdHq5zsT99RrObp8OhP6VLRqg";

  private void Awake()
  {
    this.ChannelName = this.Config.Bind<string>("Account", "channel_name", "placeholder", "");
    this.ChannelType = this.Config.Bind<string>("Account", "channel_type", "youtube", "");
    GorillaTagger.OnPlayerSpawned((Action) (async () => await this.SetupBoard()));
  }

  private async Task SetupBoard()
  {
    try
    {
      string prefabName = this.ChannelType.Value.ToLower() == "youtube" ? "SubBoardYT" : "SubBoardTT";
      GameObject gameObject = await AssetLoader.LoadAsset<GameObject>(prefabName);
      this._GSPrefab = gameObject;
      gameObject = (GameObject) null;
      if (Object.op_Equality((Object) this._GSPrefab, (Object) null))
        return;
      GameObject gsInstance = Object.Instantiate<GameObject>(this._GSPrefab);
      gsInstance.SetActive(true);
      gsInstance.transform.position = new Vector3(-62.8232f, 12.6908f, -83.8681f);
      gsInstance.transform.rotation = Quaternion.Euler(90.886f, 95.4097f, -90f);
      if (this.ChannelType.Value.ToLower() == "youtube")
      {
        Plugin.YouTubeChannelData data = await this.FetchYouTubeChannelData(this.ChannelName.Value);
        if (data != null)
        {
          TextMeshPro tmp1 = ((Component) gsInstance.transform.Find("Text (TMP) (1)"))?.GetComponent<TextMeshPro>();
          if (Object.op_Inequality((Object) tmp1, (Object) null))
            ((TMP_Text) tmp1).text = "Name: " + data.Title;
          TextMeshPro tmp2 = ((Component) gsInstance.transform.Find("Text (TMP) (2)"))?.GetComponent<TextMeshPro>();
          if (Object.op_Inequality((Object) tmp2, (Object) null))
            ((TMP_Text) tmp2).text = "Subscribers: " + data.SubscriberCount;
          TextMeshPro tmp4 = ((Component) gsInstance.transform.Find("Text (TMP) (4)"))?.GetComponent<TextMeshPro>();
          if (Object.op_Inequality((Object) tmp4, (Object) null))
            ((TMP_Text) tmp4).text = "Views: " + data.ViewCount;
          TextMeshPro tmp5 = ((Component) gsInstance.transform.Find("Text (TMP) (5)"))?.GetComponent<TextMeshPro>();
          if (Object.op_Inequality((Object) tmp5, (Object) null))
            ((TMP_Text) tmp5).text = "Videos posted: " + data.VideoCount;
          TextMeshPro tmp6 = ((Component) gsInstance.transform.Find("Text (TMP) (6)"))?.GetComponent<TextMeshPro>();
          if (Object.op_Inequality((Object) tmp6, (Object) null))
          {
            DateTime dt;
            if (DateTime.TryParse(data.PublishedAt, out dt))
              ((TMP_Text) tmp6).text = $"Creation Date: {dt:yyyy-MM-dd}";
            else
              ((TMP_Text) tmp6).text = "Creation Date: " + data.PublishedAt;
          }
          tmp1 = (TextMeshPro) null;
          tmp2 = (TextMeshPro) null;
          tmp4 = (TextMeshPro) null;
          tmp5 = (TextMeshPro) null;
          tmp6 = (TextMeshPro) null;
        }
        data = (Plugin.YouTubeChannelData) null;
      }
      prefabName = (string) null;
      gsInstance = (GameObject) null;
    }
    catch
    {
      Debug.LogError((object) "[GS]: Error setting up board.");
    }
  }

  private async Task<Plugin.YouTubeChannelData> FetchYouTubeChannelData(string channelName)
  {
    string searchUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={UnityWebRequest.EscapeURL(channelName)}&type=channel&key={this.YouTubeApiKey}";
    using (UnityWebRequest request = UnityWebRequest.Get(searchUrl))
    {
      UnityWebRequestAsyncOperation operation = request.SendWebRequest();
      YieldAwaitable yieldAwaitable;
      while (!((AsyncOperation) operation).isDone)
      {
        yieldAwaitable = Task.Yield();
        await yieldAwaitable;
      }
      if (request.result != 1)
        return (Plugin.YouTubeChannelData) null;
      JObject searchJson = JObject.Parse(request.downloadHandler.text);
      string channelId = searchJson["items"]?[(object) 0]?[(object) "id"]?[(object) "channelId"]?.ToString();
      if (string.IsNullOrEmpty(channelId))
        return (Plugin.YouTubeChannelData) null;
      string dataUrl = $"https://www.googleapis.com/youtube/v3/channels?part=snippet,statistics&id={channelId}&key={this.YouTubeApiKey}";
      using (UnityWebRequest dataRequest = UnityWebRequest.Get(dataUrl))
      {
        UnityWebRequestAsyncOperation op2 = dataRequest.SendWebRequest();
        while (!((AsyncOperation) op2).isDone)
        {
          yieldAwaitable = Task.Yield();
          await yieldAwaitable;
        }
        if (dataRequest.result != 1)
          return (Plugin.YouTubeChannelData) null;
        JObject dataJson = JObject.Parse(dataRequest.downloadHandler.text);
        JToken item = dataJson["items"]?[(object) 0];
        if (item == null)
          return (Plugin.YouTubeChannelData) null;
        return new Plugin.YouTubeChannelData()
        {
          Title = item[(object) "snippet"]?[(object) "title"]?.ToString(),
          SubscriberCount = item[(object) "statistics"]?[(object) "subscriberCount"]?.ToString(),
          ViewCount = item[(object) "statistics"]?[(object) "viewCount"]?.ToString(),
          VideoCount = item[(object) "statistics"]?[(object) "videoCount"]?.ToString(),
          PublishedAt = item[(object) "snippet"]?[(object) "publishedAt"]?.ToString()
        };
      }
    }
  }

  private class YouTubeChannelData
  {
    public string Title;
    public string SubscriberCount;
    public string ViewCount;
    public string VideoCount;
    public string PublishedAt;
  }
}
