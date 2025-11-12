using System;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    List<T> heapArray = new(8);//堆
    Func<T, T, bool> compareFunc;//比较用函数，使类更通用
    int count = 0;//堆的元素数量，初始为0，**不可在外界更改**
    public int Count => count;
    public PriorityQueue(Func<T, T, bool> func) 
    {
        compareFunc = func;//设置排序方式
        heapArray.Add(default);//初始堆
    }
    public PriorityQueue(Func<T, T, bool> func, T[] array)
    {
        compareFunc = func;//设置排序方式
        heapArray.Add(default);
        for (int i = 0; i < array.Length; i++) {
            Enqueue(array[i]);
        }//初始堆
    }
    public void Enqueue(T item) { //添加元素在数组后
        if (heapArray.Count - 1 == count)//当堆数组没元素插入时
        {
            heapArray.Add(default);//插入新元素
        }
        heapArray[++count] = item;//在最后添加元素
        int i = count;
        while (i != 1 && compareFunc(heapArray[i], heapArray[i / 2]))//比较下元素优先级是否大于上元素优先级
        {
            (heapArray[i], heapArray[i / 2]) = (heapArray[i / 2], heapArray[i]);//交换元素优先级
            i /= 2;//继续在前往根结点处的路径上检测
        }
    }
    public T Dequeue() {//删除队列最高优先级元素
        if (count > 0)
        {
            T delItem = heapArray[1];
            int i = 1;//待需交换节点索引
            int extraAddValue = 0;//额外增加i值
            while (2 * i <= count)
            {
                if (2 * i + 1 <= count)
                {
                    (heapArray[i], heapArray[i * 2 + (compareFunc(heapArray[i * 2], heapArray[i * 2 + 1]) ? 0 : 1)]) = (heapArray[i * 2 + (compareFunc(heapArray[i * 2], heapArray[i * 2 + 1]) ? (extraAddValue = 0) : (extraAddValue = 1))], heapArray[i]);//比较以获取应交换元素
                    i = i * 2 + extraAddValue;//根据交换元素来将i往其移动
                }
                else//无右节点时
                {
                    (heapArray[i], heapArray[i * 2]) = (heapArray[i * 2], heapArray[i]);//交换左节点
                    i *= 2;//根据交换元素来将i往其移动
                }
            }
            (heapArray[i], heapArray[count]) = (heapArray[count], heapArray[i]);//将应删除元素移动数组最后，并-1大小
            count--;
            return delItem;
        }
        return default;
    }
    public T GetMostPriorityItem()
    {
        return count > 0 ? heapArray[1] : default;//索引为1处为优先级最大元素，数组大小为0时不可获取
    }
}
