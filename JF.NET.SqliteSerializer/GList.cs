using System;
using System.Collections;
using System.Collections.Generic;

namespace JF.NET.SqliteSerializer
{
    public class GList<T> : GObject, IList<T>, IGList where T : class, IGObject
    {
        protected List<T> innerList = new List<T>();

        public T this[int index]
        {
            get => innerList[index];
            set
            {
                if (value == innerList[index]) return;
                innerList[index] = value;
                GDirty = true;
            }

        }
        object IList.this[int index]
        {
            get => ((IList)innerList)[index];
            set
            {
                if (value == innerList[index]) return;
                ((IList)innerList)[index] = value;
                GDirty = true;
            }
        }

        public int Count => innerList.Count;

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int ICollection.Count => innerList.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => false;

        public void Add(T item)
        {
            innerList.Add(item);
            GDirty = true;
        }

        public void Clear()
        {
            innerList.Clear();
            GDirty = true;
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
            GDirty = true;
        }

        public bool Remove(T item)
        {
            return GDirty = innerList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
            GDirty = true;
        }

        int IList.Add(object value)
        {
            var ret = ((IList)innerList).Add(value);
            GDirty = true;
            return ret;
        }

        void IList.Clear()
        {
            ((IList)innerList).Clear();
            GDirty = true;
        }

        bool IList.Contains(object value)
        {
            return ((IList)innerList).Contains(value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)innerList).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }


        int IList.IndexOf(object value)
        {
            return ((IList)innerList).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)innerList).Insert(index, value);
            GDirty = true;
        }

        void IList.Remove(object value)
        {
            ((IList)innerList).Remove(value);
            GDirty = true;
        }

        void IList.RemoveAt(int index)
        {
            ((IList)innerList).RemoveAt(index);
            GDirty = true;
        }

        void IGList.Add(IGObject item)
        {
            innerList.Add(item as T);
        }
    }
}
