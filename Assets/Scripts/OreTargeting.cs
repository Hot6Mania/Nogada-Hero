using UnityEngine;
using System.Collections.Generic;

public class OreTargeting : MonoBehaviour
{
    public float targetingRange = 5f;         // 타겟팅 범위
    public LayerMask oreLayer;                // 광물이 배치된 레이어 (예: "Ore")
    public GameObject currentTarget;          // 현재 타겟된 광물

    // Outline 셰이더가 적용된 머티리얼을 인스펙터에서 할당하세요.
    public Material outlineMaterial;

    // 원래 부모 색상을 저장하는 딕셔너리
    private Dictionary<GameObject, Color> originalParentColors = new Dictionary<GameObject, Color>();

    void Start()
    {
        Debug.Log("OreTargeting started.");
    }

    void Update()
    {
        FindNearestOre();
    }

    void FindNearestOre()
    {
        Collider2D[] ores = Physics2D.OverlapCircleAll(transform.position, targetingRange, oreLayer);
        Debug.Log($"Found {ores.Length} ores in range.");
        
        GameObject nearestOre = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D oreCollider in ores)
        {
            float distance = Vector2.Distance(transform.position, oreCollider.transform.position);
            Debug.Log($"Ore {oreCollider.gameObject.name} at distance: {distance}");
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestOre = oreCollider.gameObject;
            }
        }

        if (nearestOre == null)
        {
            Debug.Log("No ore found within targeting range.");
        }

        // 타겟이 바뀌면 이전 광물은 하이라이트 해제하고, 새 광물에 하이라이트 적용
        if (nearestOre != currentTarget)
        {
            if (currentTarget != null)
            {
                RemoveHighlight(currentTarget);
            }
            currentTarget = nearestOre;
            if (currentTarget != null)
            {
                ApplyHighlight(currentTarget);
                Debug.Log("New target applied: " + currentTarget.name);
            }
            else
            {
                Debug.Log("Current target is now null.");
            }
        }
    }

    void ApplyHighlight(GameObject ore)
    {
        SpriteRenderer sr = ore.GetComponent<SpriteRenderer>();
        if (sr == null || outlineMaterial == null)
        {
            Debug.LogWarning("ApplyHighlight: Missing SpriteRenderer or outlineMaterial for " + ore.name);
            return;
        }

        // 부모 SpriteRenderer의 원래 색상을 저장 (한 번만 저장)
        if (!originalParentColors.ContainsKey(ore))
        {
            originalParentColors[ore] = sr.color;
            Debug.Log("Original parent color saved for " + ore.name);
        }
        // 부모의 색상을 연두색으로 변경 (예: 0.5, 1.0, 0.5)
        sr.color = new Color(0.5f, 1.0f, 0.5f, sr.color.a);

        // 이미 Outline 자식 오브젝트가 있는지 확인
        Transform outlineTransform = ore.transform.Find("Outline");
        if (outlineTransform == null)
        {
            // Outline 자식 오브젝트 생성
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(ore.transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;

            // Outline 전용 SpriteRenderer 추가
            SpriteRenderer outlineSR = outlineObj.AddComponent<SpriteRenderer>();
            outlineSR.sprite = sr.sprite;
            outlineSR.material = outlineMaterial;
            outlineSR.sortingLayerID = sr.sortingLayerID;
            // 부모보다 한 단계 낮은 Sorting Order (또는 필요에 따라 조절)
            outlineSR.sortingOrder = sr.sortingOrder - 1;
            // Outline 색상도 연두색으로 설정
            outlineSR.color = new Color(0.5f, 1.0f, 0.5f, sr.color.a);

            // Outline이 살짝 확장되어 보이도록 약간의 스케일 증가 (조절 가능)
            float outlineScaleFactor = 1.1f;
            outlineObj.transform.localScale = new Vector3(outlineScaleFactor, outlineScaleFactor, 1f);

            Debug.Log("ApplyHighlight: Created outline object for " + ore.name);
        }
        else
        {
            // 이미 존재하는 경우 업데이트 (옵션)
            SpriteRenderer outlineSR = outlineTransform.GetComponent<SpriteRenderer>();
            if (outlineSR != null)
            {
                outlineSR.sprite = sr.sprite;
                outlineSR.material = outlineMaterial;
                outlineSR.color = new Color(0.5f, 1.0f, 0.5f, sr.color.a);
            }
            Debug.Log("ApplyHighlight: Updated existing outline object for " + ore.name);
        }
    }

    void RemoveHighlight(GameObject ore)
    {
        // 복원: 부모의 색상과 Outline 자식 오브젝트 제거
        SpriteRenderer sr = ore.GetComponent<SpriteRenderer>();
        if (sr != null && originalParentColors.ContainsKey(ore))
        {
            sr.color = originalParentColors[ore];
            originalParentColors.Remove(ore);
            Debug.Log("RemoveHighlight: Restored original parent color for " + ore.name);
        }

        Transform outlineTransform = ore.transform.Find("Outline");
        if (outlineTransform != null)
        {
            Destroy(outlineTransform.gameObject);
            Debug.Log("RemoveHighlight: Removed outline object for " + ore.name);
        }
    }

    // 타겟팅 범위 시각화를 위한 Gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
