using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public interface OutputInterface<T>
    {
        public void Enqueue(T item);
    }
    public interface InputInterface<T>
    {
        public T Dequeue();
        public int Count { get; }
    }
    public class Broadcaster<T>: OutputInterface<T>
    {
        public List<Channel<T>> Channels { get; private set; } = new List<Channel<T>>();
        public void Enqueue(T item)
        {
            foreach (var channel in Channels)
                channel.Enqueue(item);
        }
    }
    public class Channel<T> : IEnumerable<T>, IReadOnlyCollection<T>, ICollection, IEnumerable, InputInterface<T>, OutputInterface<T>
    {
        private T[] buffer;
        private int head;
        private int tail;
        public Channel(int capacity = 1)
        {
            head = 0;
            tail = 0;
            buffer = new T[capacity + 1];
        }
        public int Count { get => (head >= tail) ? head - tail : buffer.Length - tail + head; }
        public bool IsSynchronized { get => false; }
        public object SyncRoot { get => throw new NotImplementedException(); }
        public void CopyTo(Array array, int index)
        {
            int curr = head;
            while (curr != tail)
            {
                array.SetValue(value:buffer[curr], index:index);
                index++;
                curr = (curr + 1) % buffer.Length;
            }
        }
        public void Enqueue(T item)
        {
            int next = (head + 1) % buffer.Length;
            if (next == tail)
                throw new Exception("channel full.");
            buffer[head] = item;
            head = next;
        }
        public T Dequeue()
        {
            if (head == tail)
                throw new Exception("channel empty.");
            T item = buffer[tail];
            tail = (tail + 1) % buffer.Length;
            return item;
        }
        public IEnumerator<T> GetEnumerator() => new ChannelEnumerator<T>(buffer: buffer, head: head, tail: tail);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class ChannelEnumerator<T> : IEnumerator<T>
    {
        private T[] buffer;
        private int curr;
        private int head;
        private int tail;
        private bool destroyed;
        public T Current { get; private set; }
        object IEnumerator.Current { get => Current;  }
        public ChannelEnumerator(T[] buffer, int head, int tail)
        {
            this.buffer = buffer;
            this.head = head;
            this.tail = tail;
            Current = buffer[head];
            curr = head;
            destroyed = false;
        }
        public void Dispose()
        {
            if (destroyed)
                throw new ObjectDisposedException(GetType().Name);
            destroyed = true;
        }
        public bool MoveNext()
        {
            if (destroyed)
                throw new ObjectDisposedException(GetType().Name);
            if (curr == tail)
                return false;
            Current = buffer[curr];
            curr = (curr + 1) % buffer.Length;
            return true;
        }
        public void Reset()
        {
            if (destroyed)
                throw new ObjectDisposedException(this.GetType().Name);
            Current = buffer[head];
            curr = head;
        }
    }
}
