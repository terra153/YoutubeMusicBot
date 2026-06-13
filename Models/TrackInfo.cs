using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Models
{
    public class TrackInfo
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Requested { get; set; }
        public bool IsYoutube { get; set; }
        public TrackInfo(string title, string url, string requested, bool isYoutube)
        {
            Title = title;
            Url = url;
            Requested = requested;
            IsYoutube = isYoutube;
        }
    }
}