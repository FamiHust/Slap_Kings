# AI Appearance Management System

Hệ thống quản lý appearance (Skinned Mesh Renderer) cho AI dựa trên level, cho phép AI thay đổi cả stats và appearance qua các level.

## Tính năng chính

- **Level-based Appearance**: AI thay đổi appearance dựa trên level hiện tại
- **Head & Body Management**: Quản lý riêng biệt SkinnedMeshRenderer cho head và body
- **Automatic Integration**: Tự động tích hợp với hệ thống AI hiện có
- **Smooth Transitions**: Chuyển đổi appearance mượt mà
- **Easy Configuration**: Cấu hình dễ dàng qua Inspector

## Cấu trúc hệ thống

### 1. AIAppearanceData (ScriptableObject)
- Cấu hình các appearance sets và level range
- Định nghĩa mesh và material cho head/body
- Validate dữ liệu appearance

### 2. AIAppearanceManager (Component)
- Quản lý SkinnedMeshRenderer cho AI
- Xử lý chuyển đổi appearance dựa trên level
- Tự động tìm và assign SkinnedMeshRenderer

### 3. AIStatsData Integration
- Tích hợp appearance data vào AIStatsData
- Đồng bộ với hệ thống stats hiện có

## Cách sử dụng

### Bước 1: Tạo AIAppearanceData Asset
1. Right-click trong Project window
2. Create > Game > AI Appearance Data
3. Cấu hình các appearance sets với level range:
   - Basic AI: Level 1-3
   - Intermediate AI: Level 4-7
   - Advanced AI: Level 8-12
   - Elite AI: Level 13-18
   - Boss AI: Level 19-25

### Bước 2: Assign Meshes và Materials
1. Drag head mesh vào `headMesh` field
2. Drag head material vào `headMaterial` field
3. Drag body mesh vào `bodyMesh` field
4. Drag body material vào `bodyMaterial` field
5. Lặp lại cho tất cả appearance sets

### Bước 3: Link với AIStatsData
1. Mở AIStatsData asset
2. Assign AIAppearanceData vào `appearanceData` field
3. Hệ thống sẽ tự động tích hợp

### Bước 4: Setup AI Prefab
1. Đảm bảo AI prefab có SkinnedMeshRenderer components
2. Naming convention: "Head" và "Body" trong tên GameObject
3. AIAppearanceManager sẽ tự động tìm và assign

## API Reference

### AIAppearanceManager
```csharp
// Appearance control
void UpdateAppearanceForCurrentLevel()
void ForceUpdateAppearance()
void SetAppearanceData(AIAppearanceData data)

// Renderer management
void SetHeadRenderer(SkinnedMeshRenderer renderer)
void SetBodyRenderer(SkinnedMeshRenderer renderer)

// Events
Action<AIAppearanceData.AppearanceSet> OnAppearanceChanged
Action<int> OnLevelChanged
```

### AIAppearanceData.AppearanceSet
```csharp
string setName
int startLevel
int endLevel
Mesh headMesh
Material headMaterial
Mesh bodyMesh
Material bodyMaterial
bool allowMultipleInstances
float transitionDuration
```

## Ví dụ cấu hình

```csharp
// Basic AI (Level 1-3)
setName: "Basic AI"
startLevel: 1
endLevel: 3
headMesh: BasicHead.fbx
headMaterial: BasicHeadMaterial.mat
bodyMesh: BasicBody.fbx
bodyMaterial: BasicBodyMaterial.mat

// Elite AI (Level 13-18)
setName: "Elite AI"
startLevel: 13
endLevel: 18
headMesh: EliteHead.fbx
headMaterial: EliteHeadMaterial.mat
bodyMesh: EliteBody.fbx
bodyMaterial: EliteBodyMaterial.mat
```

## Tích hợp với Game Flow

Hệ thống tự động tích hợp với game flow:
- **Khi AI spawn**: CharacterFactory tự động thêm AIAppearanceManager
- **Khi level thay đổi**: AIHealth tự động cập nhật appearance
- **Khi stats update**: Appearance được sync với stats
- **Single Source of Truth**: Level được quản lý bởi PersistentDataManager

## Auto-Detection Features

- **SkinnedMeshRenderer Detection**: Tự động tìm renderer theo tên "Head" và "Body"
- **Fallback System**: Nếu không tìm thấy theo tên, sử dụng renderer đầu tiên
- **Component Addition**: Tự động thêm AIAppearanceManager nếu chưa có

## Debug và Testing

- Context menu có sẵn các debug functions
- Debug logs hiển thị thông tin chi tiết
- Validate AppearanceData để kiểm tra cấu hình
- Test appearance update để kiểm tra hoạt động

## Performance Notes

- Appearance chỉ thay đổi khi level thay đổi
- Smooth transitions với coroutines
- Memory efficient với shared mesh references
- No unnecessary updates khi appearance không đổi
