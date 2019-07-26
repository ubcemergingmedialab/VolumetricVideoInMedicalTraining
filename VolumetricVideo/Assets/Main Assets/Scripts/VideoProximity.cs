/// <summary>
/// This file is meant to implement functionality of proximity based video playing 
/// </summary>

using UnityEngine;
using UnityEngine.Video;

public class VideoProximity : MonoBehaviour
{

    public GameObject user;
    public int allowedDistance;
    VideoPlayer vp;

    // Start is called before the first frame update
    void Start()
    {
        vp = gameObject.GetComponent<VideoPlayer>();

        vp.Prepare();
    }

    // Update is called once per frame
    void LateUpdate()
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

        if (Mathf.Abs(Vector3.Distance(objectOne.transform.position, pos)) <= allowedDistance)
        {
            Debug.Log("Entered the zone");
            vp.Play();
        }

        if (Mathf.Abs(Vector3.Distance(objectOne.transform.position, pos)) > allowedDistance)
        {
            Debug.Log("Exited the zone");
            vp.Pause();
        }
    }

}