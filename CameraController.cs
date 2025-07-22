using UnityEngine;

public class CameraController : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Vector3 otherPos;
    private Vector3 og_pos;

    void Start()
    {
        
        og_pos = transform.position;
        otherPos = GameObject.Find("Background").transform.position + og_pos;
    }

    // Update is called once per frame
    void Update()
    {

        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gameObject.transform.position = og_pos;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            gameObject.transform.position = otherPos;
        }
    }
}
