using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {

public static class Utility {
    public static int lastScreenWidth, lastScreenHeight;
    public static Vector2Int GetScreenSize() {
        return new Vector2Int(Screen.width, Screen.height);
    }
    
    public static void SetScreenSize(int width, int height) {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }

    public static void InitScreenSizeWithRatio(float widthRatio, float heightRatio) {
        Resolution resolution = Screen.currentResolution;
        var screen = new Vector2(resolution.width, resolution.height);
        if (Application.platform == RuntimePlatform.WindowsPlayer) {
            screen = screen * 4 / 5;
        }
        var width = (int)screen.x;
        var height = (int)screen.y;
        var screenRatio = screen.x / screen.y;
        var currentRatio = widthRatio / heightRatio;
        if (screenRatio > currentRatio) {
            width = (int)(screen.y / heightRatio * widthRatio);
        } else if (screenRatio < currentRatio) {
            height = (int)(screen.x / widthRatio * heightRatio);
        }
        SetScreenSize(width, height);
    }
}

}
