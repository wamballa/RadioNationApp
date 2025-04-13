[System.Serializable]

public class RadioStationDetails 
{
    public string stationuuid; // ✅ Unique identifier for each station
    public string name;
    public string ranking;
    public string favicon;
    public string streaming_url; // ✅ The actual streaming link
    public string tags;
    public string genre;
    public string faviconBGColour;
}
