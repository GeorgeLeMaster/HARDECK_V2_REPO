using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    public float panSpeed;

    public float borderWidth;

    public Transform positionAnchor;
    public Transform rotationAnchor;

    private Vector2 lowerBound;
    private Vector2 upperBound;

    public Vector2 cameraZoomLimits;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

        lowerBound = new Vector2(-5f, -5f);
        //  upperBound = new Vector2(MapBuilder.instance.mapSize.z +5f, MapBuilder.instance.mapSize.x + 5f);
        upperBound = new Vector2(5f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 transformVec = new Vector3(Screen.width/2 - Input.mousePosition.x, 0,Screen.height/2 - Input.mousePosition.y);
        transformVec = transformVec.normalized;
        //Debug.Log(transformVec);


        if (Input.mouseScrollDelta.y != 0)
        {
            float fov = cam.fieldOfView;
            fov -= Input.mouseScrollDelta.y;
            fov = Mathf.Clamp(fov, cameraZoomLimits.x, cameraZoomLimits.y);
            cam.fieldOfView = fov;
        }

        if (Input.mousePosition.x > Screen.width - borderWidth)
        {
            positionAnchor.transform.Translate(transformVec * -panSpeed * Time.deltaTime);
        }
        else if (Input.mousePosition.x < borderWidth)
        {
            positionAnchor.transform.Translate(transformVec * -panSpeed * Time.deltaTime);

        }

        if (Input.mousePosition.y > Screen.height - borderWidth)
        {
            positionAnchor.transform.Translate(transformVec * -panSpeed * Time.deltaTime);
        }
        else if (Input.mousePosition.y < borderWidth)
        {
            positionAnchor.transform.Translate(transformVec * -panSpeed * Time.deltaTime);

        }

        Vector3 pos = positionAnchor.position;
        float x = Mathf.Clamp(pos.x, lowerBound.x, upperBound.x);
        float z = Mathf.Clamp(pos.z, lowerBound.y, upperBound.y);

        pos = new Vector3(x, 0, z);
        positionAnchor.position = pos;

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            positionAnchor.Rotate(Vector3.up * 100 * Time.deltaTime * Input.GetAxis("Mouse X"));
        }

        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
