using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Threading.Tasks;

public class PriorityQueue<T>
{
    List<T> HeapArray { get; set; }//堆
    Func<T, T, bool> CompareFunc { get; }//比较用函数，使类更通用
    Dictionary<T, Dictionary<int, bool>> ItemToIndexDic { get; set; }//用物品获取索引
    int count = 0;//堆的元素数量，初始为0，**不可在外界更改**
    public int Count => count;
    public PriorityQueue(Func<T, T, bool> _compareFunc, int heapArrayCapacity = 8)
    {
        ItemToIndexDic = new();//初始化字典
        CompareFunc = _compareFunc;//设置排序方式
        HeapArray = new(heapArrayCapacity);//设置初始堆的容量
        HeapArray.Add(default);//初始堆
    }
    public PriorityQueue(Func<T, T, bool> _compareFunc, int heapArrayCapacity = 8, params T[] array)
    {
        ItemToIndexDic = new();
        CompareFunc = _compareFunc;//设置排序方式
        HeapArray = new(Math.Max(array.Length, heapArrayCapacity));//如果要拷贝的数长度大于预留堆的容量，那么最好设置数组的容量，否则设置的是预留堆的容量
        HeapArray.Add(default);
        foreach (var item in array)
        {
            Enqueue(item);
        }//初始堆
    }
    public void Enqueue(T item)//添加元素在堆数组后
    {
        HeapArray.Add(item);//插入新元素
        int i = ++count;
        UpdateDic(item, i);//更新字典，让节点存在，有时NodeUp方法中不用UpdateDic
        NodeUp(i);//节点上升
    }
    public void EnqueueArray(T[] items)//添加元素组在堆数组后
    {
        for (int i = 0; i < items.Length; i++)
        {
            Enqueue(items[i]);
        }
    }
    public T Dequeue()
    {//删除队列最高优先级元素
        if (count > 0)
        {
            T delItem = HeapArray[1];
            int i = NodeDown(1);//待需删节点索引(执行后节点会变)
            ItemToIndexDic[delItem].Remove(i);//必定
            if (i != count)//相同时删2次
            {
                ItemToIndexDic[HeapArray[count]].Remove(count);
                UpdateDic(HeapArray[count], i);
                (HeapArray[i], HeapArray[count]) = (HeapArray[count], HeapArray[i]);//将应删除元素移动数组最后，并-1大小
            }
            count--;
            return delItem;
        }
        return default;
    }
    public T GetMostPriorityItem()
    {
        return HeapArray[1];//索引为1处为优先级最大元素，数组大小为0时不可获取
    }
    public void UpdateItem(T item, int order = 0)
    {
        if (!ItemToIndexDic.ContainsKey(item))//不存在时物品不更新
        {
            return;
        }
        //根据物品获取索引，可能有多个索引，可能无物品->不发生，用order选择，可选择索引数量超过order时选最后，order<0时自动归0，不可用索引不算
        var indexArr = from index in ItemToIndexDic[item].Keys//在索引可能在的索引内查询
                       where ItemToIndexDic[item][index]//如果索引可用
                       select index;
        order = Math.Min(Math.Max(0, order), indexArr.Count() - 1);
        int arrI = indexArr.Count() / 2 < order ? indexArr.Count() - 1 : 0;//反转
        int i = 0;
        foreach (int idx in indexArr.Count() / 2 < order ? indexArr.Reverse()/*n/2+延迟执行*/ : indexArr) {
            if (arrI == order)
            {
                i = idx;//确定索引
                break;
            }
            arrI += indexArr.Count() / 2 < order ? -1 : 1;//反转
        }
        if (i < 1)//键值对虽有，堆不存在物品
        {
            return;
        }
        if (1 != i && CompareFunc(HeapArray[i], HeapArray[i / 2]))//看是否上比下后，是上
        {//根据情况更新节点
            NodeUp(i);
        }
        else if (i * 2 <= count && !CompareFunc(HeapArray[i], count < i * 2 + 1 ? HeapArray[i * 2] : CompareFunc(HeapArray[i * 2], HeapArray[i * 2 + 1]) ? HeapArray[i * 2] : HeapArray[i * 2 + 1]))//找下二点优先级高值，查看是否下比上先 & 查看下左是否下比上先，是下
        {
            NodeDown(i);
        }
    }
    public T ChangeItem(T item, T changeItem, int order = 0)
    {
        if (!ItemToIndexDic.ContainsKey(item))//不存在时物品不更新
        {
            return item;
        }
        //根据物品获取索引，可能有多个索引，可能无物品，返回在列表内代表item的数据，用order选择，可选择索引数量超过order时选最后，order<0时自动归0，不可用索引不算
        var indexArr = from index in ItemToIndexDic[item].Keys//在索引可能在的索引内查询
                       where ItemToIndexDic[item][index]//如果索引可用
                       select index;
        order = Math.Min(Math.Max(0, order), indexArr.Count() - 1);
        int arrI = indexArr.Count() / 2 < order ? indexArr.Count() - 1 : 0;//反转
        int i = 0;
        foreach (int idx in indexArr.Count() / 2 < order ? indexArr.Reverse()/*n/2+延迟执行*/ : indexArr)
        {
            if (arrI == order)
            {
                i = idx;//确定索引
                break;
            }
            arrI += indexArr.Count() / 2 < order ? -1 : 1;//反转
        }
        if (i < 1)//不可访问时退
        {
            return default;
        }
        HeapArray[i] = changeItem;
        ItemToIndexDic[item].Remove(i);//停用原索引
        UpdateDic(changeItem, i);//新索引使用
        if (1 != i && CompareFunc(HeapArray[i], HeapArray[i / 2]))//看是否上比下后
        {//根据情况更新节点
            NodeUp(i);
        }
        else if (i * 2 <= count && !CompareFunc(HeapArray[i], count < i * 2 + 1 ? HeapArray[i * 2] : CompareFunc(HeapArray[i * 2], HeapArray[i * 2 + 1]) ? HeapArray[i * 2] : HeapArray[i * 2 + 1]))//找下二点优先级高值，查看是否下比上先 & 查看下左是否下比上先
        {
            NodeDown(i);
        }
        return changeItem;
    }
    public void Clear()//清空堆
    {
        if (ItemToIndexDic.Count != 0)
        {//dic无时禁用
            ItemToIndexDic.Clear();
        }
        count = 0;//count置0表清空
    }
    int NodeUp(int i)//节点上升
    {
        while (i != 1 && CompareFunc(HeapArray[i], HeapArray[i / 2]))//比较下元素优先级是否大于上元素优先级
        {
            (HeapArray[i], HeapArray[i / 2]) = (HeapArray[i / 2], HeapArray[i]);//交换元素优先级
            //i与i/2索引对应元素删除v
            Parallel.Invoke(() => { ItemToIndexDic[HeapArray[i / 2]].Remove(i); }, () => { ItemToIndexDic[HeapArray[i]].Remove(i / 2); });
            //i与i/2索引交换后对应元素添加v
            Parallel.Invoke(() => { UpdateDic(HeapArray[i], i); }, () => { UpdateDic(HeapArray[i / 2], i / 2); });
            //-----
            i /= 2;//继续在前往根结点处的路径上检测
        }
        return i;
    }
    int NodeDown(int i)//节点下降
    {
        int extraAddValue = 0;//额外增加i值
        while (2 * i <= count)
        {
            extraAddValue = 2 * i + 1 <= count ? (CompareFunc(HeapArray[i * 2], HeapArray[i * 2 + 1]) ? 0 : 1) : 0;//比较获取交换元素
            (HeapArray[i], HeapArray[i * 2 + extraAddValue]) = (HeapArray[i * 2 + extraAddValue], HeapArray[i]);//swap
            //i与i*2+extraAddValue索引对应元素停用v(原位删)
            Parallel.Invoke(() => { ItemToIndexDic[HeapArray[i]].Remove(i * 2 + extraAddValue); }, () => { ItemToIndexDic[HeapArray[i * 2 + extraAddValue]].Remove(i); });
            //i与i*2+extraAddValue索引交换后对应元素使用v(换位用)
            Parallel.Invoke(() => { UpdateDic(HeapArray[i], i); }, () => { UpdateDic(HeapArray[i * 2 + extraAddValue], i * 2 + extraAddValue); });
            //-----
            i = i * 2 + extraAddValue;//根据交换元素来将i往其移动
        }
        return i;
    }
    void UpdateDic(T item, int i)
    {
        if (!ItemToIndexDic.TryGetValue(item, out Dictionary<int, bool> indexs))//堆中未出现item时
        {
            ItemToIndexDic.Add(item, new Dictionary<int, bool> { { i, true } });//添加item及索引
        }
        else if (!indexs.TryGetValue(i, out bool canUse))//堆中已出现item却未出现对应索引时
        {
            indexs.Add(i, true);//添加该索引
        }
    }
}
