using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StationConfigGenerator : MonoBehaviour
{
    [System.Serializable]
    public class RadioStationCollection
    {
        public List<RadioStationDetails> stations = new List<RadioStationDetails>();
    }

    private string rawStationText =
@"
absolute70s,Absolute 70s,https://edge-bauerall-01-gos2.sharp-stream.com/absolute70shigh.aac;
absolute80s,Absolute 80s,https://edge-bauerall-01-gos2.sharp-stream.com/absolute80shigh.aac;
absolute90s,Absolute 90s,https://edge-bauerall-01-gos2.sharp-stream.com/absolute90shigh.aac;
absoluteclassicrock,Absolute Classic Rock,http://edge-bauerall-01-gos2.sharp-stream.com/absoluteclassicrockhigh.aac;
absoluteradio,Absolute Radio,http://www.radiofeeds.net/playlists/bauerflash.pls?station=absoluteradio-mp3;
bbcasiannetwork,BBC Asian Network,http://as-hls-ww-live.akamaized.net/pool_22108647/live/ww/bbc_asian_network/bbc_asian_network.isml/bbc_asian_network-audio%3d128000.norewind.m3u8;
bbcradio1,BBC Radio 1,http://a.files.bbci.co.uk/ms6/live/3441A116-B12E-4D2F-ACA8-C1984642FA4B/audio/simulcast/hls/nonuk/pc_hd_abr_v2/ak/bbc_radio_one.m3u8;
bbcradio1xtra,BBC Radio 1Xtra,http://as-hls-ww-live.akamaized.net/pool_92079267/live/ww/bbc_1xtra/bbc_1xtra.isml/bbc_1xtra-audio%3d128000.norewind.m3u8;
bbcradio2,BBC Radio 2,https://as-hls-ww.live.cf.md.bbci.co.uk/pool_74208725/live/ww/bbc_radio_two/bbc_radio_two.isml/bbc_radio_two-audio=96000-272331559.ts;
bbcradio3,BBC Radio 3,http://lstn.lv/bbcradio.m3u8?station=bbc_radio_three%22&bitrate=320000%22;
bbcradio4,BBC Radio 4,http://lstn.lv/bbcradio.m3u8?station=bbc_radio_fourfm%22&bitrate=320000%22;
bbcradio4extra,BBC Radio 4 Extra,http://lstn.lv/bbcradio.m3u8?station=bbc_radio_four_extra%&bitrate=320000;
bbcradio5live,BBC Radio 5 Live,http://lstn.lv/bbcradio.m3u8?station=bbc_radio_five_live&bitrate=96000;
bbcradio5livesportsextra,BBC Radio 5 Live Sports Extra,http://lstn.lv/bbcradio.m3u8?station=bbc_radio_five_live_sports_extra&bitrate=320000;
bbcradio6music,BBC 6 Music,http://as-hls-ww-live.akamaized.net/pool_81827798/live/ww/bbc_6music/bbc_6music.isml/bbc_6music-audio%3d320000.norewind.m3u8;
bbcradioscotland,BBC Radio Scotland,http://as-hls-ww-live.akamaized.net/pool_43322914/live/ww/bbc_radio_scotland_fm/bbc_radio_scotland_fm.isml/bbc_radio_scotland_fm-audio%3d128000.norewind.m3u8;
bbcradioworldservice,BBC Radio World Service,https://stream.live.vc.bbcmedia.co.uk/bbc_world_service; 
capitaluk,Capital UK,http://icecast.thisisdax.com/CapitalUKMP3;
capitalxtra,Capital XTRA,http://media-the.musicradio.com/CapitalXTRALondonMP3;
classicfm,Classic FM,http://icecast.thisisdax.com/ClassicFMMP3;
goldfm,Gold FM,http://media-the.musicradio.com/GoldMP3;
greatesthits,Greatest Hits Radio,http://edge-bauerall-01-gos2.sharp-stream.com/net2london.mp3;
heart,Heart Radio,http://ice-the.musicradio.com:80/HeartUKMP3;
heart80s,Heart 80s,http://media-the.musicradio.com:80/Heart80sMP3;
heat,Heat Radio,https://edge-bauerall-01-gos2.sharp-stream.com/heat.mp3;
hitsradio,HITS Radio,http://edge-bauerall-01-gos2.sharp-stream.com/hits.aac;
jazzfm,Jazz FM,https://edge-bauerall-01-gos2.sharp-stream.com/jazzhigh.aac;
kerrang,Kerrang! Radio,https://edge-bauerall-01-gos2.sharp-stream.com/kerrang.mp3;
kisstory,KISSTORY,https://edge-bauerall-01-gos2.sharp-stream.com/kisstory.aac;
kissfresh,KISS Fresh,http://live-bauer-al.sharp-stream.com/kissfresh.mp3;
lbc,LBC 97.3,https://media-ssl.musicradio.com/LBCUK;
magicchilled,Magic CHILLED,https://edge-bauerall-01-gos2.sharp-stream.com/magicchilled.aac;
magicfm,Magic FM,http://edge-bauermz-01-gos2.sharp-stream.com/magic1054.mp3?aw_0_1st.skey=1657298076;
magicsoul,Magic SOUL,https://edge-bauerall-01-gos2.sharp-stream.com/magicsoul.aac;
mellowmagic,Mellow Magic,https://edge-bauerall-01-gos2.sharp-stream.com/magicmellow.aac;
planetoldies,Planet Oldies,https://planetoldies-libstrm.radioca.st/stream;
planetrock,Planet Rock,https://edge-bauerall-01-gos2.sharp-stream.com/planetrock.aac;
radiox,Radio X,https://media-ice.musicradio.com/RadioXLondonMP3;
smooth,Smooth Radio,http://icecast.thisisdax.com/SmoothUKMP3;
smoothextra,Smooth Extra,http://icecast.thisisdax.com/SmoothLondon.m3u;
sunrise,SUNRISE RADIO,https://direct.sharp-stream.com/sunriseradio.mp3;
talkradio,talk RADIO,http://radio.talkradio.co.uk/stream-mobile?ref=rf;
talksport,talkSPORT,http://radio.talksport.com/stream?aisGetOriginalStream=true;
talksport2,talkSPORT2,http://radio.talksport.com/stream2?awparams=platform:ts-web&amsparams=playerid:ts-web;;
timesradio,Times Radio,https://timesradio.wireless.radio/stream;
virgin,Virgin Radio,https://radio.virginradio.co.uk/stream?aw_0_1st.platform=vr-web;
";

    //timesradio ""https://timesradio.wireless.radio/stream?aw_0_1st.platform=website&aw_0_1st.playerid=wireless-website""


    public bool makeNGINX;
    public bool makePYTHON;
    public bool makeJSON;

    [ContextMenu("Generate Configs")]
    public void GenerateConfigs()
    {
        var stationList = ParseStationListFromText(rawStationText);
        if (makeNGINX) GenerateNginxMap(stationList);
        if (makePYTHON) GeneratePythonStationMap(stationList);
        if (makeJSON) GenerateJsonFile(stationList);
        Debug.Log("✅ All config files generated in StreamingAssets/StationConfigs");
    }

    private List<RadioStationDetails> ParseStationListFromText(string rawText)
    {
        print("Raw Text = " + rawText);

        List<RadioStationDetails> parsedList = new List<RadioStationDetails>();

        string[] lines = rawText.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
        //string[] lines = rawText.Split('\n');
        print("Raw Text = " + rawText);
        print ("lines[0] = "+lines[0]);

        foreach (string line in lines)
        {
            print("Line = " + line);
            string trimmed = line.TrimStart().TrimEnd(';');
            //string trimmed = line.Trim().TrimEnd(';');

            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

            string[] parts = trimmed.Split(',');
            print("num of parts = " + parts.Length);
            if (parts.Length != 3) continue;

            string key = parts[0].Trim();
            string name = parts[1].Trim();
            string url = parts[2].Trim();

            print("key = " + key);
            print("name = " + name);
            print("url = " + url);

            parsedList.Add(new RadioStationDetails
            {
                stationuuid = key,
                name = name,
                favicon = $"Assets/Resources/Favicon/{key}.jpg",
                streaming_url = url,
                tags = "",
                genre = "",
                faviconBGColour = "#000000",
                ranking = ""
            });
        }

        return parsedList;
    }


    private void GenerateNginxMap(List<RadioStationDetails> stationList)
    {
        StringBuilder nginxMap = new StringBuilder();
        nginxMap.AppendLine("map $arg_station $upstream_stream {");
        nginxMap.AppendLine("    default \"\";");

        foreach (var station in stationList)
        {
            if (!string.IsNullOrEmpty(station.stationuuid) && !string.IsNullOrEmpty(station.streaming_url))
            {
                nginxMap.AppendLine($"    {station.stationuuid} \"{station.streaming_url}\";");
            }
        }

        nginxMap.AppendLine("}");
        WriteToFile("nginx_map.txt", nginxMap.ToString());
    }

    private void GeneratePythonStationMap(List<RadioStationDetails> stationList)
    {
        StringBuilder pyMap = new StringBuilder();
        pyMap.AppendLine("station_map = {");

        foreach (var station in stationList)
        {
            if (!string.IsNullOrEmpty(station.stationuuid) && !string.IsNullOrEmpty(station.streaming_url))
            {
                pyMap.AppendLine($"    \"{station.stationuuid}\": \"{station.streaming_url}\",");
            }
        }

        pyMap.AppendLine("}");
        WriteToFile("station_map.py", pyMap.ToString());
    }

    private void GenerateJsonFile(List<RadioStationDetails> stationList)
    {
        foreach (var station in stationList)
        {
            station.streaming_url = $"https://wamballa.com/stream/?station={station.stationuuid}";
        }

        RadioStationCollection collection = new RadioStationCollection { stations = stationList };
        string json = JsonUtility.ToJson(collection, true);
        WriteToFile("StationData.json", json);
    }

    private void WriteToFile(string filename, string content)
    {

#if UNITY_EDITOR
        string resourcesFolder = "Assets/Resources/GeneratedConfigs";
        if (!Directory.Exists(resourcesFolder)) Directory.CreateDirectory(resourcesFolder);

        string assetPath = Path.Combine(resourcesFolder, filename);
        File.WriteAllText(assetPath, content);

        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();

        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        if (asset != null)
        {
            Debug.Log("📄 TextAsset created: " + asset.name);
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find TextAsset at: " + assetPath);
        }
#endif
    }
}
