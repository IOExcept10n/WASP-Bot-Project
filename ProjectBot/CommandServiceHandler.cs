using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;

namespace ProjectBot
{
    class CommandServiceHandler
    {
        private CommandService commands;

        private DiscordSocketClient client;

        private IServiceProvider services;

        public static bool DebugMode { get; set; } = false;

        public CommandServiceHandler(IServiceProvider services)
        {
            this.services = services;
            commands = services.GetRequiredService<CommandService>();
            client = services.GetRequiredService<DiscordSocketClient>();

            commands.CommandExecuted += OnCommandExecuted;
            client.MessageReceived += OnMessageRecieved;
        }

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.SetGameAsync("Mini-Project");
        }

        private async Task OnMessageRecieved(SocketMessage sourceMessage)
        {
            if (sourceMessage.Source != MessageSource.User) return;
            if (sourceMessage is SocketUserMessage msg)
            {
                int argPos = 0;
                if (msg.HasCharPrefix('!', ref argPos) || msg.HasMentionPrefix(client.CurrentUser, ref argPos))
                {
                    var context = new SocketCommandContext(client, msg);
                    bool couldWrite = false;
                    foreach (var r in context.Guild.GetUser(client.CurrentUser.Id).Roles)
                    {
                        couldWrite |= context.Guild.GetChannel(context.Channel.Id).GetPermissionOverwrite(r)?.SendMessages != PermValue.Deny;
                    }
                    if (couldWrite)
                        await commands.ExecuteAsync(context, argPos, services);
                }
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                await context.Message.ReplyAsync("Пожалуйста, проверьте правильность команды и/или аргументов.");
                if (DebugMode && context.Message.Author.Id == 638653302040428544) await context.Message.ReplyAsync($"{result.ErrorReason}");
            }
        }

        ~CommandServiceHandler()
        {
            client.LogoutAsync();
        }
    }
}