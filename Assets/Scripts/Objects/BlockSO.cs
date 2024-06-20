using System.Collections.Generic;
using UnityEngine;

namespace Objects
{
    [CreateAssetMenu(fileName = "NewBlockConfig", menuName = "Blocks/BlockConfig")]
    public class BlockSO : ScriptableObject
    {
        public List<int> directions;
        public int length; // Size of the block (e.g., 1x1, 1x2)
        public Texture2D texture; // Texture for the block
        public int colorValue; // Value of the block
    }
}
