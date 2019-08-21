using UnityEngine;

public class TouchCamera : MonoBehaviour
{
    Vector2?[] oldTouchPositions = {
        null,
        null
    };
    Vector2 oldTouchVector;
    float oldTouchDistance;

    Camera cam;

    void Start()
    {
        cam = this.GetComponent<Camera>();
    }

    void Update()
    {

        if (Input.touchCount == 0)
        {
            oldTouchPositions[0] = null;
            oldTouchPositions[1] = null;
        }
        else if (Input.touchCount == 1)
        {

            if (oldTouchPositions[0] == null || oldTouchPositions[1] != null)
            {
                oldTouchPositions[0] = Input.GetTouch(0).position;
                oldTouchPositions[1] = null;
            }
            else
            {
                Vector2 newTouchPosition = Input.GetTouch(0).position;

                transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] - newTouchPosition) * cam.orthographicSize / cam.pixelHeight * 2f));

                oldTouchPositions[0] = newTouchPosition;
            }

        }
        else
        {

            if (oldTouchPositions[1] == null)
            {
                oldTouchPositions[0] = Input.GetTouch(0).position;
                oldTouchPositions[1] = Input.GetTouch(1).position;
                oldTouchVector = (Vector2)(oldTouchPositions[0] - oldTouchPositions[1]);
                oldTouchDistance = oldTouchVector.magnitude;
            }
            else
            {
                Vector2 screen = new Vector2(cam.pixelWidth, cam.pixelHeight);

                Vector2[] newTouchPositions = {
                    Input.GetTouch(0).position,
                    Input.GetTouch(1).position
                };

                Vector2 newTouchVector = newTouchPositions[0] - newTouchPositions[1];
                float newTouchDistance = newTouchVector.magnitude;

                transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] + oldTouchPositions[1] - screen) * cam.orthographicSize / screen.y));
                //transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Asin(Mathf.Clamp((oldTouchVector.y * newTouchVector.x - oldTouchVector.x * newTouchVector.y) / oldTouchDistance / newTouchDistance, -1f, 1f)) / 0.0174532924f));
                cam.orthographicSize *= oldTouchDistance / newTouchDistance;
                transform.position -= transform.TransformDirection((newTouchPositions[0] + newTouchPositions[1] - screen) * cam.orthographicSize / screen.y);

                oldTouchPositions[0] = newTouchPositions[0];
                oldTouchPositions[1] = newTouchPositions[1];
                oldTouchVector = newTouchVector;
                oldTouchDistance = newTouchDistance;
            }

        }

        if (Mathf.Abs(transform.position.x) > 28f)
            transform.position = new Vector3(Mathf.Sign(transform.position.x) * 28f, transform.position.y, -10);
        if (transform.position.y > 18f)
            transform.position = new Vector3(transform.position.x, 18f, -10);
        if (transform.position.y < -9f)
            transform.position = new Vector3(transform.position.x, -9f, -10);
        if (cam.orthographicSize < 1f)
            cam.orthographicSize = 1f;
        if (cam.orthographicSize > 12f)
            cam.orthographicSize = 12f;

    }
}
