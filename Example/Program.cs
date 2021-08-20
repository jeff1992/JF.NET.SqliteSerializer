using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JF.NET.SqliteSerializer;
using System.IO;
using System.Diagnostics;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var classRoom = new ClassRoom()
            {
                Name = "1"
            };
            classRoom.Teacher = new Teacher()
            {
                Name = "Jeff"
            };
            classRoom.Students = new GList<Student>();
            for (var i = 0; i < 100000; i++)
            {
                classRoom.Students.Add(new Student
                {
                    Name = "Student" + i,
                    Age = 18,
                    Sex = Sex.Mail
                });
            }

            var fileName = "data.db";
            if (File.Exists(fileName))
                File.Delete(fileName);

            Console.WriteLine("serializing...");
            var watch = new Stopwatch();
            watch.Start();
            var sqliteSerialize = new SqliteSerialize(new SQLiteHelper($"Data Source={fileName}"));
            sqliteSerialize.Serialize(classRoom);
            Console.WriteLine($"serialized success! cost; {watch.ElapsedMilliseconds}ms");

            Console.WriteLine("deserializing...");
            watch.Reset();
            watch.Start();
            var serialize = new SqliteSerialize(new SQLiteHelper($"Data Source={fileName}"));
            var objects = serialize.Deserialize(typeof(ClassRoom).Assembly);
            watch.Stop();
            Console.WriteLine($"deserialize success! cost; {watch.ElapsedMilliseconds}ms");

            var entry = objects.FirstOrDefault(m => m is ClassRoom) as ClassRoom;
            if (entry != null)
            {
                Console.WriteLine($"class room name: {entry.Name}");
                Console.WriteLine($"class room students: {entry.Students.Count}");
                Console.WriteLine($"class room teacher: {entry.Teacher.Name}");
            }
            Console.ReadLine();
        }
    }
}
