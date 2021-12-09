using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace ProjectBot.StudyData
{
    public class School
    {
        public List<Course> Courses { get; internal set; }

        public string Name { get; internal set; }

        SocketGuild SocketGuild { get;  set; }

        public School(string name, SocketGuild guild)
        {
            Name = name;
            SocketGuild = guild;
        }
    }
}
