using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rhino.Geometry.BrepFace;
using Rhino.DocObjects;
using static YhmAssembly.BrepUtility;
using static YhmAssembly.listEdit;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Rhino;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Runtime.InProcess;
using Rhino.Commands;
using Rhino.Display;
using Grasshopper;
using Grasshopper.Getters;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace YhmAssembly//我的工具箱，仅对缺省类进行操作!
{
    public static class listEdit
    {
        //根据keys排序values
        public static List<T> Sort<T>(List<Double> keys, params T[] values)
        {
            List<KeyValuePair<double, T>> keyValuePairs = new List<KeyValuePair<double, T>>();
            if (keys.Count == values.Length)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    keyValuePairs.Add(new KeyValuePair<double, T>(keys[i], values[i]));
                }
            }//填充keyValuePairs
            var sortedPairs = keyValuePairs.OrderBy(kv => kv.Key);//排序
            List<T> sortedValues = sortedPairs.Select(kv => kv.Value).ToList();//提取出排序后的object列表
            return sortedValues;
        }

        public static void DivideListWithIndex<T>(List<T> list, int index, out List<T> list1, out List<T> list2)
        {
            list1 = list.Take(index + 1).ToList();
            list2 = list.Skip(index + 1).ToList();
        }
        public static GH_Structure<IGH_Goo> TurnDoubleListToDataTree<T>(List<List<T>> listOfLists)
        {
            GH_Structure<IGH_Goo> tree = new GH_Structure<IGH_Goo>();

            for(int i =0;i< listOfLists.Count; i++)
            {
                GH_Path path = new GH_Path(i);
                for(int j=0;j< listOfLists[i].Count; j++)
                {
                    tree.Append(GH_Convert.ToGoo(listOfLists[i][j]), path);
                }
            }
            return tree;
        }
        public static bool IsList(Type type)
        {
            if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                return true;
            }
            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && typeof(IList<>) == it.GetGenericTypeDefinition())
                    return true;
            }
            return false;
        }
        //将链表转化为双重链表
        public static List<List<T>> ConvertToDoubleList<T>(List<T> list, int chunkSize)
        {
            List<List<T>> doubleList = new List<List<T>>();
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                List<T> chunk = list.Skip(i).Take(chunkSize).ToList();
                doubleList.Add(chunk);
            }
            return doubleList;
        }
    }
    public class NegIndexList<T>
    {
        private int watershed = 0;//internalList中，watershed该项及以后的项都是正数
        public int Watershed
        {
            get { return watershed; } 
            set {  watershed = value; }
        }
        private List<T> internalList;

        //构造函数
        public NegIndexList()
        {
            internalList = new List<T>();
        }
        public NegIndexList(List<T> externallList)
        {
            internalList = externallList;
            Watershed = externallList.Count/2;
        }

        public T this[int index]
        {
            get
            {
                int mappedIndex = MapIndex(index);
                return internalList[mappedIndex];
            }
            set
            {
                int mappedIndex = MapIndex(index);
                internalList[mappedIndex] = value;
            }
        }//this表示实例
        private int MapIndex(int originalIndex)
        {
            int mappedIndex = new int();
            if (originalIndex + Watershed >= 0 && originalIndex + Watershed < internalList.Count)
            {
                mappedIndex = originalIndex + Watershed;
            }

            return mappedIndex;
        }//originalIndex指带正负号的索引

        public void PositiveAdd(T item) 
        {
            internalList.Add(item);
        }

        public void NegtiveAdd(T item)
        {
            internalList.Insert(0, item);//往watershed前添加项
            Watershed++;
        }

        public int Count
        {
            get { return internalList.Count; }
        }

        public List<T> ToNormal(out int watershed1)
        {
            watershed1 = Watershed;
            return internalList;
        }

    }//含负数序列的列表
}
