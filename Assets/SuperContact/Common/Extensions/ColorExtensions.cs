using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions {
    public static Color TimesIgnoringAlpha(this Color color, float factor) {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
}

public static class ColorUtil {
    public static int Color32ToInt(Color32 color) {
        return (int)(((uint)color.r << 24) + ((uint)color.g << 16) + ((uint)color.b << 8) + (uint)color.a);
    }

    public static Color32 IntToColor32(int integer) {
        uint n = (uint)integer;
        byte r = (byte)((n >> 24) & 0xFF);
        byte g = (byte)((n >> 16) & 0xFF);
        byte b = (byte)((n >> 8) & 0xFF);
        byte a = (byte)(n & 0xFF);
        return new Color32(r, g, b, a);
    }
}
