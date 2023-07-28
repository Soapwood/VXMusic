using System.Collections.Generic;

public class ShazamHttpResponse
{
    public List<Match> matches { get; set; }
    public long timestamp { get; set; }
    public string timezone { get; set; }
    public string tagid { get; set; }
    public Track track { get; set; }
}

public class Root
{
    public List<ShazamHttpResponse> trackInfo { get; set; }
}

public class Match
{
    public string id { get; set; }
    public double offset { get; set; }
    public double timeskew { get; set; }
    public double frequencyskew { get; set; }
}

public class Images
{
    public string background { get; set; }
    public string coverart { get; set; }
    public string coverarthq { get; set; }
    public string joecolor { get; set; }
}

public class Share
{
    public string subject { get; set; }
    public string text { get; set; }
    public string href { get; set; }
    public string image { get; set; }
    public string twitter { get; set; }
    public string html { get; set; }
    public string avatar { get; set; }
    public string snapchat { get; set; }
}

public class Action
{
    public string name { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
}

public class Provider
{
    public string caption { get; set; }
    public Images images { get; set; }
    public List<Action> actions { get; set; }
    public string type { get; set; }
}

public class Hub
{
    public string type { get; set; }
    public string image { get; set; }
    public List<Action> actions { get; set; }
    public List<Action> options { get; set; }
    public List<Provider> providers { get; set; }
    public bool @explicit { get; set; }
    public string displayname { get; set; }
}

public class Artist
{
    public string id { get; set; }
    public string adamid { get; set; }
}

public class Genres
{
    public string primary { get; set; }
}

public class UrlParams
{
    public string tracktitle { get; set; }
    public string trackartist { get; set; }
}

public class AppleMusic
{
    public List<Action> actions { get; set; }
}

public class MyShazam
{
    public AppleMusic apple { get; set; }
}

public class Metadata
{
    public string title { get; set; }
    public string text { get; set; }
}

public class Metapages
{
    public string image { get; set; }
    public string caption { get; set; }
}

public class Sections
{
    public string type { get; set; }
    public List<Metapages> metapages { get; set; }
    public string tabname { get; set; }
    public List<Metadata> metadata { get; set; }
    public List<string> text { get; set; }
    public string footer { get; set; }
    public string youtubeurl { get; set; }
}

public class Track
{
    public string layout { get; set; }
    public string type { get; set; }
    public string key { get; set; }
    public string title { get; set; }
    public string subtitle { get; set; }
    public Images images { get; set; }
    public Share share { get; set; }
    public Hub hub { get; set; }
    public string url { get; set; }
    public List<Artist> artists { get; set; }
    public string isrc { get; set; }
    public Genres genres { get; set; }
    public UrlParams urlparams { get; set; }
    public MyShazam myshazam { get; set; }
    public string albumadamid { get; set; }
    public List<Dictionary<string, object>> sections { get; set; } // TODO Parse "sections" properly.
}

