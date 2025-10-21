# Slap Kings - OOP Improvements Documentation

## Tổng quan
Dự án đã được cải tiến để áp dụng các nguyên lý OOP tốt hơn, giảm coupling và tăng tính maintainability mà không ảnh hưởng đến logic hiện tại.

## Các cải tiến chính

### 1. Health System - Inheritance & Polymorphism
- **BaseHealth.cs**: Base class chung cho tất cả health systems
- **PlayerHealth.cs**: Kế thừa từ BaseHealth, implement logic riêng cho player
- **AIHealth.cs**: Kế thừa từ BaseHealth, implement logic riêng cho AI

**Lợi ích:**
- Code reuse: Logic chung được chia sẻ
- Polymorphism: Có thể treat tất cả health objects như BaseHealth
- Extensibility: Dễ dàng thêm character types mới
- Events: Unified event system cho health changes

### 2. Data System - Interface & Abstract Classes
- **ICharacterStats**: Interface định nghĩa contract cho character stats
- **BaseCharacterStats**: Abstract base class với common functionality
- **PlayerStatsData**: Kế thừa từ BaseCharacterStats, implement player-specific logic
- **AIStatsData**: Kế thừa từ BaseCharacterStats, implement AI-specific logic

**Lợi ích:**
- Interface Segregation: Clear contracts
- Code reuse: Common stats logic được chia sẻ
- Type safety: Compile-time checking
- Backward compatibility: Legacy methods được giữ lại

### 3. State Machine - State Pattern
- **ICharacterState**: Interface cho character states
- **BaseCharacterState**: Abstract base state
- **Concrete States**: IdleState, WaitingState, HittedState, AttackingState, DeadState
- **StateFactory**: Factory pattern để tạo state instances
- **Enhanced StateMachine**: Improved state machine với better transition control

**Lợi ích:**
- State Pattern: Mỗi state là một object riêng biệt
- Open/Closed Principle: Dễ thêm states mới mà không modify existing code
- Single Responsibility: Mỗi state chỉ handle logic của nó
- Better debugging: State transitions được track rõ ràng

### 4. Manager System - Enhanced Architecture
- **IGameManager**: Interface cho game managers
- **BaseManager**: Base class với common manager functionality
- **EnhancedSingletonManager**: Improved singleton với thread safety
- **ServiceLocator**: Dependency injection pattern
- **GameEventSystem**: Centralized event system

**Lợi ích:**
- Dependency Injection: Loose coupling giữa components
- Service Locator: Centralized service management
- Event System: Decoupled communication
- Thread Safety: Better singleton implementation

### 5. Event System - Observer Pattern
- **IGameEvent**: Interface cho game events
- **GameEvent**: Base class cho events
- **Specific Events**: PlayerHealthChangedEvent, AIDeathEvent, TurnChangedEvent, etc.
- **GameEventSystem**: Centralized event handling

**Lợi ích:**
- Observer Pattern: Loose coupling giữa components
- Centralized Events: Dễ track và debug
- Type Safety: Strongly typed events
- Extensibility: Dễ thêm events mới

### 6. Factory Pattern - Object Creation
- **ICharacterFactory**: Interface cho character creation
- **CharacterFactory**: Concrete factory implementation
- **CharacterCreationData**: Data class cho creation parameters

**Lợi ích:**
- Factory Pattern: Centralized object creation
- Dependency Injection: Factory được register với ServiceLocator
- Configuration: Flexible character creation parameters
- Extensibility: Dễ thêm character types mới

## SOLID Principles Applied

### Single Responsibility Principle (SRP)
- Mỗi class có một responsibility duy nhất
- BaseHealth chỉ handle health logic
- StateMachine chỉ handle state transitions
- EventSystem chỉ handle events

### Open/Closed Principle (OCP)
- Code mở cho extension, đóng cho modification
- Có thể thêm character types mới mà không modify existing code
- Có thể thêm states mới mà không modify StateMachine

### Liskov Substitution Principle (LSP)
- Derived classes có thể substitute base classes
- PlayerHealth và AIHealth có thể được treat như BaseHealth
- Tất cả states có thể substitute ICharacterState

### Interface Segregation Principle (ISP)
- Interfaces được design để clients chỉ depend on methods họ cần
- ICharacterStats chỉ expose necessary properties
- IGameManager chỉ có essential manager methods

### Dependency Inversion Principle (DIP)
- High-level modules không depend on low-level modules
- ServiceLocator cho phép dependency injection
- EventSystem cho phép loose coupling

## Backward Compatibility
Tất cả các cải tiến đều maintain backward compatibility:
- Legacy methods được giữ lại
- Existing code không cần thay đổi
- Gradual migration có thể được thực hiện

## Usage Examples

### Health System
```csharp
// Có thể treat cả PlayerHealth và AIHealth như BaseHealth
BaseHealth health = GetComponent<PlayerHealth>();
health.TakeDamage(10);
health.OnHealthChanged += (current, max) => Debug.Log($"Health: {current}/{max}");
```

### Event System
```csharp
// Subscribe to events
GameEventSystem.Instance.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);

// Publish events
GameEventSystem.Instance.Publish(new PlayerHealthChangedEvent(50, 100, 10));
```

### Factory Pattern
```csharp
// Get factory from ServiceLocator
var factory = ServiceLocator.Instance.Get<ICharacterFactory>();

// Create characters
var player = factory.CreatePlayerAtSpawnPoint();
var ai = factory.CreateAIAtSpawnPoint();
```

## Future Improvements
1. **Command Pattern**: Cho input handling
2. **Strategy Pattern**: Cho different AI behaviors
3. **Builder Pattern**: Cho complex object construction
4. **Decorator Pattern**: Cho character abilities/upgrades
5. **MVC Pattern**: Cho UI management

## Testing
- Unit tests có thể được viết cho từng component riêng biệt
- Mock objects có thể được tạo dễ dàng với interfaces
- Event system cho phép test interactions giữa components

## Performance Considerations
- Object pooling cho frequently created objects
- Event system có error handling để tránh performance issues
- Singleton pattern được optimize với thread safety
- State factory sử dụng caching để tránh repeated object creation
