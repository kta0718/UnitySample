using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

public class SyncAsyncFSM<TOwner> : IDisposable
{
    // 同期処理ステート
    public abstract class SyncState
    {
        public TOwner Owner { get; init; }
        public SyncAsyncFSM<TOwner> FSM { get; init; }
        public Dictionary<int, (SyncState state, int order, Func<bool> gate)> TransitionTable { get; } = new(); // 遷移テーブル
        public SortedDictionary<int, int> TransitionQueue { get; } = new(); // 遷移キュー

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void OnDispose() { }
    }

    // 非同期処理ステート
    public abstract class AsyncState : SyncState
    {
        private CancellationTokenSource _cts = new();
        private enum Status { Running, Completed, Interrupted }
        private Status _status = Status.Completed;

        public virtual async UniTask Start(CancellationToken token) { await UniTask.CompletedTask; }
        public virtual void OnCancel() { }

        public sealed override async void Enter()
        {
            if (_status != Status.Completed) return;
            _status = Status.Running;
            try
            {
                await Start(_cts.Token);
                _cts.Token.ThrowIfCancellationRequested();
                _status = Status.Completed;
            }
            catch (OperationCanceledException) // Running中にExit()が呼ばれた
            {
                OnCancel();
                _status = Status.Completed;
                _cts.Dispose();
                _cts = new();
            }
        }

        public sealed override void Exit()
        {
            if (_status == Status.Running)
            {
                _status = Status.Interrupted;
                _cts.Cancel();
            }
        }

        public sealed override void OnDispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    // 任意遷移ステートの遷移元ステート
    public sealed class AnyState : SyncState { }

    // 一つ前のステート
    public sealed class PreviousState : SyncState
    {
        private SyncState _previousValue;
        private SyncState _currentValue;

        public SyncState Value
        {
            get { return _currentValue; }
            set { (_previousValue, _currentValue) = (_currentValue, value); }
        }

        public override void Enter()
        {
            FSM.CurrentState = _previousValue;
            FSM.CurrentState.Enter();
        }
    }

    // Start()で呼び出す
    public void Start<T>() where T : SyncState, new()
    {
        CurrentState = GetOrAddState<T>();
        CurrentState?.Enter();
    }

    // Update()で呼び出す
    public void Update()
    {
        CurrentState?.Update();
    }

    // OnDestroy()で呼び出す
    public void Dispose()
    {
        CurrentState?.OnDispose();
    }

    // 遷移を発行
    public void Dispatch()
    {
        var any = GetOrAddState<AnyState>();
        foreach (var value in any.TransitionQueue.Values)
        {
            if (any.TransitionTable[value].gate())
            {
                any.TransitionQueue.Clear();
                CurrentState.TransitionQueue.Clear();
                ChangeState(any.TransitionTable[value].state);
                return;
            }
        }

        foreach (var value in CurrentState.TransitionQueue.Values)
        {
            if (CurrentState.TransitionTable[value].gate())
            {
                any.TransitionQueue.Clear();
                CurrentState.TransitionQueue.Clear();
                ChangeState(CurrentState.TransitionTable[value].state);
                return;
            }
        }

        any.TransitionQueue.Clear();
        CurrentState.TransitionQueue.Clear();
        return;
    }

    // 遷移を発行
    public void Dispatch(int eventId)
    {
        var any = GetOrAddState<AnyState>();
        any.TransitionQueue.Clear();
        CurrentState.TransitionQueue.Clear();
        if (any.TransitionTable.TryGetValue(eventId, out var to) || CurrentState.TransitionTable.TryGetValue(eventId,out to))
        {
            ChangeState(to.state);
        }
    }

    // 遷移を予約
    public void Schedule(int eventId)
    {
        if (CurrentState.TransitionTable.TryGetValue(eventId, out var trans))
        {
            CurrentState.TransitionQueue.Add(trans.order, eventId);
        }
        var any = GetOrAddState<AnyState>();
        if (any.TransitionTable.TryGetValue(eventId, out trans))
        {
            any.TransitionQueue.Add(trans.order, eventId);
        }
    }

    public void ScheduleAll()
    {
        foreach (var (eventId, trans) in CurrentState.TransitionTable)
        {
            CurrentState.TransitionQueue.Add(trans.order, eventId);
        }
    }

    // 遷移元ステートと遷移先ステートを登録
    public void AddTransition<TFrom, TTo>(int eventId, Func<bool> gate) where TFrom : SyncState, new() where TTo : SyncState, new()
    {
        var from = GetOrAddState<TFrom>();
        if (from.TransitionTable.ContainsKey(eventId))
        {
            throw new ArgumentException($"ステート'{from}'に対してイベントID '{eventId}'の遷移は登録済みです");
        }
        var to = GetOrAddState<TTo>();
        from.TransitionTable.Add(eventId, (to, from.TransitionTable.Count, gate));
    }

    // 遷移元ステートと遷移先ステートを登録
    public void AddTransition<TFrom, TTo>(int eventId) where TFrom : SyncState, new() where TTo : SyncState, new()
    {
        AddTransition<TFrom, TTo>(eventId, () => true);
    }

    // 遷移元ステートと複数の遷移先ステートを登録
    public void AddTransitionRange<TFrom>(params (int, Type, Func<bool>)[] to) where TFrom : SyncState, new()
    {
        foreach (var (eventId, type, gate) in to)
        {
            if (!typeof(SyncState).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"型'{type}'は'{nameof(SyncState)}'を継承していなければいけません");
            }
            typeof(SyncAsyncFSM<TOwner>).GetMethod(nameof(AddTransition), 2, new[] { typeof(int), typeof(Func<bool>) }).MakeGenericMethod(typeof(TFrom), type).Invoke(this, new object[] { eventId, gate });
        }
    }

    // 遷移元ステートと複数の遷移先ステートを登録
    public void AddTransitionRange<TFrom>(params (int, Type)[] to) where TFrom : SyncState, new()
    {
        foreach (var (eventId, type) in to)
        {
            if (!typeof(SyncState).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"型'{type}'は'{nameof(SyncState)}'を継承していなければいけません");
            }
            typeof(SyncAsyncFSM<TOwner>).GetMethod(nameof(AddTransition), 2, new[] { typeof(int) }).MakeGenericMethod(typeof(TFrom), type).Invoke(this, new object[] { eventId });
        }
    }

    public SyncAsyncFSM(TOwner owner) { _owner = owner; }
    public SyncState CurrentState { get; private set; } // 現在のステート

    private readonly TOwner _owner;
    private readonly LinkedList<SyncState> _states = new(); // 各ステートのインスタンス
    private readonly Queue<int> _events = new(); // 遷移イベント

    private void ChangeState(SyncState to)
    {
        CurrentState?.Exit();
        (GetOrAddState<PreviousState>().Value, CurrentState) = (CurrentState,  to);
        CurrentState?.Enter();
    }

    private T AddState<T>() where T : SyncState, new()
    {
        var state = new T { Owner = _owner, FSM = this };
        _states.AddLast(state);
        return state;
    }

    private T GetOrAddState<T>() where T : SyncState, new()
    {
        foreach (var state in _states)
        {
            if (state.GetType() == typeof(T))
            {
                return (T)state;
            }
        }
        return AddState<T>();
    }
}
