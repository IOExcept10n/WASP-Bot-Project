using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace ProjectBot
{
    /// <summary>
    /// Класс-обработчик команд для бота.
    /// </summary>
    class CommandServiceHandler
    {
        /// <summary>
        /// Сервис для работы с командами.
        /// </summary>
        private CommandService commands;
        /// <summary>
        /// Клиент дискорда для работы с API.
        /// </summary>
        private DiscordSocketClient client;
        /// <summary>
        /// Объект, предоставляющий остальные необходимые сервисы.
        /// </summary>
        private IServiceProvider services;
        /// <summary>
        /// Режим отладки, т.е. режим, при котором бот будет выводить данные об ошибке при её возникновении.
        /// </summary>
        internal static bool DebugMode { get; set; } = false;
        /// <summary>
        /// Конструктор класса, в котором настраиваются сервисы и привязываются обработчики событий.
        /// </summary>
        /// <param name="services"></param>
        public CommandServiceHandler(IServiceProvider services)
        {
            this.services = services;
            commands = services.GetRequiredService<CommandService>();
            client = services.GetRequiredService<DiscordSocketClient>();

            commands.CommandExecuted += OnCommandExecuted;
            client.MessageReceived += OnMessageRecieved;
        }
        /// <summary>
        /// Метод инициализации, производит загрузку модулей для обработки команд, а также настраивает отображаемый ботом статус игры.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await client.SetGameAsync("Mini-Project");
        }
        /// <summary>
        /// Обработчик на событие получения сообщения. Вызывается очень часто, поэтому нужно исполнять команды только при соблюдении условий с целью оптимизации.
        /// </summary>
        /// <param name="sourceMessage">Полученное сообщение с дополнительными данными об окружении.</param>
        /// <returns></returns>
        private async Task OnMessageRecieved(SocketMessage sourceMessage)
        {
            if (sourceMessage.Source != MessageSource.User) return;
            if (sourceMessage is SocketUserMessage msg)
            {
                int argPos = 0;
                if (msg.HasCharPrefix('!', ref argPos) || msg.HasMentionPrefix(client.CurrentUser, ref argPos))//Если мы тегнули бота или команда начинается на "!", то он реагирует на команду.
                {
                    var context = new SocketCommandContext(client, msg);
                    bool couldWrite = false;
                    foreach (var r in context.Guild.GetUser(client.CurrentUser.Id).Roles)//Проверяем, разрешено ли боту писать в канале. В большинстве случаев у него есть такое право, но для определённые
                    {
                        couldWrite |= context.Guild.GetChannel(context.Channel.Id).GetPermissionOverwrite(r)?.SendMessages != PermValue.Deny;
                    }
                    if (couldWrite)
                        await commands.ExecuteAsync(context, argPos, services);
                }
            }
        }
        /// <summary>
        /// Получает персональную одноимённую роль бота с помощью <see cref="System.Linq"/>-запроса.
        /// </summary>
        internal static SocketRole GetPersonalRole(DiscordSocketClient client, SocketGuild guild)
        {
            return (from role in guild.Roles where role.Name == client.CurrentUser.Username select role).First();//Получаем роль, принадлежащую боту.
        }
        /// <summary>
        /// Обработчик, срабатывающий после исполнения команды.
        /// </summary>
        /// <param name="info">Информация о команде (никогда не находил в ней хоть что-то полезное).</param>
        /// <param name="context">Контекст запроса для команды.</param>
        /// <param name="result">Результат выполнения команды (успешный/неуспешный).</param>
        /// <returns></returns>
        private async Task OnCommandExecuted(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.IsSpecified)
            {
                await context.Message.ReplyAsync(":x: Пожалуйста, проверьте правильность команды и/или аргументов.");
                if (context.Message.Author.Id == 638653302040428544) await context.Message.ReplyAsync($"Текст ошибки: {result.ErrorReason}");
            }
        }
        /// <summary>
        /// Выходим из аккаунта после завершения работы программы (так, на всякий случай).
        /// </summary>
        ~CommandServiceHandler()
        {
            client.LogoutAsync();
        }
    }
}