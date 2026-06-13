using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Services
{
    public interface IQueueClient
    {
        public void AddToQueue(TrackInfo track);
        public List<TrackInfo> GetQueue(int count);
        public void Stop();
        public void Shuffle();
        public void Clear();
        public TrackInfo Jump(int position);
        public TrackInfo Remove(int position);
        public Task Dispose();
    }
}