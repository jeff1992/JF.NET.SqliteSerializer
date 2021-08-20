using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JF.NET.SqliteSerializer;

namespace Example
{
    public class Teacher : GObject
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
    }
}
