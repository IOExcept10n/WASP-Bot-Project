using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBot.StudyData
{
    public class GradeBook
    {
        private Dictionary<SchoolUser, Grade[]> gradebook;

        public Dictionary<SchoolUser, Grade[]> Gradebook
        {
            get => new Dictionary<SchoolUser, Grade[]>(gradebook);
            internal set => gradebook = value;
        }

        public Course Course { get; }

        public Group Group { get; }

        public GradeBook(Course course, Group group)
        {
            Course = course;
            Group = group;
        }
    }
}
