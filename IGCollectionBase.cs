using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
namespace JF.NET.SqliteSerializer
{
    public interface IGCollectionBase
    {
        IList InnerList { get; }
    }
}
