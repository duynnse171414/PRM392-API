# VNPay Quick Start - Tích hợp nhanh trong 5 phút

## 📝 TL;DR (Tóm tắt)

Tôi đã tích hợp VNPay vào API để thanh toán MembershipPackage. Bạn chỉ cần:
1. Lấy `TmnCode` và `HashSecret` từ VNPay
2. Cập nhật `appsettings.json`
3. Gọi API `/api/payment/create-vnpay-payment`
4. Redirect user đến URL thanh toán

## 🚀 Test ngay (3 bước)

### Bước 1: Cấu hình (appsettings.json)

```json
"VnPay": {
  "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
  "ReturnUrl": "http://localhost:5000/api/payment/vnpay-return",
  "TmnCode": "DEMOSHOP",
  "HashSecret": "RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ"
}
```

### Bước 2: Build và chạy

```bash
cd d:\github\david\PRM392-API
dotnet build
dotnet run --project MyApp.Api
```

### Bước 3: Test với Postman/cURL

**1. Tạo payment URL:**
```bash
POST http://localhost:5000/api/payment/create-vnpay-payment
Authorization: Bearer YOUR_JWT_TOKEN

Body (JSON):
{
  "packageId": 1,
  "locale": "vn"
}
```

**2. Copy `paymentUrl` từ response và mở trong trình duyệt**

**3. Thanh toán với thẻ test:**
- Số thẻ: `9704198526191432198`
- Tên: `NGUYEN VAN A`
- Ngày: `07/15`
- OTP: `123456`

**4. Xem kết quả** tại callback URL

## 📍 Lấy TmnCode và HashSecret như thế nào?

### Cách 1: Dùng merchant demo (Test nhanh - 0 phút)
```
TmnCode: DEMOSHOP
HashSecret: RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ
```
✅ Dùng ngay được  
❌ Chỉ để test flow  
❌ Không dùng production  

### Cách 2: Đăng ký Sandbox (Test chính thức - 10 phút)

1. **Đăng ký:** https://sandbox.vnpayment.vn/devreg
2. **Đăng nhập:** https://sandbox.vnpayment.vn/merchantv2/
3. **Lấy thông tin:**
   - Vào: **Thông tin tài khoản** → **Cấu hình API**
   - Copy `TmnCode` và `HashSecret`

✅ Có merchant riêng  
✅ Quản lý giao dịch  
✅ Dùng test lâu dài  

### Cách 3: Production (Chính thức - 1-2 tuần)

1. **Liên hệ:** https://vnpay.vn/lien-he/
2. **Chuẩn bị hồ sơ:**
   - Giấy phép kinh doanh
   - Website/App hoạt động
   - CMND người đại diện
3. **Ký hợp đồng** và nhận thông tin chính thức
4. **Đổi URL** sang production:
   ```json
   "Url": "https://vnpayment.vn/paymentv2/vpcpay.html"
   ```

✅ Dùng thật  
✅ Nhận tiền thật  
💰 Phí ~1.5-2.5%/giao dịch  

## 🔑 Security - Bảo mật

**⚠️ QUAN TRỌNG:**
- `HashSecret` giống như password, **KHÔNG** được commit lên git
- Dùng environment variables cho production:
  ```bash
  export VnPay__HashSecret="YOUR_SECRET"
  export VnPay__TmnCode="YOUR_CODE"
  ```

**❌ SAI:**
```json
// appsettings.json (committed to git)
"HashSecret": "RAOEXHYVSDDIIENYWSLDIIZTANXUXZFJ"
```

**✅ ĐÚNG:**
```json
// appsettings.json
"HashSecret": ""  // Để trống

// Lấy từ environment variable hoặc Azure Key Vault
```

## 📱 Flow đơn giản

```
User → Chọn gói Premium (100,000 VND)
  ↓
Frontend → POST /api/payment/create-vnpay-payment
  ↓
API → Tạo paymentUrl
  ↓
Frontend → Redirect user đến paymentUrl
  ↓
User → Nhập thẻ và xác nhận OTP trên VNPay
  ↓
VNPay → Callback về /api/payment/vnpay-return
  ↓
API → Validate signature → Tạo subscription
  ↓
Frontend → Hiển thị "Thanh toán thành công!"
```

## 📂 Files đã tạo

```
MyApp.Business/
├── Services/
│   ├── VnPayLibrary.cs         # Core VNPay logic
│   ├── IVnPayService.cs        # Interface
│   └── VnPayService.cs         # Service implementation
└── DTOs/
    ├── request/
    │   └── VnPayPaymentRequest.cs
    └── response/
        └── VnPayPaymentResponse.cs

MyApp.Api/
└── Controllers/
    └── PaymentController.cs     # 3 endpoints chính

appsettings.json                 # Cấu hình VNPay
```

## 🎯 API Endpoints

| Endpoint | Method | Auth | Mô tả |
|----------|--------|------|-------|
| `/api/payment/create-vnpay-payment` | POST | ✅ Required | Tạo URL thanh toán |
| `/api/payment/vnpay-return` | GET | ❌ Anonymous | Callback từ VNPay |
| `/api/payment/vnpay-ipn` | GET | ❌ Anonymous | Webhook từ VNPay |
| `/api/payment/payment-status/{orderId}` | GET | ✅ Required | Kiểm tra trạng thái |

## 🧪 Test Checklist

- [ ] Build thành công
- [ ] Tạo payment URL thành công
- [ ] Redirect đến VNPay thành công
- [ ] Thanh toán sandbox thành công
- [ ] Callback trả về đúng kết quả
- [ ] Subscription được tạo trong database
- [ ] Test thanh toán thất bại (hủy giao dịch)

## ❓ FAQs

**Q: Tôi cần đăng ký không?**  
A: Không bắt buộc. Dùng merchant demo để test flow. Nhưng nên đăng ký sandbox để có merchant riêng.

**Q: Production cần làm gì?**  
A: Đăng ký VNPay chính thức, ký hợp đồng, đổi sang URL production và dùng TmnCode/HashSecret production.

**Q: ReturnUrl phải public?**  
A: Có. VNPay cần redirect về URL này. Localhost chỉ test local. Deploy thì phải dùng domain thật.

**Q: Có test mà không cần thẻ thật?**  
A: Có. Dùng thẻ test của VNPay sandbox (xem trên).

**Q: Tôi quên HashSecret?**  
A: Đăng nhập lại dashboard VNPay sandbox/production để xem lại.

## 📖 Đọc thêm

- **Chi tiết:** [VNPAY_INTEGRATION.md](./VNPAY_INTEGRATION.md)
- **VNPay Docs:** https://sandbox.vnpayment.vn/apis/
- **VNPay Sandbox:** https://sandbox.vnpayment.vn/

---

**Tạo bởi:** GitHub Copilot  
**Ngày:** 31/10/2025  
**Version:** 1.0
