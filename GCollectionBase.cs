using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.ComponentModel;

namespace JF.NET.SqliteSerializer
{
    /// <summary>
    /// 提供一个能够输出集合变化事件的基础类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class GCollectionBase<T> : IGObject, IList<T>, IEnumerable<T>, IGCollectionBase where T : GObject
    {
        #region Field
        [NonSerialized()]
        int gVisitCount = 0;
        [NonSerialized()]
        bool gDirty = true;
        protected int gid = -1;
        #endregion

        #region Property

        public int GVisitCount
        {
            get { return gVisitCount; }
            set { gVisitCount = value; }
        }
        public bool GDirty
        {
            get { return gDirty; }
            set { gDirty = value; }
        }
        public int GID
        {
            get { return gid; }
            set { gid = value; }
        }
        private List<T> innerList = new List<T>();

        public T this[int index]
        {
            get
            {
                return innerList[index] as T;
            }
            set
            {
                var oldItem = innerList[index];
                OnSet(index, innerList[index], value);
                innerList[index] = value;
                OnSetComplete(index, oldItem, value);
                this.GDirty = true;
                value.GDirty = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// 获取集合中实际包含的元素
        /// </summary>
        public int Count
        {
            get { return innerList.Count; }
        }
        #endregion

        /// <summary>
        /// 向集合内添加一个对象
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            var count = this.Count;
            OnInsert(count, item);
            innerList.Add(item);
            OnInsertComplete(count, item);
            this.GDirty = true;
            item.GDirty = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, count));
        }

        /// <summary>
        /// 向集合内批量添加对象
        /// </summary>
        /// <param name="array"></param>
        public void AddRange(T[] array)
        {
            foreach (var item in array)
                Add(item);
        }


        /// <summary>
        /// 从集合中移除指定的对象
        /// </summary>
        /// <param name="item"></param>
        public bool Remove(T item)
        {
            int index = InnerList.IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 移除集合中指定位置的对象
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var item = innerList[index];
            OnRemove(index, item);
            innerList.RemoveAt(index);
            OnRemoveComplete(index, item);
            this.GDirty = true;
            item.GDirty = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        /// <summary>
        /// 在集合指定位置插入一个对象
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            OnInsert(index, item);
            innerList.Insert(index, item);
            this.GDirty = true;
            item.GDirty = true;
            OnInsertComplete(index, item);
        }

        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            OnClear();
            foreach (var item in this)
                item.GDirty = true;
            innerList.Clear();
            OnClearComplete();
            this.GDirty = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0));
        }

        /// <summary>
        /// 判断集合是否包含某个对象
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        /// <summary>
        /// 返回集合中给定对象的索引
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        /// <summary>
        /// 从array的startIndex索引处开始，将本集合内的元素复制到array中
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        public void CopyTo(T[] array, int startIndex)
        {
            for (int i = 0; i < this.Count; i++)
            {
                array[i + startIndex] = this[i];
            }
        }

        void OnListChanged(ListChangedEventArgs args)
        {
            ListChanged?.Invoke(this, args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        protected virtual void OnClear() { }

        protected virtual void OnClearComplete() { }

        protected virtual void OnInsert(int index, T value) { }

        protected virtual void OnInsertComplete(int index, T value) { }

        protected virtual void OnRemove(int index, T value) { }

        protected virtual void OnRemoveComplete(int index, T value) { }

        protected virtual void OnSet(int index, T oldValue, T newValue) { }

        protected virtual void OnSetComplete(int index, T oldValue, T newValue) { }

        /// <summary>
        /// 集合发生变化时触发的事件
        /// </summary>
        public event ListChangedEventHandler ListChanged;

        /// <summary>
        /// 内部列表，请勿直接操作此对象
        /// </summary>
        public IList InnerList
        {
            get { return this.innerList; }
        }

        #region 实现IList<T>接口

        #endregion
    }
}
