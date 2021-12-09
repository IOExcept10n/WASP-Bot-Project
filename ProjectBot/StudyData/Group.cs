using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBot.StudyData
{
    public class Group
    {
        public School School { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public SchoolUser GroupSupervisor { get; set; }

        public List<SchoolUser> Students { get; set; }
    }
}
