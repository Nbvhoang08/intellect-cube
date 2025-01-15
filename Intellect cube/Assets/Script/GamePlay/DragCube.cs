using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DragCube : MonoBehaviour
{
    private Camera _mainCamera; // Camera chính
    private bool _isDragging = false; // Kiểm tra trạng thái kéo thả
    private Vector3 _offset; // Khoảng cách giữa chuột và object cha
    private Transform _parentObject; // Object cha của Cube
    private LayerMask _mapLayer; // Layer của Map
    private LayerMask _obstacleLayer; // Layer của Obstacle
    private Vector3 _originalPosition; // Vị trí ban đầu của object cha

    public CubeType cubeType; // Loại Cube định nghĩa hướng
    public GameObject cubePrefab; // Prefab của Cube để spawn
    [SerializeField]private bool _isDragDone;
    private void Start()
    {
        _mainCamera = Camera.main; // Gán camera chính
        _mapLayer = LayerMask.GetMask("Map"); // Lấy layer "Map"
        _obstacleLayer = LayerMask.GetMask("Obstacle"); // Lấy layer "Obstacle"
        _parentObject = transform.parent; // Lấy object cha
        _originalPosition = _parentObject.position; // Lưu vị trí ban đầu
        _isDragDone =false;
    }

    private void OnMouseDown()
    {
        if(_isDragDone) return;
        // Khi click chuột
        _isDragging = true;

        // Lấy khoảng cách giữa chuột và object cha
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        _offset = _parentObject.position - mouseWorldPos;
    }

    private void OnMouseDrag()
    {
        if(_isDragDone) return;
        if (_isDragging)
        {
            // Di chuyển object cha theo chuột (chỉ theo trục X và Y)
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 newPosition = mouseWorldPos + _offset;

            // Giữ nguyên trục Z, chỉ cập nhật trục X và Y
            newPosition.z = _parentObject.position.z;
            _parentObject.position = newPosition;
        }
    }

    private void OnMouseUp()
    {
         if(_isDragDone) return;
        // Khi thả chuột
        _isDragging = false;

        // Lấy vị trí chuột
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // Raycast kiểm tra nếu trúng layer Map, sử dụng RaycastAll để lấy tất cả va chạm
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            // Kiểm tra nếu raycast trúng layer Map và không trúng vào chính đối tượng này
            if (((1 << hit.collider.gameObject.layer) & _mapLayer) != 0 && hit.collider.gameObject != gameObject)
            {
                Vector3 newPosition = hit.point;

                // Giữ nguyên trục Z và làm tròn vị trí
                newPosition.z = _parentObject.position.z;
                newPosition = RoundToCustomHalf(newPosition);

                // Kiểm tra xem vị trí đã làm tròn có bị chặn bởi obstacle không
                if (IsObstacleAtPosition(newPosition))
                {
                    _parentObject.position = _originalPosition;
                    return;
                }

                // Đặt object tại vị trí mới
                _parentObject.position = newPosition;

                // Gọi hàm spawn các Cube theo hướng định nghĩa
                SpawnCubesByDirection(newPosition);
                _isDragDone = true;
                Subject.NotifyObservers("fill");
                return;
            }
        }

        // Nếu không hợp lệ, trả về vị trí ban đầu
        _parentObject.position = _originalPosition;
    }

    private void SpawnCubesByDirection(Vector3 startPosition)
    {
        // Lấy danh sách các hướng dựa trên CubeType
        List<Vector3> directions = GetDirectionsFromCubeType(cubeType);

        StartCoroutine(SpawnCubesWithDelay(startPosition, directions));
    }

    private IEnumerator SpawnCubesWithDelay(Vector3 startPosition, List<Vector3> directions)
    {
        foreach (Vector3 direction in directions)
        {
            Vector3 currentPosition = startPosition;

            while (true)
            {
                currentPosition += direction; // Di chuyển theo hướng
                currentPosition = RoundToCustomHalf(currentPosition); // Làm tròn vị trí

                // Kiểm tra nếu vị trí có obstacle hoặc ra khỏi map
                if (IsObstacleAtPosition(currentPosition))
                {
                    break;
                }

                // Spawn Cube tại vị trí
                GameObject cube = Instantiate(cubePrefab, currentPosition, Quaternion.identity);

                // Gọi animation spawn (VD: scale từ nhỏ đến lớn)
                PlaySpawnAnimation(cube);

                // Thông báo observer
                Subject.NotifyObservers("fill");

                // Chờ 0.1s trước khi spawn cube tiếp theo
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void PlaySpawnAnimation(GameObject cube)
    {
        // Đặt scale ban đầu của Cube là 0
        cube.transform.localScale = Vector3.zero;

        // Sử dụng DoTween để scale từ nhỏ đến lớn
        cube.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private List<Vector3> GetDirectionsFromCubeType(CubeType type)
    {
        List<Vector3> directions = new List<Vector3>();

        switch (type)
        {
            case CubeType.FourDirection:
                directions.Add(Vector3.up);
                directions.Add(Vector3.down);
                directions.Add(Vector3.left);
                directions.Add(Vector3.right);
                break;

            case CubeType.UpLeftRight:
                directions.Add(Vector3.up);
                directions.Add(Vector3.left);
                directions.Add(Vector3.right);
                break;

            case CubeType.Right:
                directions.Add(Vector3.right);
                break;

            case CubeType.RightDown:
                directions.Add(Vector3.right);
                directions.Add(Vector3.down);
                break;

            case CubeType.Left:
                directions.Add(Vector3.left);
                break;

            case CubeType.LeftDown:
                directions.Add(Vector3.left);
                directions.Add(Vector3.down);
                break;

            case CubeType.Up:
                directions.Add(Vector3.up);
                break;

            case CubeType.Down:
                directions.Add(Vector3.down);
                break;

            case CubeType.DownLeftRight:
                directions.Add(Vector3.down);
                directions.Add(Vector3.left);
                directions.Add(Vector3.right);
                break;
            case CubeType.UpRight:
                directions.Add(Vector3.up);
                directions.Add(Vector3.right);
                break;
            case CubeType.UpLeft:
                directions.Add(Vector3.up);
                directions.Add(Vector3.left);
                break;
            case CubeType.DownLeftUp:
                directions.Add(Vector3.down);
                directions.Add(Vector3.left);
                directions.Add(Vector3.up);
                break;
            case CubeType.DownRightUp:
                directions.Add(Vector3.down);
                directions.Add(Vector3.right);
                directions.Add(Vector3.up);
                break;
        }

        return directions;
    }

    private Vector3 RoundToCustomHalf(Vector3 position)
    {
        // Làm tròn giá trị X thành bội số **chẵn** của 0.5
        position.x = Mathf.Round(position.x * 2) / 2f;
        if (Mathf.RoundToInt(position.x * 2) % 2 != 0) // Nếu bội số lẻ, điều chỉnh thành chẵn
        {
            position.x += 0.5f * Mathf.Sign(position.x); // Dịch chuyển về phía gần chẵn nhất
        }

        // Làm tròn giá trị Y thành bội số **lẻ** của 0.5
        position.y = Mathf.Round(position.y * 2) / 2f;
        if (Mathf.RoundToInt(position.y * 2) % 2 == 0) // Nếu bội số chẵn, điều chỉnh thành lẻ
        {
            position.y += 0.5f;
        }

        // Trục Z không thay đổi
        return position;
    }

    private bool IsObstacleAtPosition(Vector3 position)
    {
        // Kiểm tra nếu có obstacle tại vị trí đã làm tròn
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f, _obstacleLayer);

        // Loại bỏ collider của chính object này
        foreach (Collider collider in colliders)
        {   
            if (collider != GetComponent<Collider>())
            {
                return true; // Trả về true nếu tìm thấy collider khác với collider của chính object
            }
        }

        return false; // Không có obstacle hoặc chỉ có chính object này
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Lấy vị trí chuột trong thế giới
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = _mainCamera.WorldToScreenPoint(_parentObject.position).z; // Đảm bảo độ sâu (z)
        return _mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}
