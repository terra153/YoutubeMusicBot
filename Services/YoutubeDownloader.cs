using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplodeNoPolytics;
using YoutubeExplodeNoPolytics.Converter;
using YoutubeExplodeNoPolytics.Videos;
using YoutubeExplodeNoPolytics.Videos.Streams;

namespace YoutubeMusicBot.Services
{
    public class YoutubeDownloader : IYoutubeDownloader
    {
        Random _random = new();
        public async Task<string?> Download(string VideoURL)
        {
            int id = _random.Next(999999999);

            //10 быстрых попыток
            for (int i = 0; i < 10; i++)
            {

                var process = CreateYtDlpProcess(id, VideoURL);

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    //Все файлы в папке загрузок
                    foreach (var file in Directory.GetFiles($"{Directory.GetCurrentDirectory() + "/YtDownloads/"}"))
                    {
                        //Удаляем временные файлы неудачных загрузок
                        if (file.Contains(id.ToString()))
                        {
                            File.Delete(file);
                        }
                    }
                    continue;
                }
                ;

                break;
            }


            if (!File.Exists($"{Directory.GetCurrentDirectory() + "/YtDownloads/"}{id}.mp3")) return null;

            return $"{Directory.GetCurrentDirectory() + "/YtDownloads/"}{id}.mp3";
        }

        public async Task<Video?> GetVideoMeta(string VideoURL)
        {
            var cookies = YoutubeCookieParser.ParseCookieFile(Directory.GetCurrentDirectory() + "/cookies.txt");

            var youtube = new YoutubeClient(cookies);
            var videoId = YoutubeExplodeNoPolytics.Videos.VideoId.Parse(VideoURL);
            return await youtube.Videos.GetAsync(videoId);
        }

        private static Process CreateYtDlpProcess(int id, string VideoURL)
        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = "yt-dlp",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var args = processInfo.ArgumentList;

            args.Add("-x");
            args.Add("--retries"); args.Add("1");
            args.Add("--fragment-retries"); args.Add("1");
            args.Add("--legacy-server-connect");
            args.Add("--extractor-args"); args.Add("youtube:player-client=ios,tv;skip=dash,hls,ms");
            args.Add("--http-chunk-size"); args.Add("10M");
            args.Add("-f"); args.Add("ba[ext=m4a]/ba/best");
            args.Add("--remote-components"); args.Add("ejs:github");
            args.Add("--cookies"); args.Add($"{Directory.GetCurrentDirectory() + "/cookies.txt"}");
            args.Add("--audio-format"); args.Add("mp3");
            args.Add("--no-check-certificates");
            args.Add("-o"); args.Add($"{Directory.GetCurrentDirectory() + "/YtDownloads/"}{id}.mp3");
            args.Add($"{VideoURL}");


            var process = Process.Start(processInfo)!;

            return process;
        }
    }
}