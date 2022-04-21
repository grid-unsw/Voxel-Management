using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VisualizationFunctions
{
    public static Color[] GetUniqueColors(int count)
    {
        Color[] colors = new Color[count];
        for (int i = 0; i < count; i++)
        {
            colors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
        return colors;
    }

    public static Dictionary<int, Color> GetUniqueColors(int[] ids)
    {
        Dictionary<int, Color> colors = new Dictionary<int, Color>(ids.Length);
        foreach (var id in ids)
        {
            colors[id] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        }

        return colors;
    }
}
