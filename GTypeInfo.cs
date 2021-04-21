using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;

namespace JF.Persistence
{

    internal class GTypeInfo
    {
        private GTypeInfo()
        {
        }

        static List<GTypeInfo> list = new List<GTypeInfo>();

        public Type Type;
        public string FullName;
        public string CreateTableSql;
        public string InsertSql;
        public string UpdateSql;
        public string DeleteSql;
        public FieldInfo[] Fields;
        
        public static GTypeInfo Get(Type type)
        {
            foreach (GTypeInfo gType in list)
                if (type == gType.Type) return gType;

            GTypeInfo gtype = new GTypeInfo();

            //设置Type
            gtype.Type = type;

            //获取FullName
            gtype.FullName = type.FullName;

            //获取支持存储的Fields
            var orgFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            gtype.Fields = (from t in orgFields where !t.IsNotSerialized && !t.Name.Contains('<') && SqliteSerialize.GetDbType(t.FieldType) != SqliteSerialize.DBFieldType.NONE select t).ToArray();
            //gtype.Fields = (from t in fields where !t.IsNotSerialized select t).ToArray();

            //标识是否为集合
            bool isCollection = typeof(IGCollectionBase).IsAssignableFrom(type);
            //            bool isCollection = type.(typeof(IGCollectionBase));

            //获取createTableSql
            gtype.CreateTableSql = string.Format(
                "CREATE TABLE '{0}' ({1})",
                gtype.FullName,
                string.Join(",", (from t in gtype.Fields select ("'" + t.Name + "' " + SqliteSerialize.GetDbType(t.FieldType)))) + (isCollection ? ",innerList TEXT" : "")
            );

            //获取InsertSql
            gtype.InsertSql = string.Format(
                "INSERT INTO '{0}' ({1}) VALUES ({2})",
                gtype.FullName,
                string.Join(",", (from t in gtype.Fields select $"'{t.Name}'")) + (isCollection ? ",innerList" : ""),
                string.Join(",", (from t in gtype.Fields select "@" + t.Name)) + (isCollection ? ",@innerList" : "")
            );

            //获取UpdateSql
            gtype.UpdateSql = string.Format(
                "UPDATE '{0}' SET {1} WHERE gid = @gid",
                gtype.FullName,
                string.Join(",", (from t in gtype.Fields select ($"'{t.Name}' = @{t.Name} "))) + (isCollection ? ",innerList=@innerList" : "")
            );

            //获取DeleteSql
            gtype.DeleteSql = "DELETE FROM '" + gtype.FullName + "' WHERE gid = @gid";

            

            list.Add(gtype);
            return gtype;
        }
    }
}
