using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Net.Http;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ProjectBot
{
    class Program
    {
        private DiscordSocketClient socketClient;

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())//Директива using. Позволяет с помощью временной переменной делать какие-либо действия.
            {
                socketClient = services.GetRequiredService<DiscordSocketClient>();
                socketClient.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                // Tokens should be considered secret data, and never hard-coded.
                var values = ReadJSON(".env");
                string token = values["token"];
                await socketClient.LoginAsync(TokenType.Bot, token);
                await socketClient.StartAsync();

                await services.GetRequiredService<CommandServiceHandler>().InitializeAsync();
                // Block the program until it is closed.
                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage logMsg)
        {
            Console.WriteLine(logMsg);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection().AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandServiceHandler>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private static Dictionary<string, string> ReadJSON(string path)
        {
            Dictionary<string, string> ret = null;
            StreamReader stream = new StreamReader(path);
            try
            {
                string text = stream.ReadToEnd();
                object t = JsonConvert.DeserializeObject(text, typeof(Dictionary<string, string>));
                ret = t as Dictionary<string, string>;
            }
            finally
            {
                stream.Close();
            }
            return ret;
        }
    }
}
