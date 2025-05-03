# UnitySample

UnitySampleは、Unity向けの拡張機能やサンプルコードを集約したプロジェクトです。

## 構成アセット

- `SyncAsyncFSM` - 同期・非同期を統合管理するステートマシン（Finite State Machine）

<!-- 今後のアセットが増えた際はこのリストに追記 -->

---

## SyncAsyncFSM

`SyncAsyncFSM`は、**同期処理**`SyncState`と**非同期処理**`AsyncState`を同一フレームワークで管理できる汎用ステートマシンです。ゲームオブジェクトの状態遷移や、状態に応じた非同期イベント管理に適しています。

### 特徴

- `SyncState`（同期）と`AsyncState`（非同期）の明確な区別
- 条件付き遷移（ゲート関数によるフィルタリング）
- 任意遷移ステート`AnyState`や直前ステート復帰`PreviousState`をサポート
- 遷移予約`Schedule`と即時遷移`Dispatch`の両対応
- UniTaskに対応`Cysharp.Threading.Tasks`

### 使い方

1. **FSMの初期化・起動・更新**

FSMを所有するクラス`MyOwner`を型引数に持つFSM`SyncAsyncFSM<MyOwner>`を宣言します。

```csharp:MyOwner.cs
public class MyOwner : Monobehavior
{
    private SyncAsyncFSM<MyOwner> fsm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初期化
        // 所有者のインスタンスを引数に渡す
        fsm = new SyncAsyncFSM<MyOwner>(this);

        // 2.ここで遷移登録

        // 起動
        // はじめのステートを型引数に渡す
        fsm.Start<IdleState>();
    }

    // Update is called once per frame
    void Update()
    {
        // 更新
        fsm.Update();
    }

    // 3.ここからステートを定義
}
```

2. **遷移登録**

各ステートはそれぞれ遷移テーブルを持っており、イベントID`int`・遷移先`Type`・遷移条件`Func<bool>`を追加することで遷移を登録します。

-初期化と起動の間に行います。
-登録した順に遷移が優先されます。
-任意遷移を登録するには`TFrom`に`SyncAsyncState<MyOwner>.AnyState`を指定します。
-直前復帰を登録するには`TTo`に`SyncAsyncState<MyOwner>.PreviousState`を指定します。

```csharp:MyOwner.cs
// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
{
    // 1.初期化

    // 遷移登録
    fsm.AddTransition<IdleState, EntryState>(1);
    fsm.AddTransition<EntryState, ExploreState>(2);
    fsm.AddTransition<ExploreState, DiscoverState>(3);
    fsm.AddTransitionRange<DiscoverState>(
        (4, typeof(ExploreState), () => treasureCount < 3),
        (5, typeof(IdleState), () => true));
    fsm.AddTransition<SyncAsyncFSM<MyOwner>.AnyState, MenuState>(6, () => fsm.CurrentState.GetType() != typeof(MenuState));
    fsm.AddTransition<MenuState, SyncAsyncFSM<DungeonRunner>.PreviousState>(7);

    // 1.起動
}
```

```csharp
void AddTransition<TFrom, TTo>(int eventId, Func<bool> gate);
```

指定されたイベントIDを契機として、`TFrom`から`TTo`への遷移を登録します。遷移には条件（ゲート）を付随できます。

```csharp
void AddTransition<TFrom, TTo>(int eventId);
```

常に有効な遷移（ゲートなし）を登録する簡易版。

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

遷移元ステート`TFrom`に対し、複数の遷移先を一括で登録できます。各遷移には条件を指定可能です。

```csharp
void AddTransition<TFrom>(params (int eventId, Type toType, Func<bool> gate)[] to);
```

条件なしで複数の遷移を一括登録する簡易版。

3. **ステートを定義**

同期ステートを定義するには`SyncAsyncFSM<MyOwner>.SyncState`を継承させる必要があります

```csharp:IdleState.cs
public class IdleState : SyncAsyncFSM<MyOwner>.SyncState
{
    // このステートに遷移直後に一度呼ばれる
    public override void Enter()
    {
        
    }

    public override void Update()
    {
        // 状態ロジック
    }

    // 他のステートへ遷移直前に一度呼ばれる
    public override void Exit()
    {
        
    }
}
```

非同期ステートを定義するには`SyncAsyncFSM<MyOwner>.AsyncState`を継承させる必要があります

```csharp:EntryState.cs
public class EntryState : SyncAsyncFSM<MyOwner>.AsyncState
{
    public override async UniTask Start(Cancellation token)
    {
        // 状態ロジック
    }

    // このステートが終了する前に他のステートへ遷移するときや、このFSMを所有するクラスが破棄されたときに呼ばれる
    public override void OnCancel()
    {

    }
}
```

```scharp
MyOwner Owner;
```

ステートクラス内でも`Owner`のインスタンスを取得できます。

4. **遷移の予約・実行**

```csharp
void Schedule(int eventId);
```

現在のステートの遷移テーブルに指定されたイベントIDが登録されていればその遷移が予約されます。

```csharp
void ScheduleAll();
```

現在のステートに登録されている遷移をすべて予約します。

```csharp
void DispatchIfScheduled();
```

予約されている遷移の中から優先度順（登録された順）で遷移条件を満たせば遷移が実行されます。
実行されなかった予約済みの遷移は破棄されます。

```csharp
void Dispatch(int eventId);
```

予約済みの遷移を破棄して指定されたイベントIDの遷移を遷移条件を満たせば実行します。