using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IObserver 
{
      public int TargetCell;
    [SerializeField] private int fillCell;
    public bool full => fillCell == TargetCell;
    public bool gameover;

    [SerializeField] private GameObject particleEffectPrefab; // Prefab hiệu ứng ăn mừng

    void Awake()
    {
        Subject.RegisterObserver(this);
    }

    void OnDestroy()
    {
        Subject.UnregisterObserver(this);
    }

    public void OnNotify(string eventName, object eventData)
    {
        if (eventName == "fill")
        {
            fillCell++;
        }
    }

    void Start()
    {
        fillCell = 0;
    }

    void Update()
    {
        if (gameover) return;
        if (full)
        {
           
        // Mở giao diện chiến thắng
            StartCoroutine(WinAction());
            gameover = true;
        }
    }

    IEnumerator WinAction()
    {
        // Hiển thị hiệu ứng ăn mừng
        yield return new WaitForSecondsRealtime(0.5f); // Không bị ảnh hưởng bởi Time.timeScale
         SpawnCelebrationEffect();
        yield return new WaitForSecondsRealtime(1); // Không bị ảnh hưởng bởi Time.timeScale
     
        UIManager.Instance.OpenUI<Pass>();
        yield return new WaitForSecondsRealtime(0.5f); // Không bị ảnh hưởng bởi Time.timeScale
        Time.timeScale = 0;
    }

    private void SpawnCelebrationEffect()
    {
         int numParticles = 10; // Số lượng hiệu ứng
        float radius = 2f; // Bán kính của hình tròn

        Vector3 centerPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 10f));

        for (int i = 0; i < numParticles; i++)
        {
            // Tính toán vị trí theo hình tròn
            float angle = i * Mathf.PI * 2 / numParticles;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            // Tạo particle system tại vị trí tính toán
            Vector3 spawnPosition = centerPosition + offset;
            spawnPosition.z = -4;

            GameObject particle = Instantiate(particleEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
