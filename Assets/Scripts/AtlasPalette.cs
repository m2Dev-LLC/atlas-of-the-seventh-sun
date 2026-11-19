using UnityEngine;

public static class AtlasPalette
{
    public static readonly Color Ink = Hex("17201C");
    public static readonly Color DeepInk = Hex("0A100E");
    public static readonly Color Parchment = Hex("E4D2A4");
    public static readonly Color FadedParchment = Hex("B9A878");
    public static readonly Color Sea = Hex("1D5360");
    public static readonly Color Plains = Hex("82905A");
    public static readonly Color Forest = Hex("31543C");
    public static readonly Color Desert = Hex("B38A4B");
    public static readonly Color Mountain = Hex("68685B");
    public static readonly Color Rust = Hex("A64D32");
    public static readonly Color Jade = Hex("3A8978");
    public static readonly Color Gold = Hex("F2BF45");
    public static readonly Color Danger = Hex("D15B45");
    public static readonly Color Fog = new(0.035f, 0.052f, 0.045f, 0.965f);
    public static readonly Color Scrim = new(0.035f, 0.052f, 0.045f, 0.92f);
    static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
}

