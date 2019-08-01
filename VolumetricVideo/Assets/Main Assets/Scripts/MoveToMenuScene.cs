/// <summary>
/// This file is meant to implement functionality of proximity based video playing 
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToMenuScene : MonoBehaviour
{

    public GameObject user;
    private int allowedDistance = 5;
    private Vector3 pos;
    bool transitioned = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        pos = gameObject.transform.position;
        MoveToMenu(user);
    }

    /// <summary>
    /// Checks distance between two objects and plays/pauses video based on distance
    /// </summary>
    /// 
    /// <param name="objectOne">
    /// One of the objects of interest in calculating distance with another
    /// </param>

    void MoveToMenu(GameObject objectOne)
    {
  

        if ((Mathf.Abs(Vector3.Distance(objectOne.transform.position, pos)) <= allowedDistance) && !transitioned)
        {
            SceneManager.LoadScene("LevelTwo", LoadSceneMode.Additive);
            transitioned = true;
        }
    }
}