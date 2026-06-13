using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCord.Gateway.Voice;

namespace YoutubeMusicBot.Services
{
    public class QueueClientCache : IQueueClientCache
    {
        private readonly ConcurrentDictionary<ulong, QueueClient> _activeClients = new();

        public void RegisterClient(ulong guildId, QueueClient client)
        {
            _activeClients[guildId] = client;
        }

        public async void UnregisterClient(ulong guildId)
        {
            if (_activeClients.TryRemove(guildId, out var client))
            {
                await client.Dispose();
            }
        }

        public QueueClient? GetClient(ulong guildId)
        {
            _activeClients.TryGetValue(guildId, out var client);
            return client;
        }
    }
}