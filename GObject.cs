using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using Newtonsoft.Json;

namespace JF.NET.SqliteSerializer
{
    /// <summary>
    ///  为可采用sqlite序列化的对象提供一个基类
    /// </summary>
    [Serializable]
    public class GObject : IGObject
    {
        #region Constructor
        protected GObject() { }
        #endregion

        #region Fields
        [NonSerialized()]
        bool gGDirty = true;
        [NonSerialized()]
        internal int gVisitCount = 0;
        protected int gid = -1;
        #endregion

        #region Property
        /// <summary>
        /// 获取或设置一个值，指示对象是否被修改
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool GDirty
        {
            get { return gGDirty; }
            set
            {
                gGDirty = value;
            }
        }

        [Browsable(false)]
        [JsonIgnore]
        public int GVisitCount
        {
            get { return gVisitCount; }
            set { gVisitCount = value; }
        }
        /// <summary>
        /// 获取对象的唯一识别ID
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public int GID
        {
            get { return gid; }
            set { gid = value; }
        }
        #endregion

    }

}
