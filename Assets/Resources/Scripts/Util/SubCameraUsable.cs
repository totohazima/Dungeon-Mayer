using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCameraUsable : MonoBehaviour
{
    private Camera subCamera;
    public CameraController_InGame mainCamera;
    public Transform subCameraTransform;

    [Header("CameraAnimationList")]
    public List<IEnumerator> cameraAnimList = new List<IEnumerator>();
    private bool isRunningCoroutine = false;
    void Awake()
    {
        subCamera = GetComponent<Camera>();
        subCameraTransform = subCamera.transform;
        
        mainCamera = GetComponentInParent<CameraController_InGame>();
        subCamera.orthographicSize = mainCamera.viewSize_Default;

        subCamera.enabled = false;
    }
    
    public void AddCoroutine(IEnumerator coroutine)
    { 
        cameraAnimList.Add(coroutine);

        // 코루틴이 실행 중이지 않다면 바로 실행 시작
        if (!isRunningCoroutine)
        {
            StartCoroutine(ExecuteCoroutines());
        }
    }

    private IEnumerator ExecuteCoroutines()
    {
        isRunningCoroutine = true;

        while (cameraAnimList.Count > 0)
        {
            subCamera.enabled = true;
            mainCamera.isDontMove = true;
            // 리스트에서 첫 번째 코루틴을 가져와 실행
            IEnumerator currentCoroutine = cameraAnimList[0];
            yield return StartCoroutine(currentCoroutine);

            // 실행이 끝난 후 리스트에서 제거
            cameraAnimList.RemoveAt(0);
        }

        isRunningCoroutine = false;
        subCamera.enabled = false;
        mainCamera.isDontMove = false;
    }

    //카메라가 보스 스폰 위치로 움직이게 함
    public IEnumerator CameraBossTracking(Vector3 startPos, Vector3 endPos, float moveSpeed, FieldActivity field)
    {
        subCamera.orthographicSize = mainCamera.viewSize_Default;

        float elapsedTime = 0f;
        float totalDistance = Vector3.Distance(startPos, endPos);
        Vector3 trueEndPos = new Vector3(endPos.x, endPos.y, -10);

        while (Vector3.Distance(subCamera.transform.position, trueEndPos) > 0.01f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            float RatioDistance = elapsedTime / totalDistance;

            subCamera.transform.position = Vector3.Lerp(startPos, trueEndPos, RatioDistance);

            yield return null;
        }

        subCamera.transform.position = trueEndPos;

        yield return new WaitForSeconds(3f);

        field.currentBossSpawning = false;
    }
}
