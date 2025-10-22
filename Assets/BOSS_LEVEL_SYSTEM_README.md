# Boss Level System

Hệ thống quản lý boss levels với các đặc điểm đặc biệt: tăng speed của PowerMeter và CounterMeter, tăng gấp đôi health và damage.

## Tính năng chính

- **Boss Level Detection**: Tự động phát hiện level boss
- **Speed Bonus**: PowerMeter và CounterMeter speed +1
- **Health Multiplier**: Health tăng x2 (40 thay vì 20)
- **Damage Multiplier**: Damage tăng x2
- **Easy Configuration**: Dễ dàng tắt/bật trong Inspector

## Cấu trúc hệ thống

### 1. BossLevelData (ScriptableObject)
- Cấu hình các boss levels và multipliers
- Định nghĩa speed bonus, health/damage multipliers
- Validate dữ liệu boss levels

### 2. Boss Level Integration
- Tích hợp với AIStatsData và PlayerStatsData
- Tự động áp dụng multipliers khi detect boss level
- Cập nhật PowerMeter speed với boss bonus

### 3. DataManager Integration
- Cung cấp API để check boss level
- Tự động áp dụng multipliers cho health/damage
- Cung cấp boss name và color

## Cách sử dụng

### Bước 1: Tạo BossLevelData Asset
1. Right-click trong Project window
2. Create > Game > Boss Level Data
3. Cấu hình các boss levels:
   - Level 5: First Boss
   - Level 10: Second Boss
   - Level 15: Third Boss
   - Level 20: Fourth Boss
   - Level 25: Final Boss

### Bước 2: Assign BossLevelData
1. Mở AIStatsData asset
2. Assign BossLevelData vào `bossLevelData` field
3. Mở PlayerStatsData asset
4. Assign BossLevelData vào `bossLevelData` field

### Bước 3: Cấu hình Boss Levels
1. Mở BossLevelData asset
2. Cấu hình từng boss level:
   - `level`: Số level của boss
   - `bossName`: Tên boss
   - `isActive`: Có kích hoạt boss này không
   - `healthMultiplier`: Hệ số nhân health (2.0 = gấp đôi)
   - `damageMultiplier`: Hệ số nhân damage (2.0 = gấp đôi)
   - `speedBonus`: Bonus speed cho PowerMeter (+1.0)

## API Reference

### BossLevelData
```csharp
// Boss level detection
bool IsBossLevel(int level)
BossLevel GetBossLevel(int level)

// Multipliers
float GetHealthMultiplier(int level)
float GetDamageMultiplier(int level)
float GetSpeedBonus(int level)

// Visual
string GetBossName(int level)
```

### DataManager
```csharp
// Boss level info
bool IsBossLevel(int level)
string GetBossName(int level)

// Stats with boss multipliers
int GetAIMaxHealth(int level) // Auto-applies boss multiplier
int GetAIMinDamage(int level) // Auto-applies boss multiplier
int GetAIMaxDamage(int level) // Auto-applies boss multiplier

// PowerMeter speed with boss bonus
float GetPowerMeterSpeedWithBossBonus(int level)
```

## Ví dụ cấu hình

```csharp
// Boss Level 5
level: 5
bossName: "First Boss"
isActive: true
healthMultiplier: 2.0f  // Health tăng gấp đôi
damageMultiplier: 2.0f  // Damage tăng gấp đôi
speedBonus: 1.0f       // Speed +1

// Normal Level 6
// Không có boss multiplier, stats bình thường
```

## Boss Level Effects

### Normal Level (Level 6):
- Health: 200 + (6-1) * 20 = 300
- Damage: 10-30 + (6-1) * 5 = 35-55
- PowerMeter Speed: 2.0 (base speed)

### Boss Level (Level 5):
- Health: (200 + (5-1) * 20) * 2.0 = 560
- Damage: (10-30 + (5-1) * 5) * 2.0 = 60-100
- PowerMeter Speed: 2.0 + 1.0 = 3.0

## Tích hợp với Game Flow

Hệ thống tự động tích hợp:
- **Level Detection**: Tự động detect boss level từ PersistentDataManager
- **Stats Application**: Tự động áp dụng multipliers cho health/damage
- **Speed Update**: Tự động cập nhật PowerMeter speed
- **Visual Feedback**: Debug logs hiển thị khi vào boss level

## Debug và Testing

- Context menu có sẵn các debug functions
- Debug logs hiển thị khi vào boss level
- Validate BossLevelData để kiểm tra cấu hình
- Test boss level detection

## Performance Notes

- Boss detection chỉ check khi level thay đổi
- Multipliers được tính toán real-time
- No performance impact khi không phải boss level
- Efficient lookup với List iteration

## Easy Toggle

- `EnableBossLevels`: Tắt/bật toàn bộ hệ thống boss
- `isActive`: Tắt/bật từng boss level riêng lẻ
- Inspector-friendly configuration
- Runtime modification support
