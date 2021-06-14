using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DataObjectsSample
{
    public abstract class Hashlist: IDictionary, IEnumerable
    {
        //array list that contains all the keys
        //as there are inserted, the index is associated with a key
        //so when pulling out the values by index we can get the key for the index
        //pull from the hashtable the proper value with the corresponding key
        //This is basically the same as a sorted list but does not sort the items
        //rather it leaves them in the order they were inserted - like a list

        protected List<object> m_oKeys = new List<object>();

        protected Dictionary<object, object> m_oValues = new Dictionary<object, object>();

        #region "ICollection implementation"
        //ICollection implementation
            public int Count
            {
                get { return m_oValues.Count; }
        
            }

            public bool IsSynchronized
            { 
                get { return ((IDictionary)m_oValues).IsSynchronized; }
            }

            public object SyncRoot
            {
                get { return ((IDictionary)m_oValues).SyncRoot;  }
            }

            public void CopyTo(System.Array oArray, int iArrayIndex)
            {
                ((ICollection)m_oValues).CopyTo(oArray, iArrayIndex);
            }
        #endregion

        #region "IDictionary implentation"
        public void Add(object oKey, object oValue)
         {
            m_oKeys.Add(oKey);
            m_oValues.Add(oKey, oValue);
;        }

        public void Insert(object oKey, object oValue, int index)
        {
            m_oKeys.Insert(index, oKey);
            m_oValues.Add(oKey, oValue);
        }

        public void Update(object oValue, int index)
        {
            object oKey = m_oKeys[index];
            Remove(oKey);
            m_oKeys.Insert(index, oKey);
            m_oValues.Add(oKey, oValue);
        }

        public void SwapIndex(int indexTo, int indexFrom)
        {
            object oKeyTo = m_oKeys[indexTo];
            object oKeyFrom = m_oKeys[indexFrom];
            m_oKeys[indexTo] = oKeyFrom;
            m_oKeys[indexFrom] = oKeyTo;
        }

        public bool IsFixedSize
        { 
            get { return ((IList)m_oKeys).IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return ((IList)m_oKeys).IsReadOnly; }
        }

        public ICollection Keys
        { 
            get { return m_oValues.Keys; }
        }

        public void Clear()
        {
            m_oValues.Clear();
            m_oKeys.Clear();
        }

        public bool Contains(object oKey)
        {
            return m_oValues.ContainsKey(oKey);
        }

        public bool ContainsKey(object oKey)
        {
            return m_oValues.ContainsKey(oKey);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return m_oValues.GetEnumerator();
        }

        public void Remove(object oKey)
        {
            m_oValues.Remove(oKey);
            m_oKeys.Remove(oKey);
        }

        public void Remove(int index)
        {
            object oKey = m_oKeys[index];
            m_oValues.Remove(oKey);
            m_oKeys.Remove(oKey);
        }

        public object GetAt(int Index)
        {
            return m_oValues[m_oKeys[Index]];
        }

        public object this[object oKey]
        { 
            get { return m_oValues[oKey]; }
            set { m_oValues[oKey] = value; }
        }


        public object this[int Index]
        {
            get { return m_oValues[m_oKeys[Index]]; }
            
        }

        public ICollection Values
        { 
            get { return m_oValues.Values; }
        }

        #endregion

        #region "IEnumerable implementaion"
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(m_oKeys);
        }

        #endregion

        private class Enumerator : IEnumerator
        {

            private List<object> colObj;
            private Int32 intPosition;

            public Enumerator(List<object> list)
            {
                colObj = list;
                intPosition = -1;

            }

            public bool MoveNext()
            {
                intPosition++;
                return (intPosition <= (colObj.Count - 1));
            }

            public void Reset()
            {
                intPosition = -1;
            }

            public Object Current
            {
                get
                {
                    try
                    {
                        return colObj[intPosition];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        private bool pbAscending;
        private PropertyInfo PropertyName = null;

        public void Sort(Type objType, string SortPropertyName, bool Ascending)
        {
            if (this.Count > 0)
            {
                PropertyName = objType.GetProperty(SortPropertyName);
                pbAscending = Ascending;

                Sorting(0,this.Count - 1);
                    
            }
        }

        private void Sorting(int beg, int end)
        {

            if (end == beg)
            {
                return;

            }
            else
            {
                int pivot = getPivotPoint(beg, end);
                if (pivot > beg)
                    Sorting(beg, pivot - 1);
                if (pivot < end)
                    Sorting(pivot + 1, end);
            }
        
        }

        private int getPivotPoint(int begPoint, int endPoint)
        {
            int pivot = begPoint;
            int m = begPoint + 1;
            int n = endPoint;
            object objPivot = GetAt(pivot);
            IComparable item;
            IComparable itemPivot = (IComparable)PropertyName.GetValue(objPivot, null);
            if (itemPivot == null) itemPivot = "";
            object objM;
            object objN;

            objM = GetAt(m);
            if (pbAscending)
            {
                item = (IComparable)PropertyName.GetValue(objM, null);
                if (item == null) item = "";
                while ((m < endPoint) && (itemPivot.CompareTo(item) >= 0))
                {
                    objM = GetAt(++m);
                    item = (IComparable)PropertyName.GetValue(objM, null);
                    if (item == null) item = "";
                }

            }
            else
            {
                item = (IComparable)PropertyName.GetValue(objM, null);
                if (item == null) item = "";
                while ((m < endPoint) && (itemPivot.CompareTo(item) <= 0))
                {
                    objM = GetAt(++m);
                    item = (IComparable)PropertyName.GetValue(objM, null);
                    if (item == null) item = "";
                }
            }

            objN = GetAt(n);
            if (pbAscending)
            {
                item = (IComparable)PropertyName.GetValue(objN, null);
                if (item == null) item = "";
                while ((n < begPoint) && (itemPivot.CompareTo(item) <= 0))
                {
                    objN = GetAt(--n);
                    item = (IComparable)PropertyName.GetValue(objN, null);
                    if (item == null) item = "";
                }

            }
            else
            {
                item = (IComparable)PropertyName.GetValue(objN, null);
                if (item == null) item = "";
                while ((n < begPoint) && (itemPivot.CompareTo(item) >= 0))
                {
                    objN = GetAt(--n);
                    item = (IComparable)PropertyName.GetValue(objN, null);
                    if (item == null) item = "";
                }
            }

            while (m < n)
            {
                this.SwapIndex(m, n);

                objM = GetAt(m);
                if (pbAscending)
                {
                    item = (IComparable)PropertyName.GetValue(objM, null);
                    if (item == null) item = "";
                    while ((m < endPoint) && (itemPivot.CompareTo(item) >= 0))
                    {
                        objM = GetAt(++m);
                        item = (IComparable)PropertyName.GetValue(objM, null);
                        if (item == null) item = "";
                    }

                }
                else
                {
                    item = (IComparable)PropertyName.GetValue(objM, null);
                    if (item == null) item = "";
                    while ((m < endPoint) && (itemPivot.CompareTo(item) <= 0))
                    {
                        objM = GetAt(++m);
                        item = (IComparable)PropertyName.GetValue(objM, null);
                        if (item == null) item = "";
                    }
                }

                objN = GetAt(n);
                if (pbAscending)
                {
                    item = (IComparable)PropertyName.GetValue(objN, null);
                    if (item == null) item = "";
                    while ((n < begPoint) && (itemPivot.CompareTo(item) <= 0))
                    {
                        objN = GetAt(--n);
                        item = (IComparable)PropertyName.GetValue(objN, null);
                        if (item == null) item = "";
                    }

                }
                else
                {
                    item = (IComparable)PropertyName.GetValue(objN, null);
                    if (item == null) item = "";
                    while ((n < begPoint) && (itemPivot.CompareTo(item) >= 0))
                    {
                        objM = GetAt(--n);
                        item = (IComparable)PropertyName.GetValue(objN, null);
                        if (item == null) item = "";
                    }
                }


            }

            if (pivot != n)
            {
                this.SwapIndex(n, pivot);
            
            }

            return n;
        }
        

    }
}
