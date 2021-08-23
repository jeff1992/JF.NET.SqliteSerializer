using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace JF.NET.SqliteSerializer
{
    /// <summary>
    /// 序列化工作器
    /// </summary>
    public class SqliteSerialize
    {
        #region Members

        SQLiteHelper sqliteHelper;
        List<IGObject> loadedObj = new List<IGObject>();
        List<IGObject> savedObj = new List<IGObject>();
        int maxGID = 1000;
        List<string> dataTables = new List<string>(20);
        IGObject[] linkArr = new IGObject[9999999];
        int currentVisited = 0;
        bool isDirty;
        Regex genTypeRegex = new Regex(@"(\S+)\[(?:\[(\S+),[\S ]+\])+\]");
        #endregion

        #region Constructor

        public SqliteSerialize(SQLiteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
        }

        #endregion

        #region Methods

        [DllImport("gdi32")]
        public static extern int GetEnhMetaFileBits(int hemf, int cbBuffer, byte[] lpbBuffer);

        /// <summary>
        /// 获取一个唯一的GID
        /// </summary>
        /// <returns></returns>
        int GetGID()
        {
            return ++maxGID;
        }

        /// <summary>
        /// 从一条row反序列化gobj对象
        /// </summary>
        /// <param name="gobj"></param>
        /// <param name="row"></param>
        void Deserialize(IGObject gobj, DataRow row)
        {
            var fields = GTypeInfo.Get(gobj.GetType()).Fields;
            
            var columns = row.Table.Columns;
            for (int columIndex = 0; columIndex < columns.Count; columIndex++)
            {
                if (row[columIndex] == DBNull.Value) continue;

                if (columIndex < fields.Length && columns[columIndex].ColumnName == fields[columIndex].Name) //根据columIndex快速匹配
                {
                    var field = fields[columIndex];
                    SetValue(gobj, field, row[columIndex]);
                }
                else
                {
                    for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
                    {
                        if (fields[fieldIndex].Name == columns[columIndex].ColumnName)
                        {
                            var field = fields[fieldIndex];
                            SetValue(gobj, field, row[columIndex]);
                        }
                    }
                }
            }
            
            if (gobj is IGList)
            {
                var collection = gobj as IGList;
                var str = row["innerList"].ToString();
                if (str.Length == 0) return;
                var arr = str.Split('|');
                for (int i = 0; i < arr.Length; i++)
                {
                    int id = Convert.ToInt32(arr[i]);
                    collection.Add(linkArr[id]);
                    /*
                    foreach (IGObject obj in loadedObj)
                    {
                        if (obj.GID == id)
                        {
                            collection.Add(obj);
                            break;
                        }
                    }*/
                }
            }
        }

        /// <summary>
        /// 获取一个值，表示当前序列化管理的对象是否是脏的
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (isDirty)
                    return isDirty;
                isDirty = loadedObj.Any(m => m.GDirty);
                return isDirty;
            }
        }

        void InnerSerialize(IGObject obj)
        {
            if (obj.GVisitCount == currentVisited) return;
            obj.GVisitCount = currentVisited;
            savedObj.Add(obj);
            bool create = obj.GID == -1;
            if (create) obj.GID = GetGID();
            var typeInfo = GTypeInfo.Get(obj.GetType());
            //获取字段值
            bool isList = obj is IGList;
            SQLiteParameter[] pts = new SQLiteParameter[typeInfo.Fields.Length + (isList ? 1 : 0)];
            
            //为每个字段值转换成数据库能接受的值
            for (int i = 0; i < typeInfo.Fields.Length; i++)
            {
                var field = typeInfo.Fields[i];
                var fieldType = field.FieldType;
                var fieldValue = field.GetValue(obj);
                object dbFieldValue = null;

                #region 值转换
                if (fieldValue == null) dbFieldValue = DBNull.Value;
                else if (fieldType == typeof(UInt16)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(UInt32)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(UInt64)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(Int16)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(Int32)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(Int64)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(bool)) dbFieldValue = fieldValue;
                else if (fieldType.IsEnum) dbFieldValue = fieldValue;
                else if (typeof(IGObject).IsAssignableFrom(fieldType))
                {
                    var gitem = fieldValue as IGObject;
                    if (gitem.GVisitCount < currentVisited) InnerSerialize(gitem);
                    dbFieldValue = gitem.GID;
                }
                else if (fieldType == typeof(string)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(Single) || fieldType == typeof(Double) || fieldType == typeof(Decimal)) dbFieldValue = fieldValue;
                else if (fieldType == typeof(Point)) { Point p = (Point)fieldValue; dbFieldValue = string.Format("{0},{1}", p.X, p.Y); }
                else if (fieldType == typeof(Size)) { Size s = (Size)fieldValue; dbFieldValue = string.Format("{0},{1}", s.Width, s.Height); }
                else if (fieldType == typeof(Rectangle)) { Rectangle r = (Rectangle)fieldValue; dbFieldValue = string.Format("{0},{1},{2},{3}", r.X, r.Y, r.Width, r.Height); }
                //else if (fieldValue is Image)
                //{
                //    MemoryStream ms = new MemoryStream();
                //    BinaryFormatter b = new BinaryFormatter();
                //    b.Serialize(ms, fieldValue);
                //    dbFieldValue = ms.GetBuffer();
                //    ms.Close();
                //}
                #endregion
                pts[i] = new SQLiteParameter("@" + typeInfo.Fields[i].Name, dbFieldValue);
            }
            //如果是集合则额外添加一个字段
            if (isList)
            {
                var list = obj as IGList;
                StringBuilder strBuilder = new StringBuilder(list.Count * 2);
                foreach (object item in list)
                {
                    IGObject gitem = item as IGObject;
                    if (gitem == null) continue;
                    if (gitem.GVisitCount < currentVisited) InnerSerialize(gitem);
                    strBuilder.Append(gitem.GID);
                    strBuilder.Append('|');
                }
                if (strBuilder.Length > 0) strBuilder.Remove(strBuilder.Length - 1, 1);
                pts[pts.Length - 1] = new SQLiteParameter("@innerList", strBuilder.ToString());
            }
            //添加或者修改记录
            if (create)
            {
                if (!dataTables.Contains(typeInfo.FullName))
                {
                    sqliteHelper.Execute(typeInfo.CreateTableSql);
                    dataTables.Add(typeInfo.FullName);
                }
                sqliteHelper.Execute(typeInfo.InsertSql, pts);
                obj.GDirty = false;
            }
            else if (obj.GDirty)
            {
                sqliteHelper.Execute(typeInfo.UpdateSql, pts);
                obj.GDirty = false;
            }
        }

        /// <summary>
        /// 序列化obj及其所有相关的可序列化对象到文件
        /// </summary>
        /// <param name="obj"></param>
        public void Serialize(IGObject obj)
        {
            currentVisited++;                   //更新对象目标访问次数
            sqliteHelper.BeginTransaction();    //开始事务
            savedObj.Clear();                   //清空保存的对象
            InnerSerialize(obj);                //序列化对象
            var delObj = loadedObj.Except(savedObj);    //获取需要删除的对象
            foreach (IGObject o in delObj)
            {
                sqliteHelper.Execute(GTypeInfo.Get(o.GetType()).DeleteSql, new SQLiteParameter("@gid", o.GID));
            }
            sqliteHelper.Commit();              //提交事务
            loadedObj = savedObj;               //更新以保存对象列表
            var dirties = loadedObj.Where(m => m.GDirty).ToList();
            isDirty = false;
        }

        #region Deserialize
        
        /// <summary>
        /// 从文件反序列化得到所有对象
        /// </summary>
        /// <returns></returns>
        public IGObject[] Deserialize(Assembly assembly)
        {
            dataTables.Clear();
            //获取所有表名称
            using(DataSet ds = sqliteHelper.GetDs("SELECT name FROM sqlite_master WHERE type = 'table'"))
            {
                dataTables = ds.Tables[0].Rows.Cast<DataRow>()
                            .Select(m => m[0].ToString())
                            .ToList();
            }
            var types = new List<Type>();
            foreach(var name in dataTables)
            {
                Type finalType = null;
                var match = genTypeRegex.Match(name);
                if (match.Success)
                {
                    var genType = this.GetType().Assembly.GetType(match.Groups[1].Value);
                    if (genType == null)
                        genType = assembly.GetType(match.Groups[1].Value);
                    if (genType == null)
                    {
                        throw new InvalidDataException($"can not find type: {match.Groups[1].Value}");
                    }
                    else
                    {
                        var entityTypes = new Type[match.Groups.Count - 2];
                        for (var i = 0; i < match.Groups.Count - 2; i++)
                        {
                            entityTypes[i] = assembly.GetType(match.Groups[i + 2].Value);
                        }
                        finalType = genType.MakeGenericType(entityTypes.ToArray());
                    }
                }
                else
                {
                    finalType = assembly.GetType(name);
                }

                if (finalType == null)
                    throw new InvalidDataException($"can not find type: {name}");

                types.Add(finalType);
            }

            //获取所有表内容
            using (DataSet ds = sqliteHelper.GetDs(string.Join(";", (from t in dataTables select string.Format("SELECT * FROM '{0}'", t)).ToArray())))
            {
                loadedObj = new List<IGObject>(ds.Tables.Count * 10);

                //反射创建每个独立实例
                for (var i = 0; i < ds.Tables.Count; i++)
                {
                    ds.Tables[i].TableName = dataTables[i];
                    foreach (DataRow row in ds.Tables[i].Rows)
                    {
                        var obj = System.Activator.CreateInstance(types[i]) as IGObject;
                        var gid = Convert.ToInt32(row["gid"]);
                        obj.GID = gid;
                        obj.GDirty = false;
                        loadedObj.Add(obj);
                        linkArr[gid] = obj;
                    }
                }

                //将字段赋值给实例，并创建关联引用
                var index = 0;
                for (var i = 0; i < ds.Tables.Count; i++)
                {
                    foreach (DataRow row in ds.Tables[i].Rows)
                    {
                        Deserialize(loadedObj[index++], row);
                    }
                }

                //校验数据库字段是否一致，并修正
                bool updateNeed = false;
                for (int i = 0; i < dataTables.Count; i++)
                {
                    var columns = ds.Tables[dataTables[i]].Columns;
                    Type type = types[i];
                    foreach (FieldInfo field in GTypeInfo.Get(type).Fields)
                    {
                        if (!columns.Contains(field.Name))
                        {
                            if (!updateNeed)
                            {
                                sqliteHelper.BeginTransaction(); 
                                updateNeed = true;
                            }
                            sqliteHelper.Execute(string.Format("ALTER TABLE '{0}' ADD COLUMN '{1}' {2}", dataTables[i], field.Name, GetDbType(field.FieldType)));
                        }
                    }
                }
                if (updateNeed) sqliteHelper.Commit();
            }
            return loadedObj.ToArray();
        }

        #endregion


        #endregion

        /// <summary>
        /// 获取一个类型对应在sqlite中存储的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DBFieldType GetDbType(Type type)
        {
            if (type == typeof(UInt16)) return DBFieldType.INTEGER;
            if (type == typeof(UInt32)) return DBFieldType.INTEGER;
            if (type == typeof(UInt64)) return DBFieldType.INTEGER;
            if (type == typeof(Int16)) return DBFieldType.INTEGER;
            if (type == typeof(Int32)) return DBFieldType.INTEGER;
            if (type == typeof(Int64)) return DBFieldType.INTEGER;
            if (type == typeof(bool)) return DBFieldType.INTEGER;
            if (type.IsEnum) return DBFieldType.TEXT;
            if (typeof(IGObject).IsAssignableFrom(type)) return DBFieldType.INTEGER;
            if (type == typeof(string)) return DBFieldType.TEXT;
            if (type == typeof(Single) || type == typeof(Double) || type == typeof(Decimal)) return DBFieldType.REAL;
            if (type == typeof(Point)) return DBFieldType.TEXT;
            if (type == typeof(Size)) return DBFieldType.TEXT;
            if (type == typeof(Rectangle)) return DBFieldType.TEXT;
            //if (type == typeof(Image)) return DBFieldType.BLOB;
            return DBFieldType.NONE;
        }

        /// <summary>
        /// 设置gobj对象field字段的值value
        /// </summary>
        /// <param name="gobj"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        void SetValue(IGObject gobj, FieldInfo field, object value)
        {
            Type fieldType = field.FieldType;
            if (fieldType == value.GetType())
            {
                field.SetValue(gobj, value);
                return;
            }

            if (fieldType == typeof(bool))
                field.SetValue(gobj, Convert.ToBoolean(value));
            else if (fieldType == typeof(Int16))
                field.SetValue(gobj, Convert.ToInt16(value));
            else if (fieldType == typeof(Int32))
                field.SetValue(gobj, Convert.ToInt32(value));
            else if (fieldType == typeof(Int64))
                field.SetValue(gobj, Convert.ToInt64(value));
            else if (fieldType == typeof(UInt16))
                field.SetValue(gobj, Convert.ToUInt16(value));
            else if (fieldType == typeof(UInt32))
                field.SetValue(gobj, Convert.ToUInt32(value));
            else if (fieldType == typeof(UInt64))
                field.SetValue(gobj, Convert.ToUInt64(value));
            else if (fieldType == typeof(string))
                field.SetValue(gobj, Convert.ToString(value));
            else if (fieldType == typeof(Point))
            {
                var arr = value.ToString().Split(',');
                field.SetValue(gobj, new Point(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1])));
            }
            else if (fieldType == typeof(Size))
            {
                var arr = value.ToString().Split(',');
                field.SetValue(gobj, new Size(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1])));
            }
            else if (fieldType == typeof(Rectangle))
            {
                var arr = value.ToString().Split(',');
                field.SetValue(gobj, new Rectangle(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1]), Convert.ToInt32(arr[2]), Convert.ToInt32(arr[3])));
            }
                /*
            else if (fieldType == typeof(Image))
            {
                MemoryStream stream = new MemoryStream((byte[])value);
                field.SetValue(gobj, Image.FromStream(stream));
            }
                 * */
            //else if (fieldType == typeof(Image))
            //{
            //    MemoryStream stream = new MemoryStream((byte[])value);
            //    BinaryFormatter b = new BinaryFormatter();
            //    var ddd = b.Deserialize(stream);
            //    field.SetValue(gobj, ddd);
            //    stream.Close();
            //}
            //else if (fieldType == typeof(System.Drawing.Imaging.Metafile))
            //{
            //    MemoryStream stream = new MemoryStream((byte[])value);
            //    BinaryFormatter b = new BinaryFormatter();
            //    var ddd = b.Deserialize(stream);
            //    field.SetValue(gobj, ddd);
            //    stream.Close();
            //}
            else if ((typeof(IGObject)).IsAssignableFrom(fieldType)) //GObject派生类
            {
                int gid = Convert.ToInt32(value);
                //检查引用类型是否正确
                if (linkArr[gid] != null)
                {
                    if (fieldType.IsAssignableFrom(linkArr[gid].GetType()))
                    {
                        field.SetValue(gobj, linkArr[gid]);
                    }
                    else
                    {
                        throw new Exception(string.Format("{0}[gid={1}]{2}字段指向了一个错误的引用{3}[gid={4}]", gobj.GetType().Name, gobj.GID, field.Name, linkArr[gid].GetType(), gid));
                    }
                }
            }
            else if (fieldType.IsEnum)
            {
                field.SetValue(gobj, Convert.ToInt32(value));
            }
        }

        public enum DBFieldType
        {
            TEXT,
            BLOB,
            NUMERIC,
            INTEGER,
            REAL,
            NONE
        }
    }
}
