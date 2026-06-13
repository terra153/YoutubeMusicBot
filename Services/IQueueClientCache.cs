using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCord.Gateway.Voice;

namespace YoutubeMusicBot.Services
{
    public interface IQueueClientCache
    {
        public void RegisterClient(ulong guildId, QueueClient client);
        public void UnregisterClient(ulong guildId);
        public QueueClient? GetClient(ulong guildId);
    }
}