using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace ProjectBot.StudyData
{
    public class SchoolUser
    {
        internal List<Course> courses;

        SocketUser User { get; set; }

        public List<Course> Courses 
        { 
            get => new List<Course>(courses); 
            set => courses = value;
        }

        public SchoolUser(SocketUser user)
        {
            User = user;
        }
    }
}
