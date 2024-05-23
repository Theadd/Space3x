using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Space3x.UiToolkit.SlicedText.Iterators
{

    public class RefList<T> : IEnumerable
    {
        private T[] array = null;
        private int index = 0;
        private int capacity = 4;

        public RefList(int capacity)
        {
            this.capacity = capacity;
            array = new T[capacity];
        }

        public RefList()
        {
            array = new T[capacity];
        }

        internal ref T[] GetArray()
        {
            return ref array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (index >= array.Length)
            {
                Expand();
            }

            array[index++] = value;
        }

        public ref T Get(int index)
        {
            return ref array[index];
        }

        public void Set(int index, T value)
        {
            array[index] = value;
        }

        public void Expand()
        {
            var newCapacity = array.Length * 2;

            T[] newArray = new T[newCapacity];
            Array.Copy(array, newArray, array.Length);
            array = newArray;

            capacity = newCapacity;
        }

        public T this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }

        /*/ EDIT /*/
        
        public int Count => this.index;
        
        /// <summary><para>Removes the item at the specified index of the list.</para></summary>
        /// <param name="_index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int _index)
        {
            --this.index;
            if (_index < this.index)
                Array.Copy((Array) this.array, _index + 1, (Array) this.array, _index, this.index - _index);
            this.array[this.index] = default (T);
        }

        /// <summary><para>Removes a range of elements from the list.</para></summary>
        /// <param name="_index">The zero-based starting index of the range of elements to remove.</param>
        public void RemoveRange(int _index, int _count)
        {
            this.index -= _count;
            if (_index < this.index)
                Array.Copy((Array) this.array, _index + _count, (Array) this.array, _index, this.index - _index);
            Array.Clear((Array) this.array, this.index, _count);
        }
        
        /// <summary><para>Inserts the elements of a collection in the List at the specified position.</para></summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        public void InsertRange(int _index, IEnumerable<T> collection)
        {
            if ((uint) _index > (uint) this.index)
                throw new ArgumentOutOfRangeException();
            if (collection is ICollection<T> objs)
            {
                int count = objs.Count;
                if (count > 0)
                {
                    while (this.array.Length < this.index + count)
                        Expand();

                    if (_index < this.index)
                        Array.Copy((Array) this.array, _index, (Array) this.array, _index + count, this.index - _index);
                    if (this == objs)
                    {
                        Array.Copy((Array) this.array, 0, (Array) this.array, _index, _index);
                        Array.Copy((Array) this.array, _index + count, (Array) this.array, _index * 2, this.index - _index);
                    }
                    else
                        objs.CopyTo(this.array, _index);
                    this.index += count;
                }
            }
            else
            {
                foreach (T obj in collection)
                    this.Insert(_index++, obj);
            }
        }
        
        public void InsertRange(int _index, RefList<T> collection)
        {
            if ((uint) _index > (uint) this.index)
                throw new ArgumentOutOfRangeException();
            // var objs = ((ICollection<T>) collection) ?? (RefList<T>) collection;
            if (/*collection is ICollection<T> objs || */ collection is RefList<T> objs)
            {
                int count = objs.Count;
                if (count > 0)
                {
                    while (this.array.Length < this.index + count)
                        Expand();

                    if (_index < this.index)
                        Array.Copy((Array) this.array, _index, (Array) this.array, _index + count, this.index - _index);
                    if (this == objs)
                    {
                        Array.Copy((Array) this.array, 0, (Array) this.array, _index, _index);
                        Array.Copy((Array) this.array, _index + count, (Array) this.array, _index * 2, this.index - _index);
                    }
                    else
                        objs.CopyTo(this.array, _index);
                    this.index += count;
                }
            }
            else
            {
                foreach (T obj in collection)
                    this.Insert(_index++, obj);
            }
        }
        
        /// <summary><para>Inserts an item to the List at the specified position.</para></summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> is to be inserted.</param>
        public void Insert(int _index, T item)
        {
            if ((uint) _index > (uint) this.index)
                throw new ArgumentOutOfRangeException();
            if (this.index == this.array.Length)
                Expand();
            if (_index < this.index)
                Array.Copy((Array) this.array, _index, (Array) this.array, _index + 1, this.index - _index);
            this.array[_index] = item;
            ++this.index;
        }
        
        /// <summary><para>Copies the entire list to an array.</para></summary>
        /// <param name="array">A one-dimensional, zero-based array that is the destination of the elements copied from the list.</param>
        void CopyTo(T[] _array, int arrayIndex)
        {
            try
            {
                Array.Copy((Array) this.array, 0, _array, arrayIndex, this.index);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException();
            }
        }

        /*/ --- /*/

        // public RefEnumerator GetEnumerator() => new RefEnumerator(array, capacity);
        public IEnumerator GetEnumerator() => new RefEnumerator(array, capacity);
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

        public struct RefEnumerator : IEnumerator
        {
            private T[] array;

            private int index;
            private int capacity;

            public RefEnumerator(T[] target, int capacity)
            {
                array = target;
                index = -1;
                this.capacity = capacity;
            }

            object IEnumerator.Current
            {
                get => throw new VersionNotFoundException();
            }
            
            public ref T Current
            {
                get
                {
                    if (array is null || index < 0 || index > capacity)
                    {
                        throw new InvalidOperationException();
                    }
                    return ref array[index];
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext() => ++index < capacity;

            public void Reset() => index = -1;
        }
    }
}
