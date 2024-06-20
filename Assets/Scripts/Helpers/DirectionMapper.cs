using UnityEngine;

namespace Helpers
{
    public static class DirectionMapper
    {
        public static int GetDirection(Vector2 direction)
        {
            if(direction == Vector2.up)
            {
                return 0;
            }
            else if(direction == Vector2.right)
            {
                return 1;
            }
            else if(direction == Vector2.down)
            {
                return 2;
            }
            else if(direction == Vector2.left)
            {
                return 3;
            }
            else
            {
                return -1;
            }
           
        }
    }
}