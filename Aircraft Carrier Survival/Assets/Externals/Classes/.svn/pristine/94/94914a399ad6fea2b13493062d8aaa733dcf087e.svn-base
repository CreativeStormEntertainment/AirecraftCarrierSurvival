using System.Collections.Generic;

namespace GambitUtils
{
    public class ListRandomizer<T>
    {
        public int Count => list.Count;

        private readonly List<T> list = new List<T>();
        private readonly List<T> pool = new List<T>();

        private T lastPickedElement = default;

        public static ListRandomizer<int> CreateIntRandomizer(int count)
        {
            return new ListRandomizer<int>(ListUtils.CreateIntList(0, count));
        }

        public ListRandomizer()
        {
        }

        public ListRandomizer(List<T> list) : this()
        {
            SetList(list);
        }

        public void SetList(List<T> list)
        {
            this.list.Clear();
            this.list.AddRange(list);
            pool.Clear();
            pool.AddRange(list);
        }

        public T PickRandomElement()
        {
            if (list.Count == 0)
            {
                return default;
            }
            else if (list.Count == 1)
            {
                return list[0];
            }
            T pickedElement;
            if (pool.Count == 0)
            {
                pool.AddRange(list);
                if (lastPickedElement != null)
                {
                    pool.Remove(lastPickedElement);
                }
                pickedElement = list.PickRandomElement();
                if (lastPickedElement != null)
                {
                    pool.Add(lastPickedElement);
                }
            }
            else
            {
                pickedElement = pool.PickRandomElement();
            }
            lastPickedElement = pickedElement;
            return pickedElement;
        }
    }
}
