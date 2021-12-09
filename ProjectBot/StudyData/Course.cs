using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProjectBot.StudyData
{
    public class Course
    {
        

        public string Name { get; internal set; }

        public string Description { get; internal set; }

        public List<SchoolUser> Students { get; internal set; }

        public List<SchoolUser> Teachers { get; internal set; }

        public Dictionary<Group, GradeBook> Grades { get; internal set; }

        public DateTime StartDate { get; internal set; }

        public DateTime EndDate { get; internal set; } = DateTime.MinValue;


    }
}
