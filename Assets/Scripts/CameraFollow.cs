using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("플레이어 대상")]
    public Transform target;       // 플레이어의 Transform을 할당합니다.
    
    [Header("스무딩 설정")]
    public float smoothSpeed = 0.125f; // 카메라 이동 스무딩 계수
    public Vector3 offset = new Vector3(0, 0, -10); // 카메라 오프셋 (z값은 카메라가 2D 씬에서 올바른 뷰를 유지하도록 -10 등으로 설정)

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어 위치 + 오프셋을 원하는 위치로 지정
        Vector3 desiredPosition = target.position + offset;
        
        // 스무딩(Lerp)을 이용해 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // z값은 그대로 유지하면서 카메라 위치 갱신
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, offset.z);
    }
}
