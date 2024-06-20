using System.Collections.Generic;
using System.Linq;
using Objects;
using UnityEngine;

namespace Builders
{
    public class BlockBuilder
    {
        private int length;
        private Texture2D texture;
        private int colorValue;
        private List<int> directions;
        private GameObject prefab;
        private List<BlockSO> blockScriptableObjects;

        public BlockBuilder SetLength(int length)
        {
            this.length = length;
            return this;
        }

        public BlockBuilder SetTexture(Texture2D texture)
        {
            this.texture = texture;
            return this;
        }

        public BlockBuilder SetColorValue(int colorValue)
        {
            this.colorValue = colorValue;
            return this;
        }

        public BlockBuilder SetDirection(List<int> directions)
        {
            this.directions = directions;
            return this;
        }

        public BlockBuilder SetPrefab(GameObject prefab)
        {
            this.prefab = prefab;
            return this;
        }

        public BlockBuilder SetScriptableObjects(List<BlockSO> blockScriptableObjects)
        {
            this.blockScriptableObjects = blockScriptableObjects;
            return this;
        }

        public BlockCell Build(Transform parent)
        {
            var matchingBlockSO = blockScriptableObjects.Find(blockSO => blockSO.length == length && blockSO.colorValue == colorValue && blockSO.directions.SequenceEqual(directions));

            if (matchingBlockSO != null)
            {
                var blockObj = Object.Instantiate(prefab, parent);
                var blockNode = blockObj.GetComponent<BlockCell>();
                blockNode.Initialize(matchingBlockSO.length, matchingBlockSO.colorValue, matchingBlockSO.texture, matchingBlockSO.directions);
                return blockNode;
            }

            Debug.LogError("No matching BlockSO found for the given length, color value, and direction.");
            return null;
        }
    }
}