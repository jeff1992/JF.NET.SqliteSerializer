using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace JF.NET.SqliteSerializer
{
    public class GBindingList<T> : GObject, IEnumerable<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents, IGList where T : class, IGObject
    {
        BindingList<T> innerList = new BindingList<T>();

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

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int ICollection.Count => innerList.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => false;

        bool IRaiseItemChangedEvents.RaisesItemChangedEvents => true;

        bool IBindingList.AllowEdit => true;

        bool IBindingList.AllowNew => true;

        bool IBindingList.AllowRemove => true;

        bool IBindingList.IsSorted => false;

        ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;

        PropertyDescriptor IBindingList.SortProperty => null;

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => false;

        bool IBindingList.SupportsSorting => false;

        public event ListChangedEventHandler ListChanged
        {
            add
            {
                innerList.ListChanged += value;
            }
            remove
            {
                innerList.ListChanged -= value;
            }
        }

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

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            ((IBindingList)innerList).AddIndex(property);
        }

        object IBindingList.AddNew()
        {
            return ((IBindingList)innerList).AddNew();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            ((IBindingList)innerList).ApplySort(property, direction);
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            return ((IBindingList)innerList).Find(property, key);
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            ((IBindingList)innerList).RemoveIndex(property);
        }

        void IBindingList.RemoveSort()
        {
            ((IBindingList)innerList).RemoveSort();
        }

        void ICancelAddNew.CancelNew(int itemIndex)
        {
            throw new NotImplementedException();
        }

        void ICancelAddNew.EndNew(int itemIndex)
        {
            throw new NotImplementedException();
        }
    }
}
