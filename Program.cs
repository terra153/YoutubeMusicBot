using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using YoutubeMusicBot.Modules;
using YoutubeMusicBot.Services;


//Получаем конфигурацию
var config = new ConfigurationBuilder()
    .AddJsonFile(Directory.GetCurrentDirectory() + "/settings.json", optional: false)
    .Build();


//Создаём клиент
var _client = new GatewayClient(new BotToken(config["BotToken"]!), new GatewayClientConfiguration()
{
    Intents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates,
    Logger = new ConsoleLogger()
});

//Регистрируем сервисы
var services = new ServiceCollection();
services.AddSingleton<IQueueClientCache, QueueClientCache>();
services.AddSingleton<IYoutubeDownloader, YoutubeDownloader>();
services.AddSingleton(_client);
var serviceProvider = services.BuildServiceProvider();

//Регистрируем команды
ApplicationCommandService<ApplicationCommandContext> applicationCommandService = new();
applicationCommandService.AddModules(typeof(DefaultCommandModule).Assembly);
await applicationCommandService.RegisterCommandsAsync(_client.Rest, _client.Id);

//Выполняем команду и передаём сервисы
_client.InteractionCreate += async interaction =>
{
    if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
        return;

    var result = await applicationCommandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, _client), serviceProvider);

    if (result is not IFailResult failResult)
        return;

    try
    {
        await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
    }
    catch
    {
    }
};

//Старт
await _client.StartAsync();

//Чтобы консоль не закрывалась 
await Task.Delay(-1);