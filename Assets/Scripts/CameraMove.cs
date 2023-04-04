using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//-----------------------------------------------------------------------------
// name: CameraMove.cs
// desc: keyboard input move, mouse to rotate, trigger inner worlds according
//       to player position/stillness
//-----------------------------------------------------------------------------

public class CameraMove : MonoBehaviour
{
    // mouse tracking
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
    private float sensitivity = 2f;
    private Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";

    // current movement (stationary/walk/run)
    private float mode = 0f;
    private float previousMode = 0f;
    private float speed = 6f;

    // stillness variables
    private float time = 0;
    private int previousApproxTime = -1;

    // world variables
    public GameObject forest;
    public GameObject staticParent;
    public GameObject[] sculptures;
    public GameObject[] innerWorlds;
    public int sculptBounds = 10;
    List<GameObject> sculptByDist = new List<GameObject>();

    void Start()
    {
        GetComponent<ChuckSubInstance>().RunFile("footsteps.ck", true);

        // set up camera layer culling
        float[] distances = new float[32];
        distances[11] = 200f; // bushes
        distances[12] = 350f; // trees & silent sculptures
        Camera.main.layerCullDistances = distances;
        Camera.main.layerCullSpherical = true;
    }

    void Update()
    {
        // quit game
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (Walking())
        {
            // if moving after not
            if (mode == 0)
            {
                ResetAfterStillness();
            }

            mode = 1f;
        }
        else
        {
            // start stillness timer
            if (mode == 1)
            {
                mode = 0f;
                time = 0;
                previousApproxTime = -1;
            }

            // check if within bounds of which sculpture
            int inBounds = WhichSculpture();

            // while transitioning to inner worlds
            if (time <= 30f && inBounds >= 0)
            {
                Transition(inBounds);
            }
            // at end of transition period
            else if (inBounds >= 0)
            {
                ActivateInnerWorld(inBounds);
            }
        }

        Vector3 mvmt = ApplyMovement();
        this.transform.Translate(mvmt * Time.deltaTime * speed, Space.World);

        // prevent player from leaving garden
        float z = Mathf.Clamp(transform.position.z, -240, 250);
        float x = Mathf.Clamp(transform.position.x, -130, 140);
        transform.position = new Vector3(x, 3f, z);

        // map rotation from mouse position input
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        transform.localRotation = xQuat * yQuat;

        // if mode has changed
        if (mode != previousMode)
        {
            UpdateChuck();
            previousMode = mode;
        }
    }

    // true if currently using navigation keys
    bool Walking()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) ||
               Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
    }

    // send change in movement to Chuck
    void UpdateChuck()
    {
        GetComponent<ChuckSubInstance>().SetFloat("mode", mode);
        GetComponent<ChuckSubInstance>().BroadcastEvent("changeHappened");
    }

    int WhichSculpture()
    {
        int inBounds = -1;

        for (int i = 0; i < sculptures.Length; i++)
        {
            float dist = Vector3.Distance(sculptures[i].transform.position,
                                          transform.position);

            if (dist <= sculptBounds)
            {
                inBounds = i;
                break;
            }
        }

        return inBounds;
    }

    Vector3 ApplyMovement()
    {
        Vector3 mvmt = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            mvmt = transform.forward;
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {  
            mvmt = -transform.forward;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {  
            mvmt = -transform.right;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            mvmt = transform.right;
        }

        mvmt.y = 0;
        return mvmt;
    }

    void Transition(int inBounds)
    {
        int approxTime = (int) time;

        // change fog distance according to time
        time += Time.deltaTime;
        RenderSettings.fogStartDistance = 50 - Mathf.Exp(0.3f * time);

        if (approxTime == 0)
        {
            // order sculptures by distance to player
            sculptByDist = sculptures.OrderBy(
                (s) => (s.transform.position - transform.position).sqrMagnitude
            ).ToList();

            // trigger filler static fade-outs
            foreach (Transform child in staticParent.transform)
            {
                iTween.AudioTo(child.gameObject, iTween.Hash(
                    "volume", 0,
                    "time", 26,
                    "delay", 7,
                    "easeType", iTween.EaseType.easeInQuad));
            }
        }
        else if (approxTime % 6 == 0 && (approxTime - previousApproxTime != 0))
        {
            // deactivate other sculptures from farthest to nearest
            GameObject toDeact = sculptByDist.Last();

            for (int i = 0; i < sculptures.Length; i++)
            {
                if(sculptures[i].name == toDeact.name)
                {
                    sculptures[i].SetActive(false);
                    sculptByDist.RemoveAt(sculptByDist.Count - 1);
                    break;
                }
            }

            previousApproxTime = approxTime;
        }
    }

    // activate corresponding inner world after 30 secs of stillness
    void ActivateInnerWorld(int inBounds)
    {
        innerWorlds[inBounds].SetActive(true);
        Vector3 pos = sculptures[inBounds].transform.position;
        sculptures[inBounds].GetComponent<ChuckSubInstance>().spatialize = false;

        // remove forest objects in inner world proximity
        foreach (Transform child in forest.transform)
        {
            float xDistance = Mathf.Abs(child.transform.position.x - pos.x);
            float zDistance = Mathf.Abs(child.transform.position.z - pos.z);

            if (xDistance + zDistance < 25)
            {
                child.gameObject.SetActive(false);
            }
            else
            {
                child.gameObject.SetActive(true);
            }
        }

        // make sure all other sculptures are off
        for (int i = 0; i < sculptures.Length; i++)
        {
            if (i != inBounds)
            {
                sculptures[i].SetActive(false);
            }
        }
    }

    // reset graphics & audio after inner world animations
    void ResetAfterStillness()
    {
        RenderSettings.fogStartDistance = 50;
        forest.SetActive(true);

        // reactivate all sculptures
        for (int i = 0; i < sculptures.Length; i++)
        {
            innerWorlds[i].SetActive(false);
            sculptures[i].SetActive(true);
            sculptures[i].GetComponent<ChuckSubInstance>().spatialize = true;
        }

        // turn filler static back on
        foreach (Transform child in staticParent.transform)
        {
            iTween.AudioTo(child.gameObject, iTween.Hash("volume", 1, "time", 0.1f));
        }
    }
}