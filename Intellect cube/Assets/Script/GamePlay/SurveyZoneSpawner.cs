using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveyZoneSpawner : MonoBehaviour
{
    public GameObject surveyZonePrefab; // Survey Zone prefab
    public LayerMask obstacleLayer; // Layer của Obstacle
    public float distanceBetweenSurveyZones = 1f; // Khoảng cách giữa các survey zones

    private List<GameObject> surveyZones = new List<GameObject>(); // Danh sách các survey zones
    private ObjectPool surveyZonePool; // Object pool để tái sử dụng các survey zones

    public CubeType cubeType;

    private void Start()
    {
        surveyZonePool = new ObjectPool(surveyZonePrefab, 10); // Khởi tạo Object Pool với 10 survey zones ban đầu
    }

    public void SpawnSurveyZones()
    {
        // Lấy các hướng spawn từ CubeType
        List<Vector3> directions = GetSpawnDirections(cubeType);

        foreach (var direction in directions)
        {
            // Raycast và spawn survey zone theo từng hướng
            Vector3 currentPosition = transform.position; // Vị trí bắt đầu (thường là vị trí của Cube)

            for (int i = 1; i <= 10; i++) // Số lượng survey zone tối đa (có thể tùy chỉnh)
            {
                // Tính toán vị trí tiếp theo theo hướng spawn và khoảng cách giữa các survey zones
                Vector3 newPosition = currentPosition + direction * distanceBetweenSurveyZones * i;

                // Kiểm tra raycast tại vị trí đó
                RaycastHit hit;
                if (Physics.Raycast(currentPosition, direction, out hit, distanceBetweenSurveyZones * i, obstacleLayer))
                {
                    // Nếu raycast gặp vật cản (obstacle), dừng lại
                    break;
                }

                // Kiểm tra vị trí hợp lệ và spawn survey zone từ Object Pool
                GameObject surveyZone = surveyZonePool.GetObject();
                surveyZone.transform.position = newPosition;
                surveyZones.Add(surveyZone);
            }
        }
    }

    private List<Vector3> GetSpawnDirections(CubeType cubeType)
    {
        List<Vector3> directions = new List<Vector3>();

        switch (cubeType)
        {
            case CubeType.FourDirection:
                // Raycast theo 4 hướng: lên, xuống, trái, phải
                directions.Add(Vector3.up);     // Lên
                directions.Add(Vector3.down);   // Xuống
                directions.Add(Vector3.left);   // Trái
                directions.Add(Vector3.right);  // Phải
                break;

            case CubeType.UpLeftRight:
                // Raycast theo các hướng: lên, trái, phải
                directions.Add(Vector3.up);     // Lên
                directions.Add(Vector3.left);   // Trái
                directions.Add(Vector3.right);  // Phải
                break;

            case CubeType.Right:
                // Raycast theo hướng phải
                directions.Add(Vector3.right);
                break;

            case CubeType.RightDown:
                // Raycast theo hướng phải và xuống
                directions.Add(Vector3.right);  // Phải
                directions.Add(Vector3.down);   // Xuống
                break;

            case CubeType.Left:
                // Raycast theo hướng trái
                directions.Add(Vector3.left);
                break;

            case CubeType.LeftDown:
                // Raycast theo hướng trái và xuống
                directions.Add(Vector3.left);   // Trái
                directions.Add(Vector3.down);   // Xuống
                break;

            case CubeType.Up:
                // Raycast theo hướng lên
                directions.Add(Vector3.up);
                break;

            case CubeType.Down:
                // Raycast theo hướng xuống
                directions.Add(Vector3.down);
                break;

            case CubeType.DownLeftRight:
                // Raycast theo hướng xuống, trái, phải
                directions.Add(Vector3.down);   // Xuống
                directions.Add(Vector3.left);   // Trái
                directions.Add(Vector3.right);  // Phải
                break;

            default:
                directions.Add(Vector3.zero);  // Mặc định
                break;
        }

        return directions;
    }

    public void DeactivateAllSurveyZones()
    {
        // Tắt tất cả các survey zone khi không cần thiết
        foreach (var surveyZone in surveyZones)
        {
            surveyZonePool.ReturnObject(surveyZone);
        }
        surveyZones.Clear();
    }

    // Ví dụ Object Pool (có thể tùy chỉnh thêm)
    public class ObjectPool
    {
        private GameObject prefab;
        private Queue<GameObject> pool;

        public ObjectPool(GameObject prefab, int initialSize)
        {
            this.prefab = prefab;
            pool = new Queue<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Object.Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public GameObject GetObject()
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Nếu không còn đối tượng trong pool, tạo mới
                GameObject obj = Object.Instantiate(prefab);
                return obj;
            }
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
