using UnityEngine;

namespace Helpers
{
    public static class ColorMapper
    {
        public static Color GetColor(int value)
        {
            switch (value)
            {
                case 0: return Color.red;
                case 1: return Color.green;
                case 2: return Color.blue;
                case 3: return Color.yellow;
                case 4: return Color.magenta;
                default: return Color.black;
            }
        }
    }
}