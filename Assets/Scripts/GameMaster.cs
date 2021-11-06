using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    // settings
    public static float updatesPerSecond = 1.0f;
    public static float boostFallMultiplier = 10.0f;

    // board
    public static int height = 10;
    public static int width = 24;
    public static Transform[,] grid = new Transform[width, height];
}
