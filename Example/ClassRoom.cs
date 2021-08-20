using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JF.NET.SqliteSerializer;

namespace Example
{
    public class ClassRoom : GObject
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                GDirty = true;  //dirty this
            }
        }

        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                GDirty = true;
            }
        }

        private GList<Student> students;

        public GList<Student> Students
        {
            get { return students; }
            set
            {
                students = value;
                GDirty = true;
            }
        }

        private Teacher teacher;

        public Teacher Teacher
        {
            get { return teacher; }
            set
            {
                teacher = value;
                GDirty = true;
            }
        }

    }
}
