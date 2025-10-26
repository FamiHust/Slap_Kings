# Hướng dẫn sử dụng Button Mua Skin

## Tổng quan
Đã thêm chức năng button mua skin vào PlayerSkinManager với khả năng kiểm tra đủ tiền và xử lý mua skin tự động.

## Các tính năng đã thêm

### 1. Button Mua Skin trong PlayerSkinManager
- **Method:** `OnBuySkinButtonClicked()`
- **Chức năng:** Xử lý khi button mua skin được click
- **Kiểm tra:** Skin đã mở khóa chưa, đủ tiền không
- **Thực hiện:** Mua skin và trừ tiền nếu đủ điều kiện

### 2. Cập nhật UI Button
- **Method:** `UpdateBuyButtonState()`
- **Chức năng:** Cập nhật trạng thái button dựa trên skin hiện tại
- **Hiển thị:** 
  - "Đã sở hữu" nếu skin đã mở khóa
  - "Mua (X xu)" nếu đủ tiền
  - "Không đủ tiền" nếu không đủ tiền

### 3. Tích hợp với UI Display
- Button tự động cập nhật khi chuyển skin
- Button tự động cập nhật khi có thay đổi về tiền
- Hiển thị chi phí và trạng thái skin

## Cách thiết lập trong Unity

### Bước 1: Thiết lập PlayerSkinManager
1. Mở PlayerSkinManager trong Inspector
2. Gán các UI references:
   - `m_SkinNameText`: Text hiển thị tên skin
   - `m_UnlockCostText`: Text hiển thị chi phí
   - `m_StatusText`: Text hiển thị trạng thái
   - `m_BuySkinButton`: Button mua skin

### Bước 2: Tạo Button Mua Skin
1. Tạo Button trong Canvas
2. Thêm TextMeshProUGUI làm child của button
3. Gán button vào `m_BuySkinButton` trong PlayerSkinManager
4. Gán method `OnBuySkinButtonClicked` vào button's OnClick event

### Bước 3: Sử dụng SkinBuyButtonDemo (Tùy chọn)
1. Thêm script `SkinBuyButtonDemo` vào GameObject UI
2. Gán các button references:
   - `m_BuySkinButton`: Button mua skin
   - `m_NextSkinButton`: Button chuyển skin tiếp theo
   - `m_PreviousSkinButton`: Button chuyển skin trước đó
   - `m_CurrentCoinsText`: Text hiển thị số tiền

## Cách sử dụng trong Code

### Mua skin từ code:
```csharp
// Kiểm tra có thể mua skin không
if (skinManager.CanAffordCurrentSkin())
{
    // Mua skin
    bool success = skinManager.OnBuySkinButtonClicked();
    if (success)
    {
        Debug.Log("Mua skin thành công!");
    }
}
```

### Cập nhật UI:
```csharp
// Cập nhật hiển thị cost và button
skinManager.UpdateCostDisplay();

// Chỉ cập nhật button
skinManager.UpdateBuyButton();

// Cập nhật khi có thay đổi về tiền
skinManager.OnCoinsChanged();
```

## Test Methods

### Trong PlayerSkinManager:
- `TestBuySkinButton()`: Test mua skin
- `TestUpdateBuyButton()`: Test cập nhật button
- `TestUpdateCostDisplay()`: Test cập nhật hiển thị

### Trong SkinBuyButtonDemo:
- `AddTestCoins()`: Thêm 1000 xu để test
- `ResetCoins()`: Reset tiền về 0

## Luồng hoạt động

1. **Khi chuyển skin:**
   - `ApplySkin()` được gọi
   - `UpdateCostDisplay()` được gọi
   - `UpdateBuyButtonState()` được gọi
   - Button cập nhật trạng thái

2. **Khi click button mua skin:**
   - `OnBuySkinButtonClicked()` được gọi
   - Kiểm tra skin đã mở khóa chưa
   - Kiểm tra đủ tiền không
   - Nếu đủ điều kiện: mua skin và trừ tiền
   - Cập nhật UI

3. **Khi có thay đổi về tiền:**
   - Gọi `OnCoinsChanged()`
   - Cập nhật hiển thị cost và button

## Cấu hình Text Settings

Trong PlayerSkinManager, có thể tùy chỉnh:
- `m_UnlockedText`: "✓ Đã mở khóa"
- `m_LockedText`: "✗ Chưa mở khóa"
- `m_CostFormat`: "Chi phí: {0} xu"
- `m_NotEnoughCoins`: "Không đủ tiền!"

## Lưu ý quan trọng

1. **Tích hợp với PersistentDataManager:** Hệ thống sử dụng PersistentDataManager để quản lý tiền
2. **Tự động cập nhật:** Button tự động cập nhật khi chuyển skin hoặc thay đổi tiền
3. **Kiểm tra điều kiện:** Luôn kiểm tra đủ tiền trước khi mua
4. **Debug logs:** Có thể bật/tắt debug logs trong Inspector

## Troubleshooting

**Vấn đề:** Button không cập nhật khi chuyển skin
**Giải pháp:** Kiểm tra `m_BuySkinButton` reference và gọi `UpdateCostDisplay()`

**Vấn đề:** Không thể mua skin dù đủ tiền
**Giải pháp:** Kiểm tra PersistentDataManager và method `SpendCoins()`

**Vấn đề:** Button text không thay đổi
**Giải pháp:** Kiểm tra TextMeshProUGUI component trong button
