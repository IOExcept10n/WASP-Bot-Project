using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectBot.Modules
{
    /// <summary>
    /// Модуль для работы школьного бота
    /// </summary>
    public class SchoolModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Получает роль по названию.
        /// </summary>
        internal SocketRole GetRole(string name)
        {
            return (from role in (Context.Guild).Roles where role.Name == name select role).FirstOrDefault();//Получаем роль по названию.
        }
        /// <summary>
        /// Получает роль по названию, а в случае её отсутствия — создаёт её.
        /// </summary>
        async Task<IRole> GetOrCreateRole(string name)
        {
            SocketRole role;
            if ((role = GetRole(name)) == null) return await Context.Guild.CreateRoleAsync(name, isMentionable: false);
            return role;
        }
        /// <summary>
        /// Создаёт или получает категорию по названию.
        /// </summary>
        async Task<ICategoryChannel> GetOrCreateCategory(string name)
        {
            var category = (from cat in (Context.Guild).CategoryChannels where cat.Name == name select cat).FirstOrDefault();
            if (category == null) return await Context.Guild.CreateCategoryChannelAsync(name);
            return category;
        }

        /// <summary>
        /// Метод указания учителей.
        /// </summary>
        /// <param name="mentions">Список пользователей, которых надо добавить.</param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Alias("addteachers", "set_teachers", "add_teachers", "setteacher", "teacher", "addteacher", "ыуееуфсрук", "ыуе_еуфсрукы", "фвв_еуфсрук", "еуфсрук")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setteachers")]
        public async Task SetTeachers(params SocketGuildUser[] mentions)
        {
            IRole role;
            if ((role = GetRole("Teacher"))  == null)
            {
                role = await Context.Guild.CreateRoleAsync("Teacher", new GuildPermissions(manageChannels: true, manageMessages: true, manageNicknames: true, prioritySpeaker: true), isMentionable: true);
            }
            foreach (SocketGuildUser user in mentions)
            {
                if (!user.Roles.Contains(role)) await user.AddRoleAsync(role);
            }
            await ReplyAsync(":white_check_mark: Учитель(-я) успешно добавлены!");
        }

        /// <summary>
        /// Метод добавления учеников.
        /// </summary>
        /// <param name="mentions">Список учеников для добавления.</param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Alias("addstudents", "set_students", "add_students", "setstudent", "students", "addstudent", "ыуеыегвутеы", "фввыегвутеы", "фвв_ыегвутеы", "ыегвуте")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Command("setstudents")]
        public async Task SetStudents(params SocketGuildUser[] mentions)
        {
            IRole role;
            if ((role = GetRole("Student")) == null)
            {
                role = await Context.Guild.CreateRoleAsync("Student", new GuildPermissions(speak: true, sendMessages: true, viewChannel: true), isMentionable: true);
            }
            foreach (SocketGuildUser user in mentions)
            {
                if (!user.Roles.Contains(role)) await user.AddRoleAsync(role);
            }
            await ReplyAsync(":white_check_mark: Ученик(-и) успешно добавлен(-ы)!");
        }

        /// <summary>
        /// Метод добавления класса. 
        /// </summary>
        /// <param name="name">Имя класса.</param>
        /// <param name="mentions">Ученики для добавленияя в класс.</param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [Command("addclass")]
        public async Task AddClass(string name, params SocketGuildUser[] mentions)
        {
            var role = await GetOrCreateRole("[Class] "+name);
            foreach (var user in mentions)
            {   
                await user.AddRoleAsync(role);
                if (!user.Roles.Any(x => x == GetRole("Teacher")))
                {
                    await user.AddRoleAsync(GetRole("Student"));
                }
            }
            await ReplyAsync(":white_check_mark: Класс успешно добавлен!");
        }
        /// <summary>
        /// Метод для указания классного руководителя.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="teacher"></param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Alias("setclassroomteacher")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setsupervisor")]
        public async Task SetSupervisor(string className, SocketGuildUser teacher)
        {
            IRole role = GetRole("[Class] " + className);
            if (role == null)
            {
                await ReplyAsync(":x: Увы, но класса с данным названием не существует на этом сервере.");
                return;
            }
            await teacher.AddRoleAsync(role);
            role = GetRole($"{className}-teacher");
            if (role != null && (role as SocketRole).Members.Count() > 0)
            {
                await ReplyAsync(":x: Сожалею, но данная роль уже занята другим преподавателем :disappointed:");
                return;
            }
            else if (role == null)
            {
                role = await GetOrCreateRole($"{className}-teacher");
            }
            await teacher.AddRoleAsync(role);
            await ReplyAsync(":white_check_mark: Роль была добавлена успешно!");
        }
        /// <summary>
        /// Метод для снятия ролей классного руководителя.
        /// </summary>
        /// <param name="teacher"></param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Alias("removeclassroomteacher")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("removesupervisor")]
        public async Task RemoveSupervisor(SocketGuildUser teacher)
        {
            foreach (var role in teacher.Roles)
            {
                if (role.Name.EndsWith("-teacher") || role.Name.StartsWith("[Class] "))
                {
                    await teacher.RemoveRoleAsync(role);
                    await ReplyAsync(":white_check_mark: Роль была удалена успешно!");
                }
            }
        }
        /// <summary>
        /// Метод для удаления класса.
        /// </summary>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Alias("куьщмусдфыы")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("removeclass")]
        public async Task RemoveClass(string className = null)
        {
            if (className == null)
            {
                await ReplyAsync(":x: Кажется, вы забыли написать (или неверно ввели) класс, который хотите удалить :man_shrugging:");
                return;
            }
            var role = GetRole(className);
            if (role != null)
            {
                await role.DeleteAsync();
                await ReplyAsync(":white_check_mark: Роль указанного класса была удалена успешно!");
            }
            else
            {
                await ReplyAsync(":x: Упс, кажется я не могу найти указанную роль.");
                return;
            }
        }
        /// <summary>
        /// Метод для отчиления ученика.
        /// </summary>
        /// <param name="student">Ученик.</param>
        /// <param name="kick">Флаг дляя указания: кикать ученика или нет</param>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("expell")]
        public async Task Expell(SocketGuildUser student = null, bool kick = false)
        {
            await student.RemoveRolesAsync(from role in student.Roles where role.Name.StartsWith("[Class]") || role.Name == "Student" select role);
            if (kick && CommandServiceHandler.GetPersonalRole(Context.Client, Context.Guild).Permissions.KickMembers) await student.KickAsync();
            await ReplyAsync(":white_check_mark: Ученик успешно отчислен!");
        }
        /// <summary>
        /// Очищает все учебные данные.
        /// </summary>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("clearschool")]
        public async Task ClearSchool()
        {
            await GetRole("Student")?.DeleteAsync();
            await GetRole("Teacher")?.DeleteAsync();
            foreach (var role in (from role in Context.Guild.Roles where role.Name.StartsWith("[Class]") || role.Name.EndsWith("-teacher") select role))
            {
                await role.DeleteAsync();
            }
            await ReplyAsync(":white_check_mark: Все учебные роли успешно удалены, сервер очищен!");
        }
        /// <summary>
        /// Список пользователей голосового канала.
        /// </summary>
        [RequireContext(ContextType.Guild)]
        [Alias("list")]
        [Command("listchannel")]
        public async Task ListChannel()
        {
            var channel = Context.Guild.VoiceChannels.FirstOrDefault(x =>
            {
                return x.Users.Any(u => u.Id == Context.Message.Author.Id);
            });
            if (channel == null)
            {
                await ReplyAsync(":x: Вероятно, вы сейчас не находитесь в голосовом канале :man_shrugging:");
                return;
            }
            else
            {
                var users = channel.Users;
                var builder = new EmbedBuilder()
                        .WithAuthor(Context.Message.Author)
                        .WithDescription($":scroll: Список пользователей канала {channel.Name}")
                        .WithCurrentTimestamp();
                string administators = "", teachers = "", students = "", other = "";
                foreach (var user in users)
                {
                    if (user.GuildPermissions.Administrator)
                    {
                        administators += $"{user.Mention}\n";
                    }
                    else if (user.Roles.Any(x => x.Name == "Teacher"))
                    {
                        teachers += $"{user.Mention}\n";
                    }
                    else if (user.Roles.Any(x => x.Name == "Student"))
                    {
                        students += $"{user.Mention}\n";
                    }
                    else
                    {
                        other += $"{user.Mention}\n";
                    }
                };
                if (administators == "") administators = "Пользователи отсутствуют :person_shrugging:";
                if (teachers == "") teachers = "Пользователи отсутствуют :person_shrugging:";
                if (students == "") students = "Пользователи отсутствуют :person_shrugging:";
                if (other == "") other = "Пользователи отсутствуют :person_shrugging:";
                builder.AddField(":a: Администраторы: ", administators)
                    .AddField(":teacher: Учителя: ", teachers)
                    .AddField(":student: Ученики: ", students)
                    .AddField(":eyes: Прочие пользователи", other);
                var embed = builder.Build();
                await Context.Message.ReplyAsync(embed: embed);
            }
        }
        /// <summary>
        /// Создаёт персональный канал для класса.
        /// </summary>
        [RequireContext(ContextType.Guild)]
        [Command("classchannel")]
        public async Task MakeClassChannel(string className = null)
        {
            var c = Context.Guild.Roles.FirstOrDefault(x => x.Name.Replace("[Class] ", "") == className);
            if (c == null)
            {
                await ReplyAsync(":x: Кажется, вы забыли написать (или неверно ввели) класс, для которого хотите создать канал :man_shrugging:");
                return;
            }
            var channel = await Context.Guild.CreateTextChannelAsync($"{c.Name}", x =>
            {
                x.CategoryId = GetOrCreateCategory("classes").GetAwaiter().GetResult().Id;
                });
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
            await channel.AddPermissionOverwriteAsync(c, new OverwritePermissions(viewChannel: PermValue.Allow));
            await ReplyAsync(":white_check_mark: Класс успешно создан!");
        }
        /// <summary>
        /// Удаляет текстовый канал.
        /// </summary>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [Command("destroy", false)]
        public async Task Destroy()
        {
            var warningMessage = await Context.Message.ReplyAsync("Вы уверены в том, что хотите удалить текущий текстовый канал? Ответьте реакцией, если подтверждаете удаление.\n```diff\n-!ВНИМАНИЕ, ДАННОЕ ДЕЙСТВИЕ НЕОБРАТИМО!-\n```\nЕсли через 2 секунды реакция будет подтверждена, то канал будет удалён.");
            var react = new Emoji("🔥");
            await warningMessage.AddReactionAsync(react);
            await Task.Delay(2000);
            var reactions = await warningMessage.GetReactionUsersAsync(react, 3).FlattenAsync();
            if (reactions.Any(x => x.Id == Context.Message.Author.Id))
            {
                await Task.Delay(500);
                await (Context.Channel as SocketTextChannel).DeleteAsync();
            }
        }
        /// <summary>
        /// Команда для помощи.
        /// </summary>
        /// <returns></returns>
        [RequireContext(ContextType.Guild)]
        [Command("help")]
        public async Task Help()
        {
            var builder = new EmbedBuilder()
                .WithAuthor(Context.Message.Author)
                .WithColor(Color.Blue)
                .WithDescription("Список команд: ")
                .AddField("addteachers", "Добавляет роль учителя к одному или нескольким пользователям.")
                .AddField("addstudents", "Добавляет роль ученика к одному или нескольким пользователям.")
                .AddField("addclass", "Добавляет в класс учеников. Если такого класса не существует, создаёт его")
                .AddField("setsupervisor", "Добавляет классного руководителя для указанного класса. На один класс может быть только один классный руководитель.")
                .AddField("removesupervisor", "Снимает с пользователя роль классного руководителя.")
                .AddField("removeclass", "Удаляет указанный класс")
                .AddField("expell", "\"Отчисляет\" указанного пользователя. Опционально (при указании true) может ещё и кикнуть его.")
                .AddField("clearschool", "Очиащет все данные об учебных ролях для сервера.")
                .AddField("list (listchannel)", "Выводит список участников голосового канала, сортируя их по их учебной роли (Администратор/Учитель/Ученик/Остальные).")
                .AddField("destroy", "Удаляет канал. Даёт 2 секунды на подтверждение действия.");
            var embed = builder.Build();
            await Context.Message.ReplyAsync(embed: embed);
        }
    }
}
