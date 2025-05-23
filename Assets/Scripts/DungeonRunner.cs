using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class DungeonRunner : MonoBehaviour
{
    private SyncAsyncFSM<DungeonRunner> _fsm;
    private Rigidbody2D _rigidbody;
    private DungeonTreasure _treasure;
    private int _treasureCount;
    private TextMeshProUGUI _text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<TextMeshProUGUI>(out var text))
            {
                _text = text;
                break;
            }
        }

        _fsm = new(this); // 初期化

        // 遷移を登録
        _fsm.AddTransition<IdleState, EntryState>(1);
        _fsm.AddTransition<EntryState, ExploreState>(2);
        _fsm.AddTransition<ExploreState, State>(3);
        _fsm.AddTransitionRange<State>(
            (4, typeof(ExploreState), () => _treasureCount < 3),
            (5, typeof(IdleState), () => true));
        _fsm.AddTransition<SyncAsyncFSM<DungeonRunner>.AnyState, MenuState>(6, () => _fsm.CurrentState.GetType() != typeof(MenuState));
        _fsm.AddTransition<MenuState, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

        _fsm.Start<IdleState>(); // 開始
    }

    // Update is called once per frame
    void Update()
    {
        _fsm.Update(); // 更新

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _fsm.Schedule(6); // メニューへの遷移を予約
        }

        _fsm.DispatchIfScheduled(); // 遷移を実行
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 探索中にお宝を発見したら
        if (_fsm.CurrentState.GetType() == typeof(ExploreState) && collision.collider.TryGetComponent<DungeonTreasure>(out var treasure))
        {
            _treasure = treasure;
            _treasureCount++;
            _fsm.Schedule(3); // 発見への遷移を予約
        }
    }

    void OnDestroy()
    {
        _fsm.Dispose(); // 破棄
    }

    // 待機
    private class IdleState : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        protected internal override void Enter()
        {
            Owner._text.text = "Press Any Button";
            Owner._treasureCount = 0;
        }

        protected internal override void Update()
        {
            if (Input.anyKeyDown)
            {
                FSM.Schedule(1); // エントリーへの遷移を予約
            }
        }
    }

    // エントリー
    private class EntryState : SyncAsyncFSM<DungeonRunner>.AsyncState
    {
        protected internal override async UniTask Start(CancellationToken token)
        {
            Owner._text.text = "3";
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            Owner._text.text = "2";
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            Owner._text.text = "1";
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            Owner._text.text = "Start!";
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            Owner._text.text = "";
            FSM.ScheduleAll(); // 登録済みのすべての遷移を予約
        }
    }

    // 探索
    private class ExploreState : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        private const float Speed = 3;

        protected internal override void Update()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            var velocity = new Vector2(x, y).normalized * Speed;
            Owner._rigidbody.linearVelocity = velocity;
        }
    }

    // 発見
    private class State : SyncAsyncFSM<DungeonRunner>.AsyncState
    {
        protected internal override async UniTask Start(CancellationToken token)
        {
            Owner._text.text = "Discover!";
            await Owner._treasure.Open(token);
            Owner._text.text = "";
            FSM.ScheduleAll(); // 登録済みのすべての遷移を予約
        }
    }

    // メニュー
    private class MenuState : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        protected internal override void Enter()
        {
            Owner._text.text = "Menu...";
        }

        protected internal override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FSM.Schedule(7); // 探索へ遷移
            }
        }

        protected internal override void Exit()
        {
            Owner._text.text = "";
        }
    }
}
