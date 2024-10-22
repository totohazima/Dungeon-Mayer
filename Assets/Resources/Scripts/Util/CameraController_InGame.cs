using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEditor;

public class CameraController_InGame : MonoBehaviour, ICustomUpdateMono
{
    public Camera mainCamera;
    public HeroCharacter trackingTarget;
    public SubCameraUsable subCameraUsable;
    public Transform cameraTransform;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public float minZoom;
    public float maxZoom;
    public float currentZoom;
    [Header("CameraViewBox")]
    public Transform viewBox;
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);
    public float viewSize_Default = 0f;
    public float viewSize_Tracking = 0f;
    private float xMin, xMax, yMin, yMax; //카메라 이동을 제한하는 4방향 좌표

    [Header("CameraSettings")]
    public bool isZoomDisable = false; //줌 정지
    public bool isDragDisable = false; //드래그 정지
    public bool isDontMove = false;
    public bool isCameraMove = false;
    public bool isTrackingTarget = false;
    [HideInInspector] public bool onStopTracking = false;

    private const float DirectionForceReduceRate = 0.935f; // 감속비율
    private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤
    private Vector3 startPosition;  // 입력 시작 위치를 기억
    private Vector3 directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();
        if (subCameraUsable == null)
            subCameraUsable = GetComponentInChildren<SubCameraUsable>();
        if (cameraTransform == null)
            cameraTransform = transform;

        Recycle();

        LimitPositionSet();
    }

    private void Recycle()
    {
        currentZoom = viewSize_Default;
        CameraViewSizing(currentZoom);
    }
    private void OnEnable()
    {
        CustomUpdateManager.customUpdateMonos.Add(this);
    }
    private void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);
    }

    public void CustomUpdate()
    {
        StatusUpdate();
        if (!isDontMove)
        {
            HandleZoom();
            HandleMovement();
            ReduceDirectionForce();
            UpdateCameraPosition();
        }
        
        ClampCamera();
        
    }
    protected void StatusUpdate()
    {
        if (trackingTarget != null)
        {
            isTrackingTarget = true;
            CameraViewSizing(viewSize_Tracking);
        }
        else
        {
            isTrackingTarget = false;
            CameraViewSizing(currentZoom);
        }
    }

    private void HandleZoom()
    {
        if (isZoomDisable)
        {
            return;
        }

        float zoomDelta = 0;
        float betaZoomSpeed = 0;
        if (Input.touchCount >= 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            zoomDelta = prevMagnitude - currentMagnitude;

            betaZoomSpeed = zoomSpeed;
        }
        else
        {
            // 마우스 휠이 실제로 움직일 때만 줌 적용
            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (scrollValue != 0) // 휠이 움직인 경우에만
            {
                zoomDelta = -scrollValue; // 휠의 방향에 따라 줌 적용
            }
            betaZoomSpeed = zoomSpeed * 200;
        }

        currentZoom += zoomDelta * betaZoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    protected void HandleMovement()
    {
#if UNITY_EDITOR
        //에디터에서 Scene으로 볼 경우 에러메시지 대응용
        if (EditorWindow.focusedWindow == null || EditorWindow.focusedWindow.titleContent.text != "Game" && EditorWindow.focusedWindow.titleContent.text != "Simulator")
        {
            return;
        }
#endif

        if (isDragDisable)
        {
            return;
        }

        if (isTrackingTarget)
        {
            Vector3 pos = trackingTarget.myObject.position;
            cameraTransform.position = new Vector3(pos.x, pos.y, cameraTransform.position.z);

            isZoomDisable = true;
            // 모바일 터치 또는 PC 클릭
            if (IsTouchOrClick() && !IsPointerOverUI())
            {
                Vector3 clickPosition = GetTouchOrMouseWorldPosition();
                StartCoroutine(StopTracking(clickPosition));
            }
        }
        else
        {
            Vector3 mouseWorldPosition = GetTouchOrMouseWorldPosition();

            // 두 개 이상의 터치가 있는 경우 드래그 로직을 건너뛰기
            if (Input.touchCount >= 2)
            {
                isCameraMove = false;
                isZoomDisable = false;

                return;
            }

            if (IsTouchOrClick() && !IsPointerOverUI())
            {
                CameraPositionMoveStart(mouseWorldPosition);
                isZoomDisable = true;
            }
            else if (IsTouchHeld() && !IsPointerOverUI())
            {
                CameraPositionMoveProgress(mouseWorldPosition);
                isZoomDisable = true;
            }
            else
            {
                isCameraMove = false;
                isZoomDisable = false;
            }
        }
    }
    void CameraPositionMoveStart(Vector3 startPosition)
    {
        isCameraMove = true;
        this.startPosition = startPosition;
        directionForce = Vector2.zero;
    }
    void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!isCameraMove)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }
        // directionForce 연산 최소화
        directionForce = startPosition - targetPosition;
    }
    protected void ReduceDirectionForce()
    {
        // 조작 중일때는 아무것도 안함
        if (isCameraMove)
        {
            return;
        }

        // 감속 수치 적용
        directionForce *= DirectionForceReduceRate;

        // 작은 수치가 되면 강제로 멈춤
        if (directionForce.magnitude < DirectionForceMin)
        {
            directionForce = Vector3.zero;
        }
    }

    protected void UpdateCameraPosition()
    {
        // 이동 수치가 없으면 아무것도 안함
        if (directionForce == Vector3.zero)
        {
            return;
        }

        var currentPosition = cameraTransform.position;
        var targetPosition = currentPosition + directionForce;
        cameraTransform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    private void ClampCamera()
    {
        Vector3 CameraPos = cameraTransform.position;

        float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraHalfHeight = mainCamera.orthographicSize;

        // 스테이지 경계 제한
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
    protected IEnumerator StopTracking(Vector3 clickPosition)
    {
        onStopTracking = true;
        isDontMove = true;

        Ray ray = mainCamera.ScreenPointToRay(clickPosition); // Camera.main 대신 camera 사용
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 필요한 경우에만 처리
            yield return null;
        }
        else if (trackingTarget != null)
        {
            trackingTarget = null;
        }

        yield return new WaitForSeconds(0.3f);

        isDontMove = false;
        onStopTracking = false;
    }


    protected void LimitPositionSet()
    {
        xMin = -boxSize.x / 2 + viewBox.transform.position.x;
        xMax = boxSize.x / 2 + viewBox.transform.position.x;
        yMin = -boxSize.y / 2 + viewBox.transform.position.y;
        yMax = boxSize.y / 2 + viewBox.transform.position.y;
    }
    bool IsTouchOrClick()
    {
        return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    // 터치 또는 마우스 드래그 감지 함수
    bool IsTouchHeld()
    {
        return Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
    }

    // 터치 또는 마우스 드래그가 UI위에서 작동했는지 감지 함수
    bool IsPointerOverUI()
    {
        if (Input.touchCount > 0)  //모바일
        {
            Touch touch = Input.GetTouch(0);
            return EventSystem.current.IsPointerOverGameObject(touch.fingerId); // 터치의 fingerId를 사용하여 UI 검사
        }
        else //PC 마우스
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
    Vector3 GetTouchOrMouseWorldPosition()
    {
        if (Input.touchCount > 0)
        {
            return mainCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
        }
        else
        {
            return mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    void CameraViewSizing(float viewSize)
    {
        if(mainCamera.orthographic)
        {
            mainCamera.orthographicSize = viewSize;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        //탐지 시야
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(viewBox.position, boxSize);
    }
#endif
}
