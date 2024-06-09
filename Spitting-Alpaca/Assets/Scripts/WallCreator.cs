using Unity.VisualScripting;
using UnityEngine;

public class WallCreator : MonoBehaviour
{
    public GameObject objectToProtect; // 벽을 만들 오브젝트
    public GameObject wallPrefab; // 벽 프리팹
    public Color wallColor = Color.red; // 벽의 색상
    public float distance = 3f;
    public float height = 100f;
    public float speed = 5.0f; // 이동 속도
    public float range = 2.0f;

    private GameObject[] walls = new GameObject[4];
    private void Start()
    {
        // 오브젝트의 위치를 가져옴
        objectToProtect = GameObject.FindGameObjectWithTag("Respawn");
        Vector3 objectPosition = objectToProtect.transform.position;
        // 4개의 벽 생성 및 이동
       
        walls[0] = CreateWallAndMove(objectPosition, new Vector3(0, 0,-distance),0);  // +z 방향
        walls[1] = CreateWallAndMove(objectPosition, new Vector3(0, 0,distance),0);     // -z 방향
        walls[2] = CreateWallAndMove(objectPosition, new Vector3(distance, 0,0),90);    // +x 방향
        walls[3] = CreateWallAndMove(objectPosition, new Vector3(-distance, 0,0),90);     // -x 방향
    }

    GameObject CreateWallAndMove(Vector3 position, Vector3 direction, float ratate)
    {
        // 벽 생성
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);

        // 벽 이동
        wall.transform.position += direction;
        //회전
        wall.transform.rotation = Quaternion.Euler(0f, ratate, 0f);

        wall.transform.localScale = new Vector3(distance * 2,100,1); // 가로세로, 높이, 넓이

        return wall;
    }


    void Update(){
        foreach(GameObject wall in walls){
            if(wall == null) continue; // 벽이 null인 경우 다음 벽으로 넘어갑니다.

            Vector3 wallPosition = wall.transform.position;
            float distance = Vector3.Distance(wallPosition, objectToProtect.transform.position);
    
            if(distance <= range) return ;
                wall.transform.position = Vector3.MoveTowards(wallPosition, objectToProtect.transform.position, speed * Time.deltaTime);
            }
    }
}
