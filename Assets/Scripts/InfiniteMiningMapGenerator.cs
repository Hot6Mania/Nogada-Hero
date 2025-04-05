using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OreProbability
{
    public GameObject orePrefab; // 광물 프리팹
    public float probability = 1f; // 생성 확률 (가중치)
}

public class InfiniteMiningMapGenerator : MonoBehaviour
{
    [Header("배경 및 광물 프리팹")]
    public GameObject backgroundPrefab; // MiningBackGround 프리팹 (SpriteRenderer 포함, 피벗 중앙 권장)
    
    [Header("광물 설정")]
    public List<OreProbability> oreProbabilities = new List<OreProbability>(); // 각 광물의 생성 확률 관리

    [Header("설정")]
    public Vector2 chunkSize = new Vector2(10f, 10f);   // 한 Chunk의 가로, 세로 크기
    public int oresPerChunk = 1;        // Chunk 당 생성할 광물 개수
    public Vector2 oreSpawnPadding = new Vector2(1f, 1f); // 청크 경계에서 약간의 여유 공간
    public Vector3 oreScaleRange = new Vector3(0.8f, 1.2f, 1f); // (min, max, 0): 광물 스케일 랜덤 범위

    [Header("참조")]
    public Transform player;
    public Transform mapParent;         // 생성된 배경들의 부모 오브젝트

    // 현재 생성된 청크를 (x,y) 좌표로 관리 (플레이어가 있는 청크를 중심으로)
    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastCenterChunk;

    void Start()
    {
        lastCenterChunk = GetChunkCoordFromPosition(player.position);
        GenerateInitialChunks(lastCenterChunk);
    }

    void Update()
    {
        Vector2Int currentChunk = GetChunkCoordFromPosition(player.position);
        if (currentChunk != lastCenterChunk)
        {
            UpdateChunks(currentChunk);
            lastCenterChunk = currentChunk;
        }
    }

    // 플레이어 위치로부터 청크 좌표 계산 (플레이어가 속한 청크의 좌표)
    Vector2Int GetChunkCoordFromPosition(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / chunkSize.x);
        int y = Mathf.FloorToInt(pos.y / chunkSize.y);
        return new Vector2Int(x, y);
    }

    // 초기 3x3 그리드 청크 생성 (플레이어 청크를 중앙에)
    void GenerateInitialChunks(Vector2Int centerChunk)
    {
        for (int x = centerChunk.x - 1; x <= centerChunk.x + 1; x++)
        {
            for (int y = centerChunk.y - 1; y <= centerChunk.y + 1; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                GenerateChunkAt(coord);
            }
        }
    }

    void GenerateChunkAt(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            return;

        // 청크 중심 좌표 계산 (피벗이 중앙인 경우)
        Vector3 chunkCenter = new Vector3(
            coord.x * chunkSize.x + chunkSize.x * 0.5f,
            coord.y * chunkSize.y + chunkSize.y * 0.5f,
            0);

        // 배경 생성
        GameObject bg = Instantiate(backgroundPrefab, chunkCenter, Quaternion.identity, mapParent);

        // 배경 Sprite의 크기를 청크 크기에 맞게 조절 (피벗 중앙 기준)
        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            float scaleX = chunkSize.x / spriteSize.x;
            float scaleY = chunkSize.y / spriteSize.y;
            bg.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        chunks.Add(coord, bg);
        SpawnOresInChunk(chunkCenter);
    }

    // 플레이어 주변 청크 업데이트 (3x3 그리드 유지)
    void UpdateChunks(Vector2Int centerChunk)
    {
        // 생성할 대상: centerChunk 주변 3x3
        for (int x = centerChunk.x - 1; x <= centerChunk.x + 1; x++)
        {
            for (int y = centerChunk.y - 1; y <= centerChunk.y + 1; y++)
            {
                GenerateChunkAt(new Vector2Int(x, y));
            }
        }
        // (옵션) 일정 범위 밖의 청크 제거 코드 추가 가능
    }

    // 확률에 따라 광물 프리팹 선택
    GameObject GetRandomOrePrefab()
    {
        float totalWeight = 0f;
        foreach (OreProbability op in oreProbabilities)
        {
            totalWeight += op.probability;
        }
        float randomValue = Random.Range(0, totalWeight);
        float cumulative = 0f;
        foreach (OreProbability op in oreProbabilities)
        {
            cumulative += op.probability;
            if (randomValue <= cumulative)
            {
                return op.orePrefab;
            }
        }
        return oreProbabilities[oreProbabilities.Count - 1].orePrefab;
    }

    void SpawnOresInChunk(Vector3 chunkCenter)
    {
        // 광물 개수를 랜덤으로 지정할 경우 oresPerChunk 대신 아래처럼 할 수 있음:
        int oreCount = Random.Range(0, oresPerChunk + 1);
        Debug.Log($"Ore Count: {oreCount}");
        for (int i = 0; i < oreCount; i++)
        {
            // 확률에 따라 광물 프리팹 선택
            GameObject orePrefab = GetRandomOrePrefab();

            // 중앙 기준, -반쪽 ~ +반쪽 범위에서 랜덤 오프셋
            float xOffset = Random.Range(-chunkSize.x * 0.5f + oreSpawnPadding.x, chunkSize.x * 0.5f - oreSpawnPadding.x);
            float yOffset = Random.Range(-chunkSize.y * 0.5f + oreSpawnPadding.y, chunkSize.y * 0.5f - oreSpawnPadding.y);
            Vector3 orePos = chunkCenter + new Vector3(xOffset, yOffset, 0);

            // 광물 생성 (mapParent의 자식으로 배치)
            GameObject ore = Instantiate(orePrefab, orePos, Quaternion.identity, mapParent);

            // oreScaleRange 내에서 랜덤 스케일 적용 (x,y 동일)
            float scaleFactor = Random.Range(oreScaleRange.x, oreScaleRange.y);
            ore.transform.localScale *= scaleFactor;

            // SpriteRenderer가 있다면, sortingOrder를 설정해 배경보다 앞에 렌더링
            SpriteRenderer sr = ore.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 1;
            }
        }
    }
}
