using System.Collections;
using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    public Controller controller;
    public float moveSpeed = 5f;
    public SPUM_Prefabs spumAnim; // 애니메이션 재생용 스크립트 참조
    public OreTargeting oreTargeting;       // 플레이어에 부착된 OreTargeting 스크립트 참조
    public InventoryManager inventory;      // 인벤토리 관리 스크립트 참조

    private Vector2 lastDirection = Vector2.zero;

    private Vector2 SnapTo8Directions(Vector2 input)
    {
        if (input == Vector2.zero) return Vector2.zero;

        Vector2[] directions = new Vector2[]
        {
        new Vector2(0, 1),    // ↑
        new Vector2(1, 1),    // ↗
        new Vector2(1, 0),    // →
        new Vector2(1, -1),   // ↘
        new Vector2(0, -1),   // ↓
        new Vector2(-1, -1),  // ↙
        new Vector2(-1, 0),   // ←
        new Vector2(-1, 1)    // ↖
        };

        Vector2 normalizedInput = input.normalized;
        float maxDot = -Mathf.Infinity;
        Vector2 closest = Vector2.zero;

        foreach (Vector2 dir in directions)
        {
            float dot = Vector2.Dot(normalizedInput, dir.normalized);
            if (dot > maxDot)
            {
                maxDot = dot;
                closest = dir.normalized;
            }
        }

        return closest;
    }

    public void OnMineButtonClick()
    {
        if (lastDirection.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (lastDirection.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // 타겟이 설정되어 있다면 광물 채집 처리
        if (oreTargeting != null && oreTargeting.currentTarget != null)
        {
            // 예: 광물 오브젝트에 붙은 OreItem 스크립트에서 oreType을 가져온다고 가정
            OreItem oreItem = oreTargeting.currentTarget.GetComponent<OreItem>();
            if (oreItem != null)
            {
                // 인벤토리에 추가
                inventory.AddItem(oreItem.oreType);
            }
            // 광물 제거
            Destroy(oreTargeting.currentTarget);
            oreTargeting.currentTarget = null;
        }

        StartCoroutine(PlayFastAttack());
    }

    IEnumerator PlayFastAttack()
    {
        spumAnim._anim.speed = 1.5f;
        spumAnim.PlayAnimation(PlayerState.ATTACK, 0);
        yield return new WaitForSeconds(0.5f); // 빠른 재생 고려해서 짧게 대기
        spumAnim._anim.speed = 1.0f;
    }
    void Update()
    {
        Vector2 direction = controller.Direction;
        direction = SnapTo8Directions(direction); // 여기서 8방향으로 스냅

        Vector3 move = new Vector3(direction.x, direction.y, 0f);

        if (move != Vector3.zero)
        {
            transform.position += move * moveSpeed * Time.deltaTime;

            if (direction.x < 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else if (direction.x > 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            if (lastDirection == Vector2.zero)
                spumAnim.PlayAnimation(PlayerState.MOVE, 0);
        }
        else
        {
            if (lastDirection != Vector2.zero)
                spumAnim.PlayAnimation(PlayerState.IDLE, 0);
        }

        lastDirection = direction;
    }


}
