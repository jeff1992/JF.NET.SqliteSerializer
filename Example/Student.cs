using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JF.NET.SqliteSerializer;

namespace Example
{
    public class Student : GObject
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                GDirty = true;
            }
        }

        private int age;

        public int Age
        {
            get { return age; }
            set
            {
                age = value;
                GDirty = true;
            }
        }

        private Sex sex;

        public Sex Sex
        {
            get { return sex; }
            set
            {
                sex = value;
                GDirty = true;
            }
        }

    }
}
