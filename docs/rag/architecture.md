# Architecture

Decision (2026-10-03): the game uses a **state machine with state handlers** and **dependency injection**. This refines GDD section 7; the module list there still applies, but modules are wired as described here.

## State machine with state handlers

- One `GameStateMachine` owns the session flow: `Intro → Wave → Breather → Redeem → EndCard` (Wave/Breather repeat for 3 waves).
- Each state is its own handler class implementing a small interface:

```csharp
public interface IGameState
{
    void Enter();
    void Tick(float deltaTime); // scaled time only
    void Exit();
}
```

- Handlers hold no static state. Transitions are requested through the machine (`machine.Enter<WaveState>()`), never by handlers calling each other directly.
- Only the machine's owner MonoBehaviour has `Update`; it forwards `Time.deltaTime` to the active handler. Pausing on page hide (`timeScale = 0`) therefore freezes every handler at once.
- Handlers subscribe to events in `Enter` and unsubscribe in `Exit`, so restart and state changes never leave listeners behind.

## Dependency injection

- **Manual constructor injection with a single composition root**, no DI framework (Zenject/VContainer add code size against the 5 MB ZIP budget).
- `GameInstaller` (MonoBehaviour in `Main`) is the composition root: it reads `GameConfig`, creates plain C# services (`Economy`, `WaveSpawner`, `SlotManager`, `InputRouter`, `ObjectPool`s), creates the state handlers passing their dependencies through constructors, and starts the machine.
- Scene objects (views, slots, path waypoints) are serialized references on the installer and passed down; services never call `FindObjectOfType` or singletons.
- Services depend on interfaces where a fake helps testing or swapping (e.g. `IPageVisibility`), concrete types elsewhere. Keep it lean.
- Restart reloads the scene: the installer is rebuilt, so the whole object graph is recreated from scratch with no leftover state.

## Rules

- No `static` mutable state, no singletons.
- Gameplay code uses scaled time only (`Time.deltaTime`), never `unscaledDeltaTime` or wall-clock time.
- Every subscription has a matching unsubscription (`Enter/Exit`, `OnEnable/OnDisable`, or `Dispose`).
