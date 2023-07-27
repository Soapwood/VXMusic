using Disco.Business.Interfaces.Dtos.AudD;
using System;
using System.Collections.Generic;
using System.Text;

namespace Disco.Integration.Interfaces.Dtos.AudD
{
    public class Result
    {
        public string artist { get; set; }
        public string title { get; set; }
        public string album { get; set; }
        public string release_date { get; set; }
        public string label { get; set; }
        public string timecode { get; set; }
        public string song_link { get; set; }
        //public AppleMusic apple_music { get; set; }
        //public Spotify spotify { get; set; }
    }
}
