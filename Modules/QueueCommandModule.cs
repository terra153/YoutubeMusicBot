using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using YoutubeMusicBot.Services;

namespace YoutubeMusicBot.Modules
{
    public class QueueCommandModule(YoutubeMusicBot.Services.IQueueClientCache _queueClientCache, IYoutubeDownloader _youtubeDownloader) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("play-local", "Играет локальную музыку с вашего компьютера")]
        public async Task PlayLocal([SlashCommandParameter(Description = "Выберите аудиофайл (.mp3, .wav)")] Attachment file)
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            string contentType = file.ContentType ?? "";
            if (!contentType.StartsWith("audio/") && !contentType.StartsWith("video/"))
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Невалидный формат файла!");
                return;
            }

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Подготовка к проигрыванию {file.FileName}");

            _queueClient.AddToQueue(new Models.TrackInfo(file.FileName, file.Url, $"{Context.User.Username} aka {Context.User.GlobalName}", false));
        }
        [SlashCommand("play-youtube", "Играет музыку с ютуба")]
        public async Task PlayYoutube(string url, bool force = false)
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    var vid = await _youtubeDownloader.GetVideoMeta(url);

                    if (vid == null)
                    {
                        await Context.Interaction.ModifyResponseAsync(x => x.Content = "Не удаётся скачать видео. Проверьте ссылку!");
                        return;
                    }

                    await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Подготовка к проигрыванию {vid.Title}");

                    _queueClient.AddToQueue(new Models.TrackInfo(vid.Title, url, $"{Context.User.Username} aka {Context.User.GlobalName}", true));
                }
                catch (System.Exception)
                {
                    if (force)
                    {
                        await Context.Interaction.ModifyResponseAsync(x => x.Content = "Не удаётся загрузить информацию о видео! Форсированный режим включён");
                        _queueClient.AddToQueue(new Models.TrackInfo("Неизвестно", url, $"{Context.User.Username} aka {Context.User.GlobalName}", true));
                    }
                    else
                    {
                        await Context.Interaction.ModifyResponseAsync(x => x.Content = "Не удаётся загрузить информацию о видео!");
                    }

                }

            });
        }

        [SlashCommand("clear", "Очистить очередь")]
        public async Task Clear()
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            _queueClient.Clear();

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Очистил очередь!");
        }
        [SlashCommand("shuffle", "Перемешать очередь")]
        public async Task Shuffle()
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            _queueClient.Shuffle();

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Перемешал очередь!");
        }
        [SlashCommand("next", "Следующий трек")]
        public async Task Next()
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            _queueClient.Stop();

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Переключил трек!");
        }
        [SlashCommand("remove", "Удалить трек")]
        public async Task Remove(int trackNumber)
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            var removed = _queueClient.Remove(trackNumber);

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Удалил трек {removed.Title}!");
        }
        [SlashCommand("jump", "Перейти к треку")]
        public async Task Jump(int trackNumber)
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            var jumpedTo = _queueClient.Jump(trackNumber);

            await Context.Interaction.ModifyResponseAsync(x => x.Content = $"Переключил трек! Сейчас играет: {jumpedTo.Title}");
        }
        [SlashCommand("queue", "Вывести очередь")]
        public async Task PrintQueue(int count)
        {
            //Чтобы не было таймаута
            await RespondAsync(InteractionCallback.DeferredMessage());

            if (Context.Guild == null) return;

            var _queueClient = _queueClientCache.GetClient(Context.Guild.Id);

            if (_queueClient is null)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Бот не в голосовом канале!");
                return;
            }

            var queue = _queueClient.GetQueue(count);

            if (queue.Count == 0)
            {
                await Context.Interaction.ModifyResponseAsync(x => x.Content = "Очередь пуста!");
                return;
            }

            StringBuilder output = new();
            output.AppendLine("Сейчас играет: ");

            for (int i = 0; i < queue.Count; i++)
            {
                output.Append(i + 1);
                output.Append(". ");
                output.AppendLine(queue[i].Title);
                output.AppendLine("Запросил: ");
                output.AppendLine(queue[i].Requested);
                output.AppendLine(queue[i].Url);
                output.AppendLine();
            }
            await Context.Interaction.ModifyResponseAsync(x => x.Content = output.ToString());
        }
    }
}