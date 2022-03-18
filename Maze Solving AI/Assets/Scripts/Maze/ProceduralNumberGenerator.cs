using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralNumberGenerator
{
    public static int currentPosition;
    public const string key = "1234241233421412343243123421224123323222114";

    public static int GetNextNumber() {
        string currentNum = key.Substring(currentPosition++ % key.Length, 1);

        return int.Parse(currentNum);
        //return Random.Range(1,5);
    }
}
