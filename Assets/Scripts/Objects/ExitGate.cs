using Helpers;
using UnityEngine;

namespace Objects
{
    public class ExitGate : MonoBehaviour
    {
        public int colorValue;
        public Color color;
        public int posX, posY;
        public int direction;
    
        public void Initialize(int colorValue, int direction)
        {
            this.colorValue = colorValue;
            this.direction = direction;
            color =  color = ColorMapper.GetColor(colorValue);
            GetComponent<Renderer>().material.color = color;
        }

        public void SetCoordinates(int x, int y)
        {
            posX = x;
            posY = y;
            gameObject.name = "Exit gate:(" + x + ") (" + y + ")";
        }
    
    }
}