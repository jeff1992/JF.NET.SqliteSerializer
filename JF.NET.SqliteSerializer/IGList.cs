using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JF.NET.SqliteSerializer
{
    public interface IGList : IList
    {
        void Add(IGObject item);
    }
}
