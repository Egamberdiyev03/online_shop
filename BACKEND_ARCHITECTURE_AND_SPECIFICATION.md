# OnlineShop Backend: To'liq Arxitektura, Tizim Tuzilishi va Texnik Spetsifikatsiya

> **Hujjat maqsadi:** Ushbu fayl OnlineShop loyihasining backend qismini (ASP.NET Core 8 Web API) to'liq, chuqur tahlil qilib beradi. Yangi funksionalliklarni rejalashtirish, arxitekturani kengaytirish, refaktoring qilish yoki Claude Opus va boshqa ilg'or AI modellariga loyihani kontekst sifatida taqdim etish uchun maxsus tayyorlangan.

---

## 1. Loyihaning Umumiy Umumiy Ko'rinishi (Overview)

- **Loyiha nomi:** OnlineShop (.NET Clean Architecture E-Commerce Platform)
- **Framework va Runtime:** .NET 8.0 (C# 12)
- **Arxitektura uslubi:** Clean Architecture (Onion / Hexagonal tamoyillari asosida 4 qatlamli)
- **Ma'lumotlar bazasi:** PostgreSQL (Npgsql Entity Framework Core Provider)
- **Autentifikatsiya va Avtorizatsiya:** JWT Bearer Tokens, Role-Based Access Control (RBAC), Claims-based filial nazorati
- **Real-vaqt tizimi (Real-Time):** ASP.NET Core SignalR (`/notificationHub`)
- **Fayl saqlash:** Mahalliy statik fayllar (`wwwroot/Receipts`)
- **Email xabarnomalari:** SMTP (Gmail orqali OTP kod yuborish)
- **API Hujjatlashtirish:** Swagger / OpenAPI (Swashbuckle)

---

## 2. Loyiha Qatlamlari va Arxitektura Tuzilishi

Loyiha 4 ta asosiy qatlamdan (`Project`) iborat:

```
OnlineShop (Solution)
│
├── Domain/              ──► Qaramliksiz eng ichki yadro (Entities, Enums, Interfaces)
│
├── DataAccess/          ──► Ma'lumotlar bazasi, EF Core DbContext, Repositories, Migrations
│   └── Bog'liqlik: Domain
│
├── Application/         ──► Biznes mantiq, Servislar, DTOs, AutoMapper, SignalR Hubs
│   └── Bog'liqlik: Domain, DataAccess
│
└── OnlineShop/ (WebAPI) ──► Taqdimot qatlami, Controllerlar, Middleware, DI sozlamalari
    └── Bog'liqlik: Application, DataAccess, Domain
```

---

## 3. Texnologiyalar Steki va Kutubxonalar (Dependencies)

| Komponent / Vazifa | Kutubxona / Versiya | Maqsadi |
| :--- | :--- | :--- |
| **Runtime** | .NET 8.0 | Asosiy platforma |
| **ORM** | `Microsoft.EntityFrameworkCore` (8.0.x) | Relyatsion ma'lumotlar bilan ishlash |
| **Database Driver** | `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL bilan integratsiya |
| **Naming Convention** | `EFCore.NamingConventions` | Snake_case jadval va ustun nomlash (`use_snake_case_naming_convention`) |
| **Authentication** | `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT tokenlarni tekshirish |
| **Parol xavfsizligi** | `BCrypt.Net-Next` | Parollarni xesh qilish va tekshirish |
| **Object Mapping** | `AutoMapper` | Entity <-> DTO o'rtasida konvertatsiya |
| **Real-time** | `Microsoft.AspNetCore.SignalR` | Jonli bildirishnomalar yuborish |
| **Email Jo'natish** | `MailKit` / `System.Net.Mail` | SMTP orqali OTP tasdiqlash xabarlari |
| **API Docs** | `Swashbuckle.AspNetCore` | Swagger UI va OpenAPI hujjatlari |

---

## 4. Domain Qatlami (Entities va Enums)

### 4.1. Entitylar (Ma'lumotlar modellari)

1. **User (`Domain.Entities.User`):**
   - Foydalanuvchilar (Mijozlar, Filial menejerlari, Kompaniya adminlari, Super adminlar).
   - Maydonlari: `Id`, `Name`, `Email`, `PasswordHash`, `PhoneNumber`, `Address`, `Location`, `Role` (enum), `CreatedAt`, `IsEmailConfirmed`, `EmailConfirmationCode`, `EmailConfirmationCodeExpiry`, `CompanyId` (nullable FK), `CompanyBranchId` (nullable FK).
   - Bog'lanishlar: `Company`, `CompanyBranch`, `Orders`, `Comments`, `Cart`.

2. **Company (`Domain.Entities.Company`):**
   - Asosiy kompaniya / korxona.
   - Maydonlari: `Id`, `Name`, `Address`, `PhoneNumber`, `Inn`.
   - Bog'lanishlar: `CompanyBranches` (1 to Many).

3. **CompanyBranch (`Domain.Entities.CompanyBranch`):**
   - Kompaniya filiali (do'kon, ombor yoki tarqatish punkti).
   - Maydonlari: `Id`, `Name`, `Address`, `PhoneNumber`, `Location` (koordinata/manzil), `CompanyId` (FK).
   - Bog'lanishlar: `Company`, `Products`, `Orders`.

4. **Product (`Domain.Entities.Product`):**
   - Tovarlar katalogi.
   - Maydonlari: `Id`, `Name`, `Description`, `Price`, `Quantity` (ombordagi qoldiq), `ImageUrl`, `IsActive`, `CategoryId` (FK), `CompanyBranchId` (FK), `CreatedAt`.
   - Bog'lanishlar: `Category`, `CompanyBranch`, `OrderItems`, `CartItems`, `Comments`.

5. **Category (`Domain.Entities.Category`):**
   - Tovarlar toifalari.
   - Maydonlari: `Id`, `Title`, `Description`, `IsActive`.
   - Bog'lanishlar: `Products`.

6. **Cart & CartItem (`Domain.Entities.Cart`, `CartItem`):**
   - Har bir mijoz uchun 1 ta faol savatcha (`Cart`).
   - Savatcha elementlari (`CartItem`): `CartId`, `ProductId`, `Quantity`.

7. **Order (`Domain.Entities.Order`):**
   - Buyurtmalar.
   - Maydonlari: `Id`, `UserId` (FK), `CompanyBranchId` (FK), `TotalPrice`, `Status` (`OrderStatus` enum), `CreatedAt`, `ProductCount`.
   - Bog'lanishlar: `User`, `CompanyBranch`, `OrderItems`, `Payment`.

8. **OrderItem (`Domain.Entities.OrderItem`):**
   - Buyurtma tarkibidagi tovarlar: `OrderId`, `ProductId`, `Quantity`, `UnitPrice`.

9. **Payment (`Domain.Entities.Payment`):**
   - To'lov yozuvi (chek yuklash va admin tekshiruvi orqali).
   - Maydonlari: `Id`, `OrderId` (FK), `Amount`, `Status` (`PaymentStatus` enum), `ReceiptImageUrl`, `PaymentMethod`, `CreatedAt`, `ConfirmedAt`, `ConfirmedByUserId` (FK), `RejectionReason`.
   - Bog'lanishlar: `Order`.

10. **Notification (`Domain.Entities.Notification`):**
    - Foydalanuvchilar va xodimlar uchun bildirishnomalar.
    - Maydonlari: `Id`, `UserId`, `Type` (`NotificationType` enum), `Title`, `Body`, `IsRead`, `CreatedAt`, `ReadAt`, `RelatedEntityType` (Order/Product/Branch), `RelatedEntityId`, `Link`, `BranchId`, `Priority`.

11. **Comment (`Domain.Entities.Comment`):**
    - Mahsulotlarga yozilgan sharhlar va yulduzli baholash: `Id`, `Content`, `UserId`, `ProductId`, `CreatedAt`, `UpdatedAt`, `StarRating`.

---

### 4.2. Muhim Enumlar

- **Role:**
  - `0 = Customer` — oddiy xaridor.
  - `1 = BranchManager` — faqat o'z filialiga tegishli buyurtmalar va mahsulotlarni boshqaruvchi xodim.
  - `2 = CompanyAdmin` — kompaniya miqyosidagi admin.
  - `3 = SuperAdmin` — platforma egasi / bosh administrator.

- **OrderStatus:**
  - `0 = Pending` (Kutilmoqda — yangi yaratilgan buyurtma, to'lovi tasdiqlanmagan).
  - `1 = Confirmed` (Tasdiqlangan — to'lov tekshirilib qabul qilingan, omborga uzatilgan).
  - `2 = Shipped` (Jo'natilgan — kuryer yo'lda).
  - `3 = Delivered` (Yetkazildi — mijoz qabul qilgan, yakuniy holat).
  - `4 = Cancelled` (Bekor qilingan — bekor bo'lgan, ombor qoldiqlari qaytarilgan).

- **PaymentStatus:**
  - `0 = Pending` (Chek yuborilgan, admin tekshirishi kutilmoqda).
  - `1 = Completed` (To'lov qabul qilingan va tasdiqlangan).
  - `2 = Failed` (To'lov rad etilgan).
  - `3 = Refunded` (Pul qaytarilgan).
  - `4 = Cancelled` (Bekor qilingan).

- **NotificationType:**
  - `OrderCreated`, `OrderConfirmed`, `OrderShipped`, `OrderDelivered`, `OrderCancelled`, `PaymentSucceeded`, `PaymentFailed`, `PaymentReceiptUploaded`, va boshqalar.

---

## 5. DataAccess Qatlami (Ma'lumotlar bazasi va Repozitoriylar)

### 5.1. `AppDbContext`
- Barcha entitylar uchun `DbSet`lar e'lon qilingan.
- `UseSnakeCaseNamingConvention()` orqali PostgreSQL-da ustunlar `created_at`, `user_id` kabi formatlanadi.

### 5.2. Generic Repository Pattern (`IRepository<T>` va `Repository<T>`)
- Asosiy metodlar:
  - `AddAsync(entity)`, `AddRangeAsync(entities)`
  - `GetByIdAsync(id)`, `GetAllAsync()`
  - `UpdateAsync(entity)`, `DeleteAsync(id)`
  - `GetAsQueryable()` — LINQ va `Include` orqali optimal so'rovlar yozish imkonini beradi.
  - `SaveChangesAsync()`

### 5.3. Maxsus Repozitoriylar
- `INotificationRepository` / `NotificationRepository`: Foydalanuvchiga tegishli bildirishnomalarni sahifalab olish (`GetUserNotificationsAsync`), o'qilmaganlar sonini hisoblash (`GetUnreadCountAsync`) va barchasini o'qilgan qilish (`MarkAllAsReadAsync`).

---

## 6. Application Qatlami (Biznes Mantiq va Servislar)

### 6.1. Autentifikatsiya va Xavfsizlik (`AuthService`, `TokenService`)
- **Ro'yxatdan o'tish (Register):**
  - Foydalanuvchi ma'lumotlari kiritiladi, parol `BCrypt` orqali xeshlanadi.
  - 6 xonali tasodifiy OTP kod generatsiya qilinadi (10 daqiqalik amal qilish muddati).
  - Foydalanuvchi emailiga `EmailService` orqali OTP kod yuboriladi (`IsEmailConfirmed = false`).
- **Emailni tasdiqlash (ConfirmEmail):**
  - Kod to'g'ri kiritilsa, `IsEmailConfirmed = true` qilinadi va JWT token qaytariladi.
- **Tizimga kirish (Login):**
  - Email va parol tekshiriladi (`BCrypt.Verify`).
  - JWT token yaratiladi. Claimlar: `UserId`, `Email`, `Role`, `companyBranchId`, `companyId`. Token muddati 60 daqiqa (sozlanadi).

### 6.2. Savatcha Mantig'i (`CartService`)
- Har bir mijoz faqat bitta savatchaga ega.
- Mahsulot qo'shish, miqdorini oshirish/kamaytirish, mahsulotni o'chirish.
- Miqdorlar bazadagi `Product.Quantity` bilan cheklanadi.

### 6.3. Buyurtma Mantig'i (`OrderService`)
- **Buyurtma yaratish (`CreateOrder`):**
  1. Savatcha va uning elementlari yuklanadi.
  2. Ombordagi mahsulot qoldig'i (`Quantity`) yetarli ekani qat'iy tekshiriladi.
  3. Mahsulot qoldig'i ombordan ayirib tashlanadi (`product.Quantity -= item.Quantity`).
  4. Yangi `Order` (`Pending` holatida) va tegishli `OrderItem`lar yaratiladi.
  5. Savatcha tozalanadi.
  6. Mijozga va filial mas'ullariga SignalR orqali xabar yuboriladi.
- **Buyurtma holatini yangilash (`UpdateOrderStatus`):**
  - `Cancelled` yoki `Delivered` holatidagi buyurtmalarni o'zgartirish bloklangan (immutability).
  - Yangilanish bo'yicha mijozga real-vaqtda bildirishnoma jo'natiladi (`Shipped`, `Delivered`).
- **Buyurtmani bekor qilish (`CancelOrder`):**
  - Faqat hali yetkazilmagan yoki jo'natilmagan buyurtmalarni bekor qilish mumkin.
  - Buyurtmadagi tovarlar yana ombor qoldig'iga qaytariladi (`product.Quantity += item.Quantity`).
  - Mijoz va filial administratorlariga bekor qilinganlik haqida xabarnoma yuboriladi.
- **Filial bo'yicha cheklov:**
  - `BranchManager` faqat o'z filialiga tegishli buyurtmalarni ko'rish va statusini boshqarish huquqiga ega.

### 6.4. To'lov Mantig'i (`PaymentService`)
- **Chek yuklash (`UploadReceipt`):**
  - Rasmlar yoki PDF cheklar `wwwroot/Receipts` papkasiga saqlanadi (maksimal 5MB, `.jpg`, `.jpeg`, `.png`, `.pdf`).
- **To'lov yaratish (`CreatePayment`):**
  - To'lov summasi buyurtmaning `TotalPrice` iga teng bo'lishi shart.
  - Yangi `Payment` (`Pending`) saqlanadi.
  - Filial menejerlari va SuperAdminga darhol xabarnoma boradi.
- **To'lovni tasdiqlash (`ConfirmPayment`):**
  - Admin yoki filial boshqaruvchisi chekni ko'rib tasdiqlaydi.
  - `Payment.Status = Completed` bo'ladi.
  - Bog'langan `Order.Status` avtomatik ravishda `Confirmed` ga o'zgaradi!
  - Mijozga to'lov qabul qilingani haqida bildirishnoma yetkaziladi.
- **To'lovni rad etish (`RejectPayment`):**
  - Sababi ko'rsatilgan holda rad etiladi (`Payment.Status = Failed`).
  - Bog'langan buyurtma bekor qilinadi (`Order.Status = Cancelled`).

### 6.5. Real-Vaqt Bildirishnomalar (`NotificationService` & `NotificationHub`)
- SignalR orqali individual foydalanuvchiga (`Clients.User(userId)`) yoki foydalanuvchilar guruhiga jonli xabar uzatiladi.
- Xabarlar bir vaqtning o'zida PostgreSQL bazasiga `Notification` jadvaliga yoziladi, shuning uchun foydalanuvchi oflayn bo'lsa ham keyinroq ko'ra oladi.

---

## 7. Web API Qatlami (Controllerlar va Marshrutlar)

| Controller | Asosiy Marshrutlar | Ruxsatlar (Roles) | Tavsif |
| :--- | :--- | :--- | :--- |
| **AuthController** | `POST /Auth/Register`<br>`POST /Auth/ConfirmEmail`<br>`POST /Auth/Login`<br>`POST /Auth/ForgotPassword`<br>`POST /Auth/ResetPassword` | Ochiq (AllowAnonymous) | Ro'yxatdan o'tish, tasdiqlash va kirish |
| **OrderController** | `POST /Order/Create`<br>`GET /Order/GetById`<br>`GET /Order/GetAll`<br>`GET /Order/GetOrderByUserId`<br>`PUT /Order/UpdateStatus`<br>`PUT /Order/Cancel`<br>`GET /Order/GetOrdersByBranchId`<br>`GET /Order/GetOrdersByStatus`<br>`GET /Order/GetOrdersByDateRange` | Autentifikatsiyadan o'tganlar.<br>`GetAll`, `UpdateStatus`, `GetOrdersByBranchId` faqat `SuperAdmin`, `CompanyAdmin`, `BranchManager` (filial filtratsiyasi bilan) | Buyurtma yaratish va boshqarish |
| **PaymentController** | `POST /Payment/create`<br>`GET /Payment/get-by-order-id`<br>`GET /Payment/get-all`<br>`POST /Payment/confirm`<br>`POST /Payment/reject`<br>`POST /Payment/upload-receipt` | Chek yuklash va yaratish mijozga ochiq.<br>`confirm`/`reject` faqat admin va menejerlarga | To'lovlarni qabul qilish va tasdiqlash |
| **ProductController** | `GET /Product/GetAll`<br>`GET /Product/GetPaged`<br>`GET /Product/GetById`<br>`POST /Product/Create`<br>`PUT /Product/Update`<br>`DELETE /Product/Delete` | O'qish hamma uchun ochiq.<br>Yaratish/o'chirish faqat `SuperAdmin`, `CompanyAdmin` | Mahsulotlar katalogi va sahifalash |
| **CartController** | `POST /Cart/Create`<br>`GET /Cart/GetByCustomerIdCart`<br>`POST /Cart/AddItemtoCart`<br>`PUT /Cart/Update`<br>`DELETE /Cart/RemoveItemfromCart` | Mijozlar va Adminlar | Foydalanuvchi savatchasi |
| **CategoryController**| `GET /Category/GetAll`<br>`POST /Category/Create`<br>`PUT /Category/Update`<br>`DELETE /Category/Delete` | Ko'rish ochiq, boshqaruv Adminlarga | Kategoriyalar |
| **CompanyController** | `GET /Company/GetAll`, `POST /Company/Create`, etc. | `SuperAdmin` | Kompaniyalarni boshqarish |
| **CompanyBranchController** | `GET /CompanyBranch/GetAll`, `POST /CompanyBranch/Create`, etc. | `SuperAdmin`, `CompanyAdmin` | Filiallarni boshqarish |
| **UserController** | `GET /User/GetById`, `GET /User/GetAll`, `POST /User/AssignBranchManager`, etc. | Admin boshqaruvi | Xodimlar va rollarni tayinlash |
| **NotificationController** | `GET /Notification/my`, `GET /Notification/unread-count`, `POST /Notification/mark-read`, `POST /Notification/mark-all-read` | Autentifikatsiya qilingan har bir user | Bildirishnomalar tarixi |
| **CommentController** | `POST /Comment/Create`, `GET /Comment/GetByProductId`, etc. | Foydalanuvchilar | Mahsulot sharhlari va reyting |

---

## 8. Konfiguratsiya va Muhit Sozlamalari (`appsettings.json`)

- **Baza ulanishi:** `Host=localhost;Database=OnlineShopDatabase;Username=postgres;Password=1234`
- **JWT sozlamalari:**
  - `Issuer`: `OnlineShop`
  - `Audience`: `OnlineShopUsers`
  - `ExpireMinutes`: `60`
- **SignalR Autentifikatsiyasi:** WebSockets HTTP sarlavhalarini doim ham yubormagani uchun, token `?access_token=...` orqali olinadi (`JwtBearerEvents.OnMessageReceived` da ushlab qolinadi).
- **CORS Siyosati ("Front"):**
  - Ruxsat etilgan domenlar: `http://localhost:4200`, `http://onlineshopuz.runasp.net`, `https://onlineshopuz.runasp.net`
  - Sozlamalar: `AllowAnyHeader`, `AllowAnyMethod`, `AllowCredentials` (SignalR uchun shart).

---

## 9. Mavjud Kuchli Tomonlar (Strengths)

1. **Toza Qatlamli Arxitektura:** Kod qat'iy ajratilgan, Domain qatlami mutlaqo mustaqil.
2. **Keng qamrovli biznes oqimi:** Savat -> Buyurtma -> Chek yuklash -> Admin tasdiqlashi -> Omborga uzatish -> Yetkazib berish zanjiri to'liq amalga oshirilgan.
3. **Ombor qoldig'i xavfsizligi:** Buyurtma berilganda qoldiq kamayadi, bekor qilinganda yana qaytariladi.
4. **Filiallar miqyosida ajratish (Multi-branch / Multi-tenant):** `BranchManager` faqat o'z filialining ma'lumotlari bilan ishlay oladi.
5. **Real-time SignalR bildirishnomalar:** Buyurtma va to'lov o'zgarishlari mijoz va adminga bir zumda yetib boradi.

---

## 10. Zaifliklar, Cheklovlar va Texnik Qarzlar (Known Gaps & Technical Debt)

Claude Opus bilan yangi ishlarni rejalashtirishda e'tibor qaratilishi kerak bo'lgan asosiy nuqtalar:

1. **Raqobatbardosh tranzaksiyalar (Race conditions in Stock Deduction):**
   - Agar bir vaqtning o'zida ikkita mijoz oxirgi qolgan tovarga buyurtma bersa, ikkalasiga ham tasdiqlanib qolishi mumkin. Buni hal qilish uchun EF Core `Database Transaction` (yoki `RowVersion` / `Pessimistic locking`) qo'llanilishi kerak.
2. **Mahalliy Fayl Saqlash (`wwwroot/Receipts`):**
   - Hozirda to'lov cheklari serverning o'z diskiga saqlanmoqda. Agar server konteynerda (Docker) yoki bulutda (masalan, AWS/Azure/Render) restart bo'lsa yoki bir nechta instansiyaga ko'paytirilsa (load balancing), fayllar yo'qoladi. Buni **Cloud Object Storage (AWS S3, MinIO yoki Cloudflare R2)** ga o'tkazish tavsiya etiladi.
3. **Avtomatlashtirilgan To'lov Shlyuzlari (Payment Gateways):**
   - Hozirgi to'lov tizimi qo'lda chek yuklashga asoslangan. Click, Payme, Uzum Bank kabi mahalliy to'lov provayderlarining to'g'ridan-to'g'ri Webhook / Merchant API integratsiyasi kiritilmagan.
4. **Kesh tizimining yo'qligi (Caching):**
   - Mahsulotlar katalogi, kategoriyalar va filiallar ro'yxati har safar PostgreSQL-dan qayta o'qilmoqda. `IMemoryCache` yoki taqsimlangan `Redis` keshini joriy qilish orqali unumdorlikni 5-10 baravarga oshirish mumkin.
5. **Global Xatoliklarni boshqarish (Global Exception Handling Middleware):**
   - Hozirda xatoliklar asosan har bir servisda qo'lda yoki controller darajasida qaytariladi. Tizimli `GlobalExceptionHandlerMiddleware` yoki ASP.NET Core 8 ning yangi `IExceptionHandler` interfeysidan foydalanish maqsadga muvofiq.
6. **Avtotestlar (Unit & Integration Tests):**
   - Loyihada test loyihasi (xUnit / NUnit) mavjud emas. Servislar biznes mantig'i uchun unit testlar va API uchun integratsion testlar yozish lozim.

---

## 11. Yangi Rivojlanish Bosqichlari Uchun Reja (Roadmap for Next Sprints)

Claude Opus yordamida quyidagi ustuvor vazifalarni rejalashtirish va amalga oshirish tavsiya etiladi:

- **1-bosqich: To'lov tizimini avtomatlashtirish:**
  - Payme va Click uchun alohida Webhook controllerlari va servislarini joriy qilish.
  - To'lov holatlarini avtomatik tekshiruvchi foniy vazifa (Background Worker / Quartz.NET).
- **2-bosqich: Kuryerlik va Yetkazib berish xizmati:**
  - Kuryerlar uchun alohida rol (`Courier`) qo'shish.
  - Buyurtmani kuryerga biriktirish, uning koordinatalarini real-vaqtda kuzatish (SignalR orqali xaritada ko'rsatish).
- **3-bosqich: Analitika va Dashboard:**
  - SuperAdmin va Filial menejerlari uchun kunlik/oylik savdo tushumi, eng ommabop mahsulotlar bo'yicha analitik APIlar (`Dapper` yoki optimallashtirilgan SQL orqali).
- **4-bosqich: Bulutli Fayl Saqlash va Xavfsizlik:**
  - Fayllarni S3/MinIO ga yuklash servisini yaratish.
  - `Rate Limiting` (API so'rovlar sonini cheklash — `AspNetCoreRateLimit` yoki .NET 8 RateLimiter).
