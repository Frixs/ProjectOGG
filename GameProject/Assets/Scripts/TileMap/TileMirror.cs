using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TileMirror : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Tilemap tm = (Tilemap)gameObject.GetComponent("Tilemap");

        BoundsInt bounds = tm.cellBounds;
        //Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 180, 0), Vector3.one);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = bounds.position.y; y < bounds.size.y; y++)
            {
                TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                Matrix4x4 currentMatrix = tm.GetTransformMatrix(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    tm.SetTile(new Vector3Int(-x, y, 0), tile);
                    currentMatrix.m00 *= -1;
                    tm.SetTransformMatrix(new Vector3Int(-x, y, 0), currentMatrix); 
                    
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
