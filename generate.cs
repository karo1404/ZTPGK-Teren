using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! Sample terrain animator/generator
public class generate : MonoBehaviour
{
    private Terrain _myTerr;
    private TerrainData _myTerrData;
    private int _xRes;
    private int _yRes;
    private float[,] _terrHeights;
    private float[,] originalTerrainSectionHeight;
    public bool useImage = false;
    public Texture2D image;
    public int x=0, y=0;
    public int numberOfPasses = 5;
    public int radiusOfAnimation = 50;

    // Use this for initialization
    void Start()
    {
        // Get terrain and terrain data handles
        _myTerr = GetComponent<Terrain>();
        _myTerrData = _myTerr.terrainData; // Get terrain dimensions in tiles (X tiles x Y tiles) 
        _xRes = _myTerrData.heightmapWidth;
        _yRes = _myTerrData.heightmapHeight; // Set heightmap
        if (useImage)
        {
            GenerateFromImage();
        } else
        {
            RandomizeTerrain();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!useImage)
        {
            // Call animation function
            AnimTerrain();
        }
    }

    private void GenerateFromImage()
    {
        _terrHeights = _myTerrData.GetHeights(0, 0, _xRes, _yRes);
        

        for (int i = 0; i < _xRes; i++)
        {
            for (int j = 0; j < _yRes; j++)
            {
                Color pixel = image.GetPixel((int)((Convert.ToSingle(j) / Convert.ToSingle(_xRes)) * image.width), (int)((Convert.ToSingle(i) / Convert.ToSingle(_yRes)) * image.height));
                _terrHeights[i, j] = (pixel.r + pixel.g + pixel.b)/3.0f;
            }
        }
        _myTerrData.SetHeights(0, 0, _terrHeights);
    }

    // Set the terrain using noise pattern
    private void RandomizeTerrain()
    {
        // Extract entire heightmap (expensive!)
        _terrHeights = _myTerrData.GetHeights(0, 0, _xRes, _yRes);
        // STUDENT'S CODE
        //
        // ...
        float[] scale = new float[numberOfPasses];
        float[] scale2 = new float[numberOfPasses];
        float[] scale3 = new float[numberOfPasses];
        for (int k = 0; k < numberOfPasses; k++)
        {
            scale[k] = UnityEngine.Random.Range(0.0f, 3.0f);
            scale2[k] = UnityEngine.Random.Range(0.0f, 20.0f);
            scale2[k] = UnityEngine.Random.Range(5.0f, 10.0f);
        }
        for (int i = 0; i < _xRes; i++)
        {
            for (int j = 0; j < _yRes; j++)
            {
                float xCoeff = (float)i / _xRes;
                float yCoeff = (float)j / _yRes;
                _terrHeights[i, j] = 0;
                for (int k = 0; k < numberOfPasses; k++)
                {
                    _terrHeights[i, j] += Mathf.PerlinNoise(xCoeff * scale[k], yCoeff * scale[k])*0.5f;
                    _terrHeights[i, j] += Mathf.PerlinNoise(xCoeff * scale2[k], yCoeff * scale2[k])*0.05f;
                    _terrHeights[i, j] += Mathf.PerlinNoise(xCoeff * scale3[k], yCoeff * scale3[k]) * 0.05f;
                }
                _terrHeights[i, j] /= (float)numberOfPasses;
            }
        }

        // 
        // Set terrain heights (_terrHeights[coordX, coordY] = heightValue) in a loop
        //
        // You can sample perlin's noise (Mathf.PerlinNoise (xCoeff, yCoeff)) using coefficients
        // between 0.0f and 1.0f
        // 
        // You can combine 2-3 layers of noise with different resolutions and amplitudes fora better effect
        // ...
        // END OF STUDENT'S CODE 
        //
        // Set entire heightmap (expensive!)
        _myTerrData.SetHeights(0, 0, _terrHeights); 
        originalTerrainSectionHeight = _myTerrData.GetHeights(x, y, radiusOfAnimation * 2, radiusOfAnimation * 2);
    }

    // Animate part of the terrain
    private void AnimTerrain()
    {
        // STUDENT'S CODE 
        //
        // ...
        // Extract PART of the terrain e.g. 40x40 tiles (select corner (x, y) and extractedpatch size)
        // GetHeights(5, 5, 10, 10) will extract 10x10 tiles at position (5, 5)
        //
        // Animate it using Time.time and trigonometric functions
        //
        // 3d generalizaton of sinc(x) function can be used to create the teardrop effect(sinc(x) = sin(x) / x)
        //
        // It is reasonable to store animated part of the terrain in temporary variable e.g.in RandomizeTerrain()
        // function. Later, in AnimTerrain() this temporary area can be combined withcalculated Z (height) value.
        // Make sure you make a deep copy instead of shallow one (Clone(), assign operator).
        // 
        // Set PART of the terrain (use extraction parameters)
        //
        // END OF STUDENT'S CODE 
        //
        _terrHeights = _myTerrData.GetHeights(x, y, radiusOfAnimation*2, radiusOfAnimation*2);
        Vector2 middle = new Vector2(radiusOfAnimation, radiusOfAnimation);
        for (int i = 0; i < radiusOfAnimation*2; i++)
        {
            for (int j = 0; j < radiusOfAnimation*2; j++)
            {
                Vector2 point = new Vector2(i, j);
                double distance = Vector2.Distance(point, middle);
                double difference = (radiusOfAnimation - distance) / radiusOfAnimation;
                if (difference < 0) difference = 0;
                _terrHeights[i, j] = (float)(originalTerrainSectionHeight[i, j] * (System.Math.Sin(Time.time + distance / 10) / 2f) * difference)*0.2f + originalTerrainSectionHeight[i, j];
            }
        }
        _myTerrData.SetHeights(x, y, _terrHeights);
    }
}