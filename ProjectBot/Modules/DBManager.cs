using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ProjectBot.Modules
{
    /// <summary>
    /// Класс, регулирующий работу с базами данных для серверов.
    /// </summary>
    internal static class DBManager
    {
        public static bool IsInitialized { get; private set; }
    }
}
