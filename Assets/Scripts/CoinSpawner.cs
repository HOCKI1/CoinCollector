using System.Collections.Generic;
using AxGrid.Base;
using UnityEngine;

public struct Coin
{
    public Vector3 position;
}

public class CoinSpawner : MonoBehaviourExt
{
    [SerializeField] public int gridSize = 10; // Размер сетки
    [SerializeField] public float gridCellSize = 1f; // Размер клетки
    public AudioClip collectSound; // Звук сбора
    public AudioSource audioSource; // Источник звука
    private int currentCoinsAmount = 0; // Количество монет на сцене
    private int maxCoinsAmount = 1000; // Максимальное количество монет
    public Mesh coinMesh; // Меш монеты
    public Material coinMaterial; // Материал монеты
    private List<Coin> coins = new(); // Список монет в сцене
    private List<Matrix4x4> matrices = new(); // Список матриц для отрисовки

    public CoinManager coinManager;

    [OnAwake]
    public void SpawnCoins()
    {
        SpawnInitialCoins();
    }

    // Создание стартовых монет
    private void SpawnInitialCoins()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var coin = new Coin
                {
                    position = new Vector3(
                        x * gridCellSize + Random.Range(-0.5f, 0.5f) * gridCellSize,
                        0.5f,
                        y * gridCellSize + Random.Range(-0.5f, 0.5f) * gridCellSize
                    )
                };
                coins.Add(coin);
                currentCoinsAmount++;
            }
        }
    }

    // Отрисовка монет
    [OnUpdate]
    public void RenderCoins()
    {
        matrices.Clear();

        foreach (var coin in coins)
        {
            matrices.Add(Matrix4x4.TRS(
                coin.position,
                Quaternion.identity,
                new Vector3(50f, 50f, 50f)
            ));
        }

        // Оптимизация отрисовки монет
        for (int i = 0; i < matrices.Count; i += 1023)
        {
            Graphics.DrawMeshInstanced(
                coinMesh,
                0,
                coinMaterial,
                matrices.GetRange(i, Mathf.Min(1023, matrices.Count - i))
            );
        }
    }

    // Создание новой монеты
    [OnRefresh(0.25f)]
    public void SpawnNewCoin()
    {
        if (currentCoinsAmount >= maxCoinsAmount)
            return;

        int x = Random.Range(0, gridSize);
        int y = Random.Range(0, gridSize);

        coins.Add(new Coin
        {
            position = new Vector3(
                x * gridCellSize + Random.Range(-0.5f, 0.5f) * gridCellSize,
                0.5f,
                y * gridCellSize + Random.Range(-0.5f, 0.5f) * gridCellSize
            )
        });

        currentCoinsAmount++;
    }

    // Сбор монет
    public void CollectCoin(Vector3 playerPosition, float radius = 0.5f)
    {
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(playerPosition, coins[i].position) < radius)
            {
                // Воспроизведение звука
                if (audioSource != null && collectSound != null)
                    audioSource.PlayOneShot(collectSound);

                coins.RemoveAt(i);
                currentCoinsAmount--;
                coinManager.AddCoin();
            }
        }
    }
}
