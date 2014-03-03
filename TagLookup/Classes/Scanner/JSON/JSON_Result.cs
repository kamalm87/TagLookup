using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagLookup.Classes.Scanner.JSON
{
    // This class hierachy is for JSON text to Object serialization using the
    // www.acoustid.org API
    // see: https://acoustid.org/webservice
    // assumed meta tags: meta=releases+releasegroups+tracks+compress+usermeta+source

    public class JSON_Result
    {
        public string status { get; set; }
        public List<Result> results { get; set; }




    }

    public class Result
    {
        public List<Releasegroup> releasegroups { get; set; }
        public double score { get; set; }
        public string id { get; set; }
    }

    public class Releasegroup
    {
        public List<Artist> artists { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public List<Release> releases { get; set; }
        public string title { get; set; }
    }

    public class Release
    {
        public int track_count { get; set; }
        public List<Releaseevent> releaseevents { get; set; }
        public string country { get; set; }
        public Date2 date { get; set; }
        public int medium_count { get; set; }
        public List<Medium> mediums { get; set; }
        public string id { get; set; }
    }

    public class Artist
    {
        public string id { get; set; }
        public string name { get; set; }
        public string joinphrase { get; set; }
    }

    public class Date
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
    }

    public class Releaseevent
    {
        public Date date { get; set; }
        public string country { get; set; }
    }

    public class Date2
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
    }

    public class Artist2
    {
        public string joinphrase { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Track
    {
        public int position { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public List<Artist2> artists { get; set; }
    }

    public class Medium
    {
        public int position { get; set; }
        public List<Track> tracks { get; set; }
        public int track_count { get; set; }
        public string format { get; set; }
    }

}
