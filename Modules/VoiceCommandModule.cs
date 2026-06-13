using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using YoutubeMusicBot.Services;


namespace YoutubeMusicBot.Modules;

public class VoiceCommandModule(YoutubeMusicBot.Services.IQueueClientCache _queueService, IYoutubeDownloader _youtubeDownloader) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("join", "Войти в голосовой канал")]
    public async Task Join()
    {
        //Чтобы не было таймаута
        await RespondAsync(InteractionCallback.DeferredMessage());

        var guild = Context.Guild!;

        if (Context.Guild == null) return;

        if (!guild.VoiceStates.TryGetValue(Context.User.Id, out var voiceState))
        {
            await Context.Interaction.ModifyResponseAsync(x => x.Content = "Вы не подключены ни к одному голосовому каналу!");
            return;
        }

        var client = Context.Client;

        var voiceClient = await client.JoinVoiceChannelAsync(
        guild.Id,
        voiceState.ChannelId.GetValueOrDefault());

        var queueClient = new QueueClient(voiceClient, _youtubeDownloader);
        _queueService.RegisterClient(guild.Id, queueClient);

        await Context.Interaction.ModifyResponseAsync(x => x.Content = "Подключён к голосовому каналу!");
    }
    [SlashCommand("leave", "Покинуть голосовой канал")]
    public async Task Leave()
    {
        if (Context.Guild == null) return;

        var _voiceClient = _queueService.GetClient(Context.Guild.Id);

        if (_voiceClient is null)
        {
            await RespondAsync(InteractionCallback.Message("Бот не находится в голосовом канале"));
            return;
        }

        _queueService.UnregisterClient(Context.Guild.Id);

        //Выходим из канала
        await Context.Client.UpdateVoiceStateAsync(new(Context.Guild.Id, null));

        await RespondAsync(InteractionCallback.Message("Вышел из голосового канала :("));
    }
}