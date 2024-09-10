using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public HeroCharacter trackingTarget;

    public bool isCameraMove; // ���� ������ �ϰ��ִ��� Ȯ���� ���� ����
    public bool isDontMove; //UI�� �� ���� �� �������� �ʰ�
    public bool isCrossLimitLine; //true�϶� ī�޶� �� �ٱ����� ������ �� ����
    public bool isTrackingTarget; //true�϶� Ŭ���� ������ ����
    [SerializeField] private bool onStopTracking = false;

    private new Camera camera;
    private Transform cameraTransform;
    private const float DirectionForceReduceRate = 0.935f; // ���Ӻ���
    private const float DirectionForceMin = 0.001f; // ����ġ ������ ��� �������� ����
    private Vector3 startPosition;  // �Է� ���� ��ġ�� ���
    private Vector3 directionForce; // ������ �������� ������ �����ϸ鼭 �̵� ��Ű��

    [Header("CameraViewBox")]
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);
    private float xMin, xMax, yMin, yMax; //ī�޶� �̵��� �����ϴ� 4���� ��ǥ
    [SerializeField] private float viewSize_Default = 0f;
    [SerializeField] private float viewSize_Tracking = 0f;
    
    void Awake()
    {
        camera = Camera.main;
        cameraTransform = camera.transform;
    }
    void Update()
    {
        StatusUpdate();

        if (!isDontMove)
        {
            // ī�޶� ������ �̵�
            ControlCameraPosition();

            // ������ �������� ����
            ReduceDirectionForce();

            // ī�޶� ��ġ ������Ʈ
            UpdateCameraPosition();
        }

        if (!isCrossLimitLine)
        {
            //ī�޶� �̵� ������ǥ ������Ʈ
            LimitPositionSet();

            //ī�޶� �� ���� ������ ���ϰ� �̵�
            CameraMoveLimit();
        }
        
    }
    protected void StatusUpdate()
    {
        if (trackingTarget != null)
        {
            isTrackingTarget = true;
            Camera.main.orthographicSize = viewSize_Tracking;
        }
        else
        {
            isTrackingTarget = false;
            Camera.main.orthographicSize = viewSize_Default;
        }
    }
    protected void LimitPositionSet()
    {
        xMin = -boxSize.x / 2;
        xMax = boxSize.x / 2;
        yMin = -boxSize.y / 2;
        yMax = boxSize.y / 2;
    }

    protected void ControlCameraPosition()
    {
#if UNITY_EDITOR
        //�����Ϳ��� Scene, Simulator���� �� ��� �����޽��� ������
        if(EditorWindow.focusedWindow == null || EditorWindow.focusedWindow.titleContent.text != "Game" && EditorWindow.focusedWindow.titleContent.text != "Simulator")
        {
            return;
        }
#endif
        if (isTrackingTarget)
        {
            Vector3 pos = trackingTarget.myObject.position;
            cameraTransform.position = new Vector3(pos.x, pos.y, cameraTransform.position.z);

            Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && !onStopTracking)
            {
                StartCoroutine(StopTracking(mouseWorldPosition));
            }
        }
        else
        {
            Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                CameraPositionMoveStart(mouseWorldPosition);
            }
            else if (Input.GetMouseButton(0))
            {
                CameraPositionMoveProgress(mouseWorldPosition);
            }
            else
            {
                CameraPositionMoveEnd();
            }
        }
    }
    protected IEnumerator StopTracking(Vector3 clickPosition)
    {
        onStopTracking = true;

        Ray ray = Camera.main.ScreenPointToRay(clickPosition);
        RaycastHit hit;
        isDontMove = true;

        if (Physics.Raycast(ray, out hit))
        {
            yield return 0;
        }
        else if(trackingTarget != null) 
        {
            trackingTarget = null;
        }

        //�����̸� ���� ������ CameraDrag ��ũ��Ʈ���� ī�޶� ����������
        yield return new WaitForSeconds(0.3f);
        isDontMove = false;
        onStopTracking = false;
    }

    protected void CameraPositionMoveStart(Vector3 startPosition)
    {
        isCameraMove = true;
        this.startPosition = startPosition;
        directionForce = Vector2.zero;
    }
    protected void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (isCameraMove == false)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        directionForce = startPosition - targetPosition;
    }
    protected void CameraPositionMoveEnd()
    {
        isCameraMove = false;
    }
    protected void ReduceDirectionForce()
    {
        // ���� ���϶��� �ƹ��͵� ����
        if (isCameraMove == true)
        {
            return;
        }

        // ���� ��ġ ����
        directionForce *= DirectionForceReduceRate;

        // ���� ��ġ�� �Ǹ� ������ ����
        if (directionForce.magnitude < DirectionForceMin)
        {
            directionForce = Vector3.zero;
        }
    }
    protected void UpdateCameraPosition()
    {
        // �̵� ��ġ�� ������ �ƹ��͵� ����
        if (directionForce == Vector3.zero)
        {
            return;
        }

        var currentPosition = transform.position;
        var targetPosition = currentPosition + directionForce;
        transform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    protected void CameraMoveLimit()
    {
        Vector3 CameraPos = cameraTransform.position;
        
        float cameraHalfWidth = camera.orthographicSize * camera.aspect;
        float cameraHalfHeight = camera.orthographicSize;

        // �������� ��� ����
        float minX = xMin + cameraHalfWidth;
        float maxX = xMax - cameraHalfWidth;
        float minY = yMin + cameraHalfHeight;
        float maxY = yMax - cameraHalfHeight;

        // Check if the camera bounds are smaller than the stage bounds
        bool isCameraWiderThanStage = cameraHalfWidth * 2 >= (xMax - xMin);
        bool isCameraTallerThanStage = cameraHalfHeight * 2 >= (yMax - yMin);

        if (isCameraWiderThanStage)
        {
            // Center the camera horizontally if the stage is narrower than the camera
            CameraPos.x = (xMin + xMax) / 2;
        }
        else
        {
            // Otherwise, clamp the camera position horizontally
            CameraPos.x = Mathf.Clamp(CameraPos.x, minX, maxX);
        }

        if (isCameraTallerThanStage)
        {
            // Center the camera vertically if the stage is shorter than the camera
            CameraPos.y = (yMin + yMax) / 2;
        }
        else
        {
            // Otherwise, clamp the camera position vertically
            CameraPos.y = Mathf.Clamp(CameraPos.y, minY, maxY);
        }

        transform.position = CameraPos;
    }

#if UNITY_EDITOR
    int segments = 100;
    bool drawWhenSelected = true;

    void OnDrawGizmosSelected()
    {
        if (drawWhenSelected)
        {
            //Ž�� �þ�
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }


#endif

}
