/// <summary>
/// This file is meant to implement functionality of proximity based video playing 
/// </summary>

using UnityEngine;
using UnityEngine.Video;

public class VideoProximity : MonoBehaviour
{

    public GameObject user;
    public GameObject userCamera;
    public int allowedDistance;
    VideoPlayer vp;
    bool looking;

    // Start is called before the first frame update
    void Start()
    {
        vp = gameObject.GetComponent<VideoPlayer>();
        vp.Prepare();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        vp.Play();
        LocationPausePlayVideo(user);
    }

    /// <summary>
    /// Checks distance between two objects and plays/pauses video based on distance
    /// </summary>
    /// 
    /// <param name="objectOne">
    /// One of the objects of interest in calculating distance with another
    /// </param>

    void LocationPausePlayVideo(GameObject objectOne)
    {

        Vector3 pos = vp.transform.position;
        RaycastHit hit;


        if (Physics.Raycast(user.transform.position, userCamera.transform.forward, out hit, allowedDistance)) 
        {

            if (hit.collider.gameObject.CompareTag("Video"))
            {
                Debug.Log("Looking");
                looking = true;
            }
            else
            {
                looking = false;
            }
        } 

        if ((Mathf.Abs(Vector3.Distance(objectOne.transform.position, pos)) <= allowedDistance) && looking)
        {
            vp.Play();

        }
        else if(Mathf.Abs(Vector3.Distance(objectOne.transform.position, pos)) > allowedDistance || !looking)
        {
            vp.Pause();
        }
    }
}