using UnityEngine;
namespace _Scripts.Utils
{
    public static class ColorUtils
    {
        public static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex,out var color);
            return color;
        }
    }
}