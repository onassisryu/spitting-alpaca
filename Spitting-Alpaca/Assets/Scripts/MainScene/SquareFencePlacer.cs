using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareFencePlacer : MonoBehaviour
{
    public GameObject fencePrefab;  // Fence prefab을 public 변수로 지정
    public int width = 10;          // 사각형의 너비
    public int height = 5;          // 사각형의 높이
    public float spacing = 1.0f;    // fence 간의 간격
    public Vector3 center = new Vector3(-50, 0, -50); // 중심 위치 설정

    void Start()
    {
        // 가로(Width)에 대한 fence 배치
        for (int i = 0; i <= width; i++)
        {
            PlaceFence(new Vector3(i * spacing - (width * spacing) / 2, 0, -(height * spacing) / 2), Quaternion.identity); // 아래쪽 가로
            PlaceFence(new Vector3(i * spacing - (width * spacing) / 2, 0, (height * spacing) / 2), Quaternion.identity); // 위쪽 가로
        }

        // 세로(Height)에 대한 fence 배치
        for (int j = 0; j <= height; j++)
        {
            PlaceFence(new Vector3(-(width * spacing) / 2, 0, j * spacing - (height * spacing) / 2), Quaternion.Euler(0, 90, 0)); // 왼쪽 세로
            PlaceFence(new Vector3((width * spacing) / 2, 0, j * spacing - (height * spacing) / 2), Quaternion.Euler(0, 90, 0)); // 오른쪽 세로
        }
    }

    void PlaceFence(Vector3 localPosition, Quaternion rotation)
    {
        GameObject fence = Instantiate(fencePrefab, center + localPosition, rotation);
        fence.transform.SetParent(transform); // 부모 오브젝트를 현재 GameObject로 설정
    }
}
