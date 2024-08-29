using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GetTilePostion : MonoBehaviour
{
    public Tilemap tilemap; // Ÿ�ϸ��� ����
    public Transform player; // �÷��̾��� Transform
    [SerializeField] Vector3 currentTilePosition;
    void Update()
    {
        // �÷��̾��� ���� ��ġ���� Ÿ�� ��ǥ�� ����
        Vector3Int tilePosition = GetTilePositionUnderPlayer(player.position);
        currentTilePosition = tilePosition;
        // ����� �������� �ֿܼ� ���
        Debug.Log("Player is standing on tile at: " + tilePosition);
    }

    public Vector3Int GetTilePositionUnderPlayer(Vector3 worldPosition)
    {
        // ���� ��ǥ�� Ÿ�ϸ��� �� ��ǥ�� ��ȯ
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        // Ÿ�ϸ��� ������ Ȯ��
        BoundsInt bounds = tilemap.cellBounds;

        // ���� ����� z ���� ã�� ���� ���� �ʱ�ȭ
        float closestZ = float.MaxValue;
        Vector3Int closestCellPosition = cellPosition;

        // Z ���� �������� ��ȸ�ϸ� ���� ����� Ÿ�� ã��
        for (int z = bounds.zMin; z < bounds.zMax; z++)
        {
            Vector3Int checkPosition = new Vector3Int(cellPosition.x, cellPosition.y, z);

            if (tilemap.HasTile(checkPosition))
            {
                // ���� Z�� ���� ��ǥ ���ϱ�
                Vector3 tileWorldPosition = tilemap.GetCellCenterWorld(checkPosition);

                // ���� ��ǥ�� Ÿ���� Z ������ ��
                float distance = Mathf.Abs(worldPosition.z - tileWorldPosition.z);

                if (distance < closestZ)
                {
                    closestZ = distance;
                    closestCellPosition = checkPosition;
                }
            }
        }

        // ���� ����� Z �������� Ÿ�� �� ��ǥ ��ȯ
        return closestCellPosition;
    }
}

