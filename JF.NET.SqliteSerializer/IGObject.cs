using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JF.NET.SqliteSerializer
{
    public interface IGObject
    {
        bool GDirty { get; set; }

        int GVisitCount { get; set; }
        
        int GID { get; set; }    
    }
}
