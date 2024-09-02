using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    public Tilemap tilemap;
    public HunterCharacter trackingTarget;
    private new Camera camera;
    private Transform cameraTransform;
    private const float DirectionForceReduceRate = 0.935f; // ���Ӻ���
    private const float DirectionForceMin = 0.001f; // ����ġ ������ ��� �������� ����
    private Vector3 startPosition;  // �Է� ���� ��ġ�� ���
    private Vector3 directionForce; // ������ �������� ������ �����ϸ鼭 �̵� ��Ű��
    private float xMin, xMax, yMin, yMax; //ī�޶� �̵��� �����ϴ� 4���� ��ǥ
    public bool isCameraMove; // ���� ������ �ϰ��ִ��� Ȯ���� ���� ����
    public bool isDontMove; //UI�� �� ���� �� �������� �ʰ�
    public bool isCrossLimitLine; //true�϶� ī�޶� �� �ٱ����� ������ �� ����
    public bool isTrackingTarget; //true�϶� Ŭ���� ������ ����
    
    private void Awake()
    {
        camera = Camera.main;
        cameraTransform = camera.transform;
    }
    private void Update()
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
            Camera.main.orthographicSize = 2f;
        }
        else
        {
            isTrackingTarget = false;
            Camera.main.orthographicSize = 3f;
        }
    }
    private void LimitPositionSet()
    {
        if(tilemap == null)
        {
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        xMin = bounds.xMin;
        xMax = bounds.xMax;
        yMin = bounds.yMin;
        yMax = bounds.yMax;
    }

    private void ControlCameraPosition()
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
    private void CameraPositionMoveStart(Vector3 startPosition)
    {
        isCameraMove = true;
        this.startPosition = startPosition;
        directionForce = Vector2.zero;
    }
    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (isCameraMove == false)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        directionForce = startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        isCameraMove = false;
    }
    private void ReduceDirectionForce()
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
    private void UpdateCameraPosition()
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

    private void CameraMoveLimit()
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
    
}
