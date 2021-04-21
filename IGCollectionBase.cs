using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
namespace JF.Persistence
{
    public interface IGCollectionBase
    {
        IList InnerList { get; }
    }
}
