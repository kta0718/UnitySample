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

        _fsm = new(this); // ������

        // �J�ڂ�o�^
        _fsm.AddTransition<IdleState, EntryState>(1);
        _fsm.AddTransition<EntryState, ExploreState>(2);
        _fsm.AddTransition<ExploreState, State>(3);
        _fsm.AddTransitionRange<State>(
            (4, typeof(ExploreState), () => _treasureCount < 3),
            (5, typeof(IdleState), () => true));
        _fsm.AddTransition<SyncAsyncFSM<DungeonRunner>.AnyState, MenuState>(6, () => _fsm.CurrentState.GetType() != typeof(MenuState));
        _fsm.AddTransition<MenuState, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

        _fsm.Start<IdleState>(); // �J�n
    }

    // Update is called once per frame
    void Update()
    {
        _fsm.Update(); // �X�V

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _fsm.Schedule(6); // ���j���[�ւ̑J�ڂ�\��
        }

        _fsm.DispatchIfScheduled(); // �J�ڂ����s
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �T�����ɂ���𔭌�������
        if (_fsm.CurrentState.GetType() == typeof(ExploreState) && collision.collider.TryGetComponent<DungeonTreasure>(out var treasure))
        {
            _treasure = treasure;
            _treasureCount++;
            _fsm.Schedule(3); // �����ւ̑J�ڂ�\��
        }
    }

    void OnDestroy()
    {
        _fsm.Dispose(); // �j��
    }

    // �ҋ@
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
                FSM.Schedule(1); // �G���g���[�ւ̑J�ڂ�\��
            }
        }
    }

    // �G���g���[
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
            FSM.ScheduleAll(); // �o�^�ς݂̂��ׂĂ̑J�ڂ�\��
        }
    }

    // �T��
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

    // ����
    private class State : SyncAsyncFSM<DungeonRunner>.AsyncState
    {
        protected internal override async UniTask Start(CancellationToken token)
        {
            Owner._text.text = "Discover!";
            await Owner._treasure.Open(token);
            Owner._text.text = "";
            FSM.ScheduleAll(); // �o�^�ς݂̂��ׂĂ̑J�ڂ�\��
        }
    }

    // ���j���[
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
                FSM.Schedule(7); // �T���֑J��
            }
        }

        protected internal override void Exit()
        {
            Owner._text.text = "";
        }
    }
}
