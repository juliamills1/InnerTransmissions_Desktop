using UnityEngine;

//-----------------------------------------------------------------------------
// name: FilePlayer.cs
// desc: run ChucK file corresponding to current game object
//-----------------------------------------------------------------------------

public class FilePlayer : MonoBehaviour
{
    void Start()
    {
        string file = gameObject.name + "player.ck";
        GetComponent<ChuckSubInstance>().RunFile(file, true);
    }
}