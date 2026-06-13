using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCord.Services.ApplicationCommands;

namespace YoutubeMusicBot.Modules
{
    public class DefaultCommandModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("ping", "Проверка связи")]
        public string Pong() => $"Pong! {Context.Client.Latency.Milliseconds}ms";
    }
}