using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArrayHelper
{
    public static T[] Add<T>(T value, T[] array)
    {
        ArrayList tempList = new ArrayList();

        foreach (T item in array) { tempList.Add(item); }
        tempList.Add(value);

        return tempList.ToArray(typeof(T)) as T[];
    }

    public static T[] RemoveAt<T>(int idx, T[] array)
    {
        ArrayList tempList = new ArrayList();

        foreach (T item in array) { tempList.Add(item); }
        tempList.RemoveAt(idx);

        return tempList.ToArray(typeof(T)) as T[];
    }

    public static T[] Remove<T>(T value, T[] array)
    {
        ArrayList tempList = new ArrayList();

        foreach (T item in array)
        {
            if (item.Equals(value)) { continue; }

            tempList.Add(item);
        }

        return tempList.ToArray(typeof(T)) as T[];
    }

    public static List<T> ArrayToList<T>(T[] array)
    {
        return array.ToList();
    }

    public static T[] ListToArray<T>(List<T> list)
    {
        return list.ToArray();
    }

    public static void Shuffle<T>(ref T[] array)
    {
        int curIndex = array.Length;

        while (curIndex > 1)
        {
            --curIndex;
            int nextIndex = Random.Range(0, curIndex + 1);
            (array[curIndex], array[nextIndex]) = (array[nextIndex], array[curIndex]);
        }
    }
}
