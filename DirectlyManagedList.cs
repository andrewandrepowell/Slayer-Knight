using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    public interface DirectlyManagedInterface<T> 
    {
        public T ManagerObject { get; set; }
    }

    public interface ManagerInterface<T>
    {
        public void SetupFeature(T feature);
        public void DestroyFeature(T feature);
    }

    public class DirectlyManagedList<T1, T2> : IList<T1> 
        where T1 : DirectlyManagedInterface<T2> 
        where T2 : ManagerInterface<T1>
    {
        private List<T1> featureList;
        private T2 manager;
        private void setup(T1 item)
        {
            if (featureList.Contains(item))
                throw new Exception($"This DirectlyManagedList {this} already contains item {item}.");
            manager.SetupFeature(item);
            item.ManagerObject = manager;
        }
        private void destroy(T1 feature)
        {
            feature.ManagerObject = default;
            manager.DestroyFeature(feature);
        }
        public DirectlyManagedList(T2 manager)
        {
            featureList = new List<T1>();
            this.manager = manager;
        }
        public T1 this[int index] { get => featureList[index]; set => featureList[index] = value; }
        public int Count => featureList.Count;
        public bool IsReadOnly => (featureList as IList<T1>).IsReadOnly;
        public void Add(T1 item)
        {
            setup(item);
            featureList.Add(item);
        }
        public void Clear()
        {
            foreach (var feature in featureList)
                destroy(feature);
            featureList.Clear();
        }
        public bool Contains(T1 item) => featureList.Contains(item);
        public void CopyTo(T1[] array, int arrayIndex) => featureList.CopyTo(array, arrayIndex);
        public IEnumerator<T1> GetEnumerator() => featureList.GetEnumerator();
        public int IndexOf(T1 item) => featureList.IndexOf(item);
        public void Insert(int index, T1 item)
        {
            setup(item);
            featureList.Insert(index, item);
        }
        public bool Remove(T1 item)
        {
            var removed = featureList.Remove(item);
            if (removed)
                destroy(item);
            return removed;
        }
        public void RemoveAt(int index)
        {
            var item = this[index];
            destroy(item);
            Remove(item);
        }
        IEnumerator IEnumerable.GetEnumerator() => (featureList as IEnumerable).GetEnumerator();
    }
}
