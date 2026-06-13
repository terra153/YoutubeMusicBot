using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetCord.Gateway.Voice;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Services
{
    public class QueueClient : IQueueClient
    {
        private readonly IYoutubeDownloader _youtubeDownloader;
        private readonly VoiceClient _client;
        private int? _currentTrackIndex;
        private CancellationTokenSource? _tokenSource;
        public TrackInfo CurrentTrack => _queue[_currentTrackIndex ?? 0];
        private Process? _currentStreamProcess;

        private Stream? _voiceStream;
        private OpusEncodeStream? _opusStream;

        private List<TrackInfo> _queue = [];
        public QueueClient(VoiceClient client, IYoutubeDownloader youtubeDownloader)
        {
            _client = client;
            _youtubeDownloader = youtubeDownloader;
            Init();
        }
        async void Init()
        {
            await _client.StartAsync();

            _voiceStream = _client.CreateVoiceStream();
            //Оборачиваем в удобный для discord Opus
            _opusStream = new(_voiceStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);
        }
        public void AddToQueue(TrackInfo track)
        {
            _queue.Add(track);

            if (_currentStreamProcess == null) Play();
        }

        public async void Stop()
        {
            try
            {
                _currentStreamProcess?.Kill();
            }
            catch (System.Exception)
            { }
        }
        private void Next()
        {
            _currentTrackIndex++;

            if (_queue.Count <= _currentTrackIndex)
            {
                _currentTrackIndex = 0;
            }

            Play();
        }

        async void Play()
        {

            //Выполняется при добавлении первого трека в очередь
            if (_queue.Count > 0 && _currentTrackIndex == null)
                _currentTrackIndex = _queue.IndexOf(_queue.First());

            //Если нет треков в очереди
            if (_queue.Count == 0) return;

            await _client.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));
            _tokenSource = new();

            var path = CurrentTrack.IsYoutube ? await _youtubeDownloader.Download(CurrentTrack.Url) : CurrentTrack.Url;

            if (path == null)
            {
                Next();
                return;
            }

            using var ffmpeg = CreateFFmpegProcess(path);
            using var output = ffmpeg.StandardOutput.BaseStream;

            _currentStreamProcess = ffmpeg;

            try
            {
                //Ограничиваем буфер дабы не было "рывков" при воспроизведении
                await output.CopyToAsync(_opusStream!, 3840, _tokenSource.Token);
                await _opusStream!.FlushAsync(_tokenSource.Token);
            }
            catch (System.Exception)
            {
                //Выполняется при Clear()
            }
            finally
            {
                await _client.EnterSpeakingStateAsync(new SpeakingProperties(0));

                //При окончании автоматически идём дальше
                _currentStreamProcess?.Kill();
                _currentStreamProcess?.WaitForExit(2000);
                _currentStreamProcess = null;

                Next();
            }

        }
        public async Task Dispose()
        {
            Clear();
            await _client.CloseAsync();
            _client.Dispose();
        }

        public async void Clear()
        {
            _queue.Clear();
            Array.ForEach(Directory.GetFiles("YtDownloads"), File.Delete);
            _tokenSource?.Cancel();
            _currentStreamProcess?.Kill();
        }

        private Process CreateFFmpegProcess(string? url)
        {
            string args;
            if (url == null)
            {
                //Принимает данные из stdin
                args = $"-i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1";
            }
            else
            {
                args = $"-i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1 -reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5";
            }

            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            })!;
        }

        public List<TrackInfo> GetQueue(int count)
        {
            if (_currentTrackIndex == null) return [];

            int tracksRemainingCount = _queue.Count - _currentTrackIndex ?? 0;

            var tracksAfterCurrent = _queue.GetRange(_currentTrackIndex ?? 0, tracksRemainingCount);
            var tracksBeforeCurrent = _queue.GetRange(0, _currentTrackIndex ?? 0);


            List<TrackInfo> nextQueue = [.. tracksAfterCurrent, .. tracksBeforeCurrent];

            return [.. nextQueue.Take(count)];
        }

        public TrackInfo Jump(int position)
        {
            //Получаем актуальную дорожку
            var nextQueue = GetQueue(1000);

            //Находим трек по номеру
            var track = nextQueue[position - 1];

            //Передаём индекс трека в оригинальном списке, -1 при учёте, что Skip() автоинкрементит значение
            _currentTrackIndex = _queue.IndexOf(track) - 1;
            Stop();

            return track;
        }

        public TrackInfo Remove(int position)
        {
            //Получаем актуальную дорожку
            var nextQueue = GetQueue(1000);

            //Находим трек по номеру
            var track = nextQueue[position - 1];

            //Удаляем трек из оригинального списка
            _queue.Remove(track); ;

            return track;
        }

        public void Shuffle()
        {
            _queue = [.. _queue.Shuffle()];

            //-1 потому что будет автоинкремент в Next()
            _currentTrackIndex = -1;
            Stop();
        }
    }
}