using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using UnityEngine;
using AxGrid.Path;

public class CharacterFSM : MonoBehaviourExt
{

    public GameObject player;
    public CoinSpawner coinSpawner;
    public Animator animator;
    private bool isMoving = false;


    [OnAwake]
    private void CreateFsm()
    {
        Settings.Fsm = new FSM();
        Settings.Fsm.Add(new IdleState() { idle_animator = animator }); // Idle состояние
        Settings.Fsm.Add(new MoveState() { player = player, coinSpawner = coinSpawner, animator = animator }); // Move состояние
        Debug.Log("FSM создан");
    }

    [OnStart]
    private void StartFsm()
    {
        Settings.Fsm.Start("IdleState"); // Запуск Idle состояния
        Debug.Log("FSM запущен");
    }

    [OnUpdate]
    public void UpdateFsm()
    {
        Settings.Fsm.Update(Time.deltaTime);
    }

    public void StartMovement()
    {
        if (isMoving) return;
        isMoving = true;
        Settings.Fsm?.Change("MoveState");
    }

    public void StopMovement()
    {
        if (!isMoving) return;
        isMoving = false;
        Settings.Fsm?.Change("IdleState");
    }
}

// Idle состояние
[State("IdleState")]
public class IdleState : FSMState
{
    public Animator idle_animator;

    [One(0f)]
    public void Enter()
    {
        Debug.Log("Переход в Idle state");

        // Переход в Idle анимацию
        if (idle_animator != null)
            idle_animator.SetFloat("Speed", 0f);
    }
}


// Move состояние
[State("MoveState")]
public class MoveState : FSMState
{

    public Animator animator;
    public GameObject player;
    public CoinSpawner coinSpawner;

    private CPath path;
    private Vector3 target;
    private Vector3 previousPosition;

    [One(0f)]
    public void Enter()
    {
        Debug.Log("Переход в Move state");
        previousPosition = player.transform.position;
        StartNewPath();
    }

    [Loop(0f)]
    public void Tick(float deltaTime)
    {
        path?.Update(deltaTime);

        // Вычисление скорости движения
        float speed = (player.transform.position - previousPosition).magnitude / deltaTime;
        previousPosition = player.transform.position;

        // Передача скорости в аниматор
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }


    private void StartNewPath()
    {
        // Выбор случайной точки в пределах карты
        target = new Vector3(
            Random.Range(0.3f, (coinSpawner.gridSize * coinSpawner.gridCellSize)-0.3f),
            0.5f,
            Random.Range(0.3f, (coinSpawner.gridSize * coinSpawner.gridCellSize)-0.3f)
        );

        Vector3 start = player.transform.position;
        float distance = Vector3.Distance(start, target); // Расстояния до точки в метрах
        float duration = Mathf.Max(0.1f, distance); // Длительность движения

        path = new CPath()
            .EasingLinear(duration / 2, 0f, 1f, f =>
            {
                // Движение игрока
                Vector3 newPos = Vector3.Lerp(start, target, f);
                player.transform.position = newPos;

                // Поворот в сторону движения
                Vector3 direction = (target - newPos).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, lookRotation, 10f * Time.deltaTime);
                }

                // Сбор монет
                coinSpawner.CollectCoin(newPos, 0.5f);
            })
            .Action(() =>
            {
                Debug.Log($"Достигнута точка: {target}");
                StartNewPath(); // Запуск следующего пути
            });
    }
}

