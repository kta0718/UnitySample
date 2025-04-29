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

        _fsm = new(this); // ‰Šú‰»

        // ‘JˆÚ‚ğ“o˜^
        _fsm.AddTransition<Idle, Entry>(1);
        _fsm.AddTransition<Entry, Explore>(2);
        _fsm.AddTransition<Explore, Discover>(3);
        _fsm.AddTransitionRange<Discover>(
            (4, typeof(Explore), () => _treasureCount < 3),
            (5, typeof(Idle), () => true));
        _fsm.AddTransition<SyncAsyncFSM<DungeonRunner>.AnyState, Menu>(6, () => _fsm.CurrentState.GetType() != typeof(Menu));
        _fsm.AddTransition<Menu, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

        _fsm.Start<Idle>(); // ŠJn
    }

    // Update is called once per frame
    void Update()
    {
        _fsm.Update(); // XV

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _fsm.Schedule(6); // ƒƒjƒ…[‚Ö‚Ì‘JˆÚ‚ğ—\–ñ
        }

        _fsm.Dispatch(); // ‘JˆÚ‚ğÀs
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ’Tõ’†‚É‚¨•ó‚ğ”­Œ©‚µ‚½‚ç
        if (_fsm.CurrentState.GetType() == typeof(Explore) && collision.collider.TryGetComponent<DungeonTreasure>(out var treasure))
        {
            _treasure = treasure;
            _treasureCount++;
            _fsm.Schedule(3); // ”­Œ©‚Ö‚Ì‘JˆÚ‚ğ—\–ñ
        }
    }

    void OnDestroy()
    {
        _fsm.Dispose(); // ”jŠü
    }

    // ‘Ò‹@
    private class Idle : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        public override void Enter()
        {
            Owner._text.text = "Press Any Button";
            Owner._treasureCount = 0;
        }

        public override void Update()
        {
            if (Input.anyKeyDown)
            {
                FSM.Schedule(1); // ƒGƒ“ƒgƒŠ[‚Ö‚Ì‘JˆÚ‚ğ—\–ñ
            }
        }
    }

    // ƒGƒ“ƒgƒŠ[
    private class Entry : SyncAsyncFSM<DungeonRunner>.AsyncState
    {
        public override async UniTask Start(CancellationToken token)
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
            FSM.ScheduleAll(); // “o˜^Ï‚İ‚Ì‚·‚×‚Ä‚Ì‘JˆÚ‚ğ—\–ñ
        }
    }

    // ’Tõ
    private class Explore : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        private const float Speed = 3;

        public override void Update()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            var velocity = new Vector2(x, y).normalized * Speed;
            Owner._rigidbody.linearVelocity = velocity;
        }
    }

    // ”­Œ©
    private class Discover : SyncAsyncFSM<DungeonRunner>.AsyncState
    {
        public override async UniTask Start(CancellationToken token)
        {
            Owner._text.text = "Discover!";
            await Owner._treasure.Open(token);
            Owner._text.text = "";
            FSM.ScheduleAll(); // “o˜^Ï‚İ‚Ì‚·‚×‚Ä‚Ì‘JˆÚ‚ğ—\–ñ
        }
    }

    // ƒƒjƒ…[
    private class Menu : SyncAsyncFSM<DungeonRunner>.SyncState
    {
        public override void Enter()
        {
            Owner._text.text = "Menu...";
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FSM.Schedule(7); // ’Tõ‚Ö‘JˆÚ
            }
        }

        public override void Exit()
        {
            Owner._text.text = "";
        }
    }
}
