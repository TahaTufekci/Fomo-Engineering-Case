using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using UnityEngine;

public class BlockCell : MonoBehaviour
{
    public int colorValue;
    public Color color;
    public List<int> directions;
    public int posX, posY;
    public int length;
    public Texture2D texture; // Texture for the block

    public void Initialize(int length, int value, Texture2D texture = null, List<int> directions = null)
    {
        this.length = length;
        this.colorValue = value;
        this.texture = texture;
        foreach (var direction in directions)
        {
            this.directions.Add(direction);
        }
        color = ColorMapper.GetColor(value);
        GetComponent<Renderer>().material.color = color;
        ApplyTexture(texture);
    }
   
    public void SetCoordinates(int x, int y)
    {
        posX = x;
        posY = y;
        gameObject.name = "Block: (" + x + ") (" + y + ")";
    }

    private void ApplyTexture(Texture2D texture)
    {
        // Apply the texture to the block
        var renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }
    
    public void MoveToTarget(Vector3 targetPos)
    {
        StartCoroutine(MoveCoroutine(targetPos));
    }
    
    private IEnumerator MoveCoroutine(Vector3 targetPos)
    {
        var duration = 0.2f;
        Vector3 starPosition = transform.position;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            transform.position = Vector3.Lerp(starPosition, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        } 

        transform.position = targetPos;
    }
}