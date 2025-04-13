//using UnityEngine;
//using System.Collections;
//using UnityEngine.Networking;
//using System;


//[Serializable]
//public class MetadataResponse
//{
//    public string icy_name;
//    public string icy_description;
//    public string now_playing;
//}


//public class MetadataHandler : MonoBehaviour
//{
//    [Header("Debug Flags")]
//    public bool logToConsole = true;
//    [Header("Core Engine")]
//    public RadioPlayer radioPlayer;
//    //public VLCPlayer vlcPlayer;

//    [Header("Meta Data strings")]
//    public string nowPlayingMeta;
//    public string previous_nowPlayingMeta;

//    // Private variables
//    //private bool hasFoundMeta;
//    private Coroutine metadataCoroutine;
//    //private int numberOfAttempts;

//    public void StartFetchingMetadata()
//    {
//        Log("[MetaDataHandler] Started fetching meta data");
//        //numberOfAttempts = 3;

//        previous_nowPlayingMeta = "none";
//        nowPlayingMeta = "";
//        //hasFoundMeta = false;

//        if (metadataCoroutine != null)
//        {
//            StopCoroutine(metadataCoroutine);
//        }
//        metadataCoroutine = StartCoroutine(UpdateMetadata());
//    }

//    private IEnumerator UpdateMetadata()
//    {
//        while (radioPlayer.playerState == RadioPlayer.PlayerState.Playing)
//        {
//            //numberOfAttempts--;
//            yield return new WaitForSeconds(5);
//            FetchMetadata();
//        }
//    }

//    private void FetchMetadata()
//    {

//        if (radioPlayer == null) return;

//        // ✅ Check if the current stream is an .m3u or .m3u8 playlist
//        string currentStreamUrl = radioPlayer.currentStreamingURL;

//        // Replace only the first occurrence of /stream/ with /metadata/
//        string metadataUrl = currentStreamUrl.Replace("/stream/", "/metadata/");

//        Log("[MetadataHandler] Metadata URL = " + metadataUrl);

//        //Helper.Log("Attempting to fetch meta data. Attempt " + numberOfAttempts);
//        //Helper.Log("FetchMetaData, currentStreamUrl = " + currentStreamUrl);
//        if (currentStreamUrl.EndsWith(".m3u") || metadataUrl.EndsWith(".m3u8"))
//        {
//            Log("[Metadata Handler] stream ends in .m3u");
//            StartCoroutine(ResolveMetadataFromPlaylist(metadataUrl));
//        }
//        else
//        {
//            StartCoroutine(ParseMetadata(metadataUrl));
//        }
//    }

//    private IEnumerator ResolveMetadataFromPlaylist(string playlistUrl)
//    {
//        Log($"🔍 Resolving metadata from playlist: {playlistUrl}");

//        using (UnityWebRequest request = UnityWebRequest.Get(playlistUrl))
//        {
//            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
//            request.redirectLimit = 5;

//            yield return request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                LogError($"❌ Failed to fetch metadata playlist: {request.error}");
//                yield break;
//            }

//            string playlistContent = request.downloadHandler.text;
//            Log($"📜 Playlist Metadata Content:\n{playlistContent}");

//            // ✅ Extract the first valid stream URL
//            string resolvedStreamUrl = ExtractStreamURLFromPlaylist(playlistContent);

//            if (!string.IsNullOrEmpty(resolvedStreamUrl))
//            {
//                Log($"✅ Using stream URL for metadata: {resolvedStreamUrl}");
//                //radioPlayer.SetResolvedStreamUrl(resolvedStreamUrl); // Store new stream URL
//                StartCoroutine(ParseMetadata(resolvedStreamUrl)); // Fetch metadata now that we have the real stream
//            }
//            else
//            {
//                LogError("❌ No valid stream URL found in playlist for metadata.");
//            }
//        }
//    }

//    private string ExtractStreamURLFromPlaylist(string playlistContent)
//    {
//        string[] lines = playlistContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

//        foreach (string line in lines)
//        {
//            if (line.StartsWith("http")) // ✅ Find the first valid stream link
//            {
//                return line.Trim();
//            }
//        }
//        return null; // No valid stream found
//    }



//    private IEnumerator ParseMetadata(string streamUrl)
//    {
//        //Log("[MetaDataHandler] ParseMetadata from "+streamUrl);

//        UnityWebRequest request = UnityWebRequest.Get(streamUrl);
//        request.downloadHandler = new DownloadHandlerBuffer();
//        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
//        request.timeout = 10;

//        //Log("[MetaDataHandler] Sending metadata request to: " + streamUrl);

//        yield return request.SendWebRequest();

//        //Log("[MetaDataHandler] Metadata request completed.");

//        if (request.result != UnityWebRequest.Result.Success)
//        {
//            LogError("[MetaDataHandler] Metadata fetch failed: " + request.error);
//            yield break;
//        }

//        string json = request.downloadHandler.text;

//        Log("[MetaDataHandler] Received JSON: " + json);

//        MetadataResponse meta = JsonUtility.FromJson<MetadataResponse>(json);

//        nowPlayingMeta = meta.now_playing ?? meta.icy_description ?? meta.icy_name ?? "";

//        if (!string.IsNullOrEmpty(nowPlayingMeta))
//        {
//            if (nowPlayingMeta.Contains("AD") || nowPlayingMeta.Contains("Advert"))
//            {
//                nowPlayingMeta = "Ad Break";
//                Log("[MetaDataHandler] Skipped ad metadata: " + nowPlayingMeta);
//            }

//            //hasFoundMeta = true;

//            if (nowPlayingMeta != previous_nowPlayingMeta)
//            {
//                Log("[MetaDataHandler] Now Playing: " + nowPlayingMeta);
//                previous_nowPlayingMeta = nowPlayingMeta;
//            }
//        }
//        else
//        {
//            Log("[MetaDataHandler] No Now Playing data found.");
//            //hasFoundMeta = false;
//        }

//    }

//    public void StopFetchingMetadata()
//    {
//        if (metadataCoroutine != null)
//        {
//            StopCoroutine(metadataCoroutine);
//            metadataCoroutine = null;
//        }
//    }

//    void Log(object message)
//    {
//        if (logToConsole)
//            Debug.Log(message);
//    }

//    void LogError(object message)
//    {
//        if (logToConsole)
//            Debug.LogError(message);
//    }

//}



