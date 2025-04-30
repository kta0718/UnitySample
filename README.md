# UnitySample

UnitySample�ϡ�Unity������?��Ѧ���䫵��׫뫳?�ɪ��峪����׫������ȪǪ���

## ϰ�������ë�

- `SyncAsyncFSM` - ��Ѣ?ު��Ѣ������η�⪹�뫹��?�ȫޫ���Finite State Machine��

<!-- �����Ϋ����ëȪ�?�����Ϫ��Ϋ꫹�Ȫ����� -->

---

## SyncAsyncFSM

`SyncAsyncFSM<TOwner>`�ϡ�**��Ѣ?��**`SyncState`��**ު��Ѣ?��**`AsyncState`������ի�?���?����η��Ǫ������ī���?�ȫޫ���Ǫ�����?�૪�֫������Ȫ�?����칪䡢?����?����ު��Ѣ���٫��η����ꪷ�ƪ��ު���

### ��?

- `SyncState`����Ѣ����`AsyncState`��ު��Ѣ����٥����?ܬ
- ?��������칣���?��??�˪��ի��뫿��󫰣�
- ������칫���?��`AnyState`�����񫹫�?����?`PreviousState`�򫵫�?��
- ������`Schedule`��?�����`Dispatch`��???
- UniTask??`Cysharp.Threading.Tasks`

### �Ū�۰

1. **FSM����Ѣ��?����?����**

FSM����󪹪뫯�髹`MyOwner`������?����FSM`SyncAsyncFSM<MyOwner>`����몷�ު���

```csharp:MyOwner.cs
public class MyOwner : Monobehavior
{
    private SyncAsyncFSM<MyOwner> fsm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ��Ѣ��
        // ����Ϋ��󫹫��󫹪���?��Ԥ��
        fsm = new SyncAsyncFSM<MyOwner>(this);

        // 2.�����������?

        // ����
        // �Ϫ���Ϋ���?�Ȫ�����?��Ԥ��
        fsm.Start<IdleState>();
    }

    // Update is called once per frame
    void Update()
    {
        // ����
        fsm.Update();
    }

    // 3.�������髹��?�Ȫ�����
}
```

2. **�����?**

������?�ȪϪ��쪾����칫�?�֫���êƪ��ꡢ���٫��ID`int`,����`Type`,���?��`Func<bool>`����ʥ���몳�Ȫ���칪���?���ު���

��Ѣ�������Ѫ���������ު���

��?����������칪���໪����誦�˪ʪ�ު���

```csharp:MyOwner.cs
// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
{
    // 1.��Ѣ��

    // �����?
    fsm.AddTransition<IdleState, EntryState>(1);
    fsm.AddTransition<EntryState, ExploreState>(2);
    fsm.AddTransition<ExploreState, DiscoverState>(3);
    fsm.AddTransitionRange<DiscoverState>(
        (4, typeof(ExploreState), () => treasureCount < 3),
        (5, typeof(IdleState), () => true));
    fsm.AddTransition<SyncAsyncFSM<MyOwner>.AnyState, MenuState>(6, () => fsm.CurrentState.GetType() != typeof(MenuState));
    fsm.AddTransition<MenuState, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

    // 1.����
}
```

```csharp
void AddTransition<TFrom, TTo>(int eventId, Func<bool> gate);
```

��Ҫ��쪿���٫��ID����Ѧ�Ȫ��ơ�`TFrom`����`TTo`�ت���칪���?���ު�����칪˪�?�죨��?�ȣ�����?�Ǫ��ު���

```csharp
void AddTransition<TFrom, TTo>(int eventId);
```

�Ȫ���?����칣���?�Ȫʪ�������?�����������

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

���ꪫ���?��`TFrom`��?������?�����໪���������?�Ǫ��ު�������칪˪�?������ʦ���Ǫ���

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

?��ʪ�����?����칪�������?�����������

3. **����?�Ȫ�����**

��Ѣ����?�Ȫ���������˪�`SyncAsyncFSM<MyOwner>.SyncState`��?㯪�������驪�����ު�

```csharp:IdleState.cs
public class IdleState : SyncAsyncFSM<MyOwner>.SyncState
{
    // ���Ϋ���?�Ȫ�����������������Ъ��
    public override void Enter()
    {
        
    }

    public override void Update()
    {
        // ?�����ë�
    }

    // ��Ϋ���?�Ȫ����������������Ъ��
    public override void Exit()
    {
        
    }
}
```

ު��Ѣ����?�Ȫ���������˪�`SyncAsyncFSM<MyOwner>.AsyncState`��?㯪�������驪�����ު�

```csharp:EntryState.cs
public class EntryState : SyncAsyncFSM<MyOwner>.AsyncState
{
    public override async UniTask Start(Cancellation token)
    {
        // ?�����ë�
    }

    // ���Ϋ���?�Ȫ��������������Ϋ���?�Ȫ���칪���Ȫ��䡢����FSM����󪹪뫯�髹����ѥ���쪿�Ȫ������Ъ��
    public override void OnCancel()
    {

    }
}
```

```scharp
MyOwner Owner;
```

����?�ȫ��髹?�Ǫ�`Owner`�Ϋ��󫹫��󫹪�����Ǫ��ު���

4. **��칪����??��**

```csharp
void Schedule(int eventId);
```

��Ϋ���?�Ȫ���칫�?�֫����Ҫ��쪿���٫��ID����?����ƪ���Ъ�����칪���峪���ު���

```csharp
void ScheduleAll();
```

��Ϋ���?�Ȫ���?����ƪ�����칪򪹪٪���峪��ު���

```csharp
void Dispatch();
```

��峪���ƪ�����칪��骫�������������?���쪿���������?���?��������칪�?������ު���
?������ʪ��ê����?�ߪ���칪���ѥ����ު���

```csharp
void Dispatch(int eventId);
```

���?�ߪ���칪���ѥ������Ҫ��쪿���٫��ID����칪����?���?������?�����ު���

