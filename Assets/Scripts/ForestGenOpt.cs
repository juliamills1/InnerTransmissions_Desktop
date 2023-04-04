using PathCreation;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System;
using UnityEngine;

//-----------------------------------------------------------------------------
// name: ForestGenOpt.cs
// desc: randomly place trees, silent sculptures, and bushes
//-----------------------------------------------------------------------------

public class ForestGenOpt : MonoBehaviour
{
    // forest properties
    private int forestX = 350;
    private int forestZ = 550;
    private int elemSpacing = 12;
    private int pathMargin = 20;

    // prefab and path arrays
    public Elem[] elements;
    public PathCreator[] paths;
    List<Vector3> pathPts = new List<Vector3>();

    void Start()
    {
        // store path points
        for (int p = 0; p < paths.Length; p++)
        {
            for (float i = 0f; i < 1f; i += 0.01f)
            {
                pathPts.Add(paths[p].path.GetPointAtTime(i, EndOfPathInstruction.Stop));
            }
        }

        Generate();
    }

    // semi-randomly place objects according to priority list
    void Generate()
    {
        float randOffset = elemSpacing / 4f;

        // iterate across area
        for (int x = -forestX / 2; x < forestX / 2; x += elemSpacing)
        {
            for (int z = -forestZ / 2; z < forestZ / 2; z += elemSpacing)
            {
                // through each element type (trees, silent sculptures, bushes)
                for (int i = 0; i < elements.Length; i++)
                {
                    Elem elem = elements[i];

                    // generate new object if not on path, weighted by density
                    if (elem.CanPlace() && NotOnPath(x, z))
                    {
                        Vector3 pos = new Vector3(x, 0f, z);
                        Vector3 offset = new Vector3(Random.Range(-randOffset, randOffset),
                                                     0f,
                                                     Random.Range(-randOffset, randOffset));

                        Vector3 rotation = new Vector3(Random.Range(0f, 5f),
                                                       Random.Range(0f, 360f),
                                                       Random.Range(0f, 5f));

                        Vector3 scale = Vector3.one * Random.Range(0.75f, 2f);

                        GameObject newElem = Instantiate(elem.GetRandom());
                        newElem.transform.SetParent(transform);
                        newElem.transform.position = pos + offset;
                        newElem.transform.eulerAngles = rotation;
                        newElem.transform.localScale = scale;

                        // assign large/small objects to different camera layers
                        if (String.Equals(elem.name, "Bushes"))
                        {
                            newElem.layer = 11;
                        }
                        else
                        {
                            newElem.layer = 12;
                        }

                        break;
                    }
                }
            }
        }
    }

    // if point is inside path margin, return false
    public bool NotOnPath(int x, int z)
    {
        Vector3 pt = new Vector3(x, 0f, z);

        for (int i = 0; i < pathPts.Count; i++)
        {
            if (Vector3.Distance(pt, pathPts[i]) < pathMargin)
            {
                return false;
            }
        }
        return true;
    }
}

// generic class for the various elements (prefab categories) to be placed
[System.Serializable]
public class Elem
{
    public string name;
    public GameObject[] prefabs;
    [Range(1, 10)] public int density;

    // return true/false randomly WRT density cutoff
    public bool CanPlace()
    {
        if (Random.Range(0, 10) < density)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // randomly select prefab from element options
    public GameObject GetRandom()
    {
        return prefabs[Random.Range(0, prefabs.Length)];
    }
}
