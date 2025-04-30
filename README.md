# UnitySample

UnitySampleªÏ¡¢Unityú¾ª±ªÎ?íåÑ¦Òöªä«µ«ó«×«ë«³?«Éªòó¢å³ª·ª¿«×«í«¸«§«¯«ÈªÇª¹¡£

## Ï°à÷«¢«»«Ã«È

- `SyncAsyncFSM` - ÔÒÑ¢?ŞªÔÒÑ¢ªò÷ÖùêÎ·×âª¹ªë«¹«Æ?«È«Ş«·«ó£¨Finite State Machine£©

<!-- ĞÑı­ªÎ«¢«»«Ã«Èª¬?ª¨ª¿ğ·ªÏª³ªÎ«ê«¹«ÈªËõÚÑÀ -->

---

## SyncAsyncFSM

`SyncAsyncFSM<TOwner>`ªÏ¡¢**ÔÒÑ¢?×â**`SyncState`ªÈ**ŞªÔÒÑ¢?×â**`AsyncState`ªòÔÒìé«Õ«ì?«à«ï?«¯ªÇÎ·×âªÇª­ªëÛñéÄ«¹«Æ?«È«Ş«·«óªÇª¹¡£«²?«à«ª«Ö«¸«§«¯«ÈªÎ?÷¾ôÃì¹ªä¡¢?÷¾ªË?ª¸ª¿ŞªÔÒÑ¢«¤«Ù«ó«ÈÎ·×âªËîêª·ªÆª¤ªŞª¹¡£

### ÷å?

- `SyncState`£¨ÔÒÑ¢£©ªÈ`AsyncState`£¨ŞªÔÒÑ¢£©ªÎÙ¥ü¬ªÊ?Ü¬
- ?ËìÜõª­ôÃì¹£¨«²?«È??ªËªèªë«Õ«£«ë«¿«ê«ó«°£©
- ìòëòôÃì¹«¹«Æ?«È`AnyState`ªäòÁîñ«¹«Æ?«ÈÜÖ?`PreviousState`ªò«µ«İ?«È
- ôÃì¹åøå³`Schedule`ªÈ?ãÁôÃì¹`Dispatch`ªÎ???
- UniTask??`Cysharp.Threading.Tasks`

### ŞÅª¤Û°

1. **FSMªÎôøÑ¢ûù?ÑÃÔÑ?ÌÚãæ**

FSMªòá¶êóª¹ªë«¯«é«¹`MyOwner`ªòúşìÚ?ªËò¥ªÄFSM`SyncAsyncFSM<MyOwner>`ªòà¾åëª·ªŞª¹¡£

```csharp:MyOwner.cs
public class MyOwner : Monobehavior
{
    private SyncAsyncFSM<MyOwner> fsm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ôøÑ¢ûù
        // á¶êóíºªÎ«¤«ó«¹«¿«ó«¹ªòìÚ?ªËÔ¤ª¹
        fsm = new SyncAsyncFSM<MyOwner>(this);

        // 2.ª³ª³ªÇôÃì¹Ôô?

        // ÑÃÔÑ
        // ªÏª¸ªáªÎ«¹«Æ?«ÈªòúşìÚ?ªËÔ¤ª¹
        fsm.Start<IdleState>();
    }

    // Update is called once per frame
    void Update()
    {
        // ÌÚãæ
        fsm.Update();
    }

    // 3.ª³ª³ª«ªé«¹«Æ?«ÈªòïÒëù
}
```

2. **ôÃì¹Ôô?**

ÊÀ«¹«Æ?«ÈªÏª½ªìª¾ªìôÃì¹«Æ?«Ö«ëªòò¥ªÃªÆªªªê¡¢«¤«Ù«ó«ÈID`int`,ôÃì¹à»`Type`,ôÃì¹?Ëì`Func<bool>`ªòõÚÊ¥ª¹ªëª³ªÈªÇôÃì¹ªòÔô?ª·ªŞª¹¡£

ôøÑ¢ûùªÈÑÃÔÑªÎÊàªËú¼ª¤ªŞª¹¡£

Ôô?ª·ª¿â÷ªËôÃì¹ª¬éĞà»ªµªìªëªèª¦ªËªÊªêªŞª¹¡£

```csharp:MyOwner.cs
// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
{
    // 1.ôøÑ¢ûù

    // ôÃì¹Ôô?
    fsm.AddTransition<IdleState, EntryState>(1);
    fsm.AddTransition<EntryState, ExploreState>(2);
    fsm.AddTransition<ExploreState, DiscoverState>(3);
    fsm.AddTransitionRange<DiscoverState>(
        (4, typeof(ExploreState), () => treasureCount < 3),
        (5, typeof(IdleState), () => true));
    fsm.AddTransition<SyncAsyncFSM<MyOwner>.AnyState, MenuState>(6, () => fsm.CurrentState.GetType() != typeof(MenuState));
    fsm.AddTransition<MenuState, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

    // 1.ÑÃÔÑ
}
```

```csharp
void AddTransition<TFrom, TTo>(int eventId, Func<bool> gate);
```

ò¦ïÒªµªìª¿«¤«Ù«ó«ÈIDªòÌøÑ¦ªÈª·ªÆ¡¢`TFrom`ª«ªé`TTo`ªØªÎôÃì¹ªòÔô?ª·ªŞª¹¡£ôÃì¹ªËªÏ?Ëì£¨«²?«È£©ªòÜõ?ªÇª­ªŞª¹¡£

```csharp
void AddTransition<TFrom, TTo>(int eventId);
```

ßÈªËêó?ªÊôÃì¹£¨«²?«ÈªÊª·£©ªòÔô?ª¹ªëÊÛæ¶÷ú¡£

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

ôÃì¹êª«¹«Æ?«È`TFrom`ªË?ª·¡¢ÜÜ?ªÎôÃì¹à»ªòìéÎÀªÇÔô?ªÇª­ªŞª¹¡£ÊÀôÃì¹ªËªÏ?Ëìªòò¦ïÒÊ¦ÒöªÇª¹¡£

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

?ËìªÊª·ªÇÜÜ?ªÎôÃì¹ªòìéÎÀÔô?ª¹ªëÊÛæ¶÷ú¡£

3. **«¹«Æ?«ÈªòïÒëù**

ÔÒÑ¢«¹«Æ?«ÈªòïÒëùª¹ªëªËªÏ`SyncAsyncFSM<MyOwner>.SyncState`ªò?ã¯ªµª»ªëù±é©ª¬ª¢ªêªŞª¹

```csharp:IdleState.cs
public class IdleState : SyncAsyncFSM<MyOwner>.SyncState
{
    // ª³ªÎ«¹«Æ?«ÈªËôÃì¹òÁı­ªËìéÓøû¼ªĞªìªë
    public override void Enter()
    {
        
    }

    public override void Update()
    {
        // ?÷¾«í«¸«Ã«¯
    }

    // öâªÎ«¹«Æ?«ÈªØôÃì¹òÁîñªËìéÓøû¼ªĞªìªë
    public override void Exit()
    {
        
    }
}
```

ŞªÔÒÑ¢«¹«Æ?«ÈªòïÒëùª¹ªëªËªÏ`SyncAsyncFSM<MyOwner>.AsyncState`ªò?ã¯ªµª»ªëù±é©ª¬ª¢ªêªŞª¹

```csharp:EntryState.cs
public class EntryState : SyncAsyncFSM<MyOwner>.AsyncState
{
    public override async UniTask Start(Cancellation token)
    {
        // ?÷¾«í«¸«Ã«¯
    }

    // ª³ªÎ«¹«Æ?«Èª¬ğûÖõª¹ªëîñªËöâªÎ«¹«Æ?«ÈªØôÃì¹ª¹ªëªÈª­ªä¡¢ª³ªÎFSMªòá¶êóª¹ªë«¯«é«¹ª¬÷òÑ¥ªµªìª¿ªÈª­ªËû¼ªĞªìªë
    public override void OnCancel()
    {

    }
}
```

```scharp
MyOwner Owner;
```

«¹«Æ?«È«¯«é«¹?ªÇªâ`Owner`ªÎ«¤«ó«¹«¿«ó«¹ªòö¢ÔğªÇª­ªŞª¹¡£

4. **ôÃì¹ªÎåøå³??ú¼**

```csharp
void Schedule(int eventId);
```

úŞî¤ªÎ«¹«Æ?«ÈªÎôÃì¹«Æ?«Ö«ëªËò¦ïÒªµªìª¿«¤«Ù«ó«ÈIDª¬Ôô?ªµªìªÆª¤ªìªĞª½ªÎôÃì¹ª¬åøå³ªµªìªŞª¹¡£

```csharp
void ScheduleAll();
```

úŞî¤ªÎ«¹«Æ?«ÈªËÔô?ªµªìªÆª¤ªëôÃì¹ªòª¹ªÙªÆåøå³ª·ªŞª¹¡£

```csharp
void Dispatch();
```

åøå³ªµªìªÆª¤ªëôÃì¹ªÎñéª«ªééĞà»Óøâ÷£¨Ôô?ªµªìª¿â÷£©ªÇôÃì¹?Ëìªò?ª¿ª»ªĞôÃì¹ª¬?ú¼ªµªìªŞª¹¡£
?ú¼ªµªìªÊª«ªÃª¿åøå³?ªßªÎôÃì¹ªÏ÷òÑ¥ªµªìªŞª¹¡£

```csharp
void Dispatch(int eventId);
```

åøå³?ªßªÎôÃì¹ªò÷òÑ¥ª·ªÆò¦ïÒªµªìª¿«¤«Ù«ó«ÈIDªÎôÃì¹ªòôÃì¹?Ëìªò?ª¿ª»ªĞ?ú¼ª·ªŞª¹¡£

