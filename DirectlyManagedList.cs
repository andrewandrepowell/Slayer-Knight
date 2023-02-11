using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    public interface DirectlyManagedInterface<T>
    {
        public T ManagerObject { get; set; }
    }

    public class DirectlyManagedList<T1, T2> : IList<T1> where T1 : DirectlyManagedInterface<T2>
    {
        private List<T1> featureList;
        private T2 manager;
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
            item.ManagerObject = manager;
            featureList.Add(item);
        }
        public void Clear()
        {
            foreach (var feature in featureList)
                feature.ManagerObject = default;
            featureList.Clear();
        }
        public bool Contains(T1 item) => featureList.Contains(item);
        public void CopyTo(T1[] array, int arrayIndex) => featureList.CopyTo(array, arrayIndex);
        public IEnumerator<T1> GetEnumerator() => featureList.GetEnumerator();
        public int IndexOf(T1 item) => featureList.IndexOf(item);
        public void Insert(int index, T1 item)
        {
            item.ManagerObject = manager;
            featureList.Insert(index, item);
        }
        public bool Remove(T1 item)
        {
            var removed = featureList.Remove(item);
            if (removed)
                item.ManagerObject = default;
            return removed;
        }
        public void RemoveAt(int index)
        {
            var item = this[index];
            item.ManagerObject = default;
            Remove(item);
        }
        IEnumerator IEnumerable.GetEnumerator() => (featureList as IEnumerable).GetEnumerator();
    }
}
