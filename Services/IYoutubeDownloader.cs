using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplodeNoPolytics.Videos;

namespace YoutubeMusicBot.Services
{
    public interface IYoutubeDownloader
    {
        public Task<string?> Download(string VideoURL);
        public Task<Video?> GetVideoMeta(string VideoURL);
    }
}