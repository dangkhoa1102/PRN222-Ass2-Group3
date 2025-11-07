# PRN222 Assignment 2 – EV Dealer System

## Tổng quan
Dự án xây dựng một hệ thống quản lý đại lý xe điện với giao diện web dựa trên Razor Pages. Ứng dụng hỗ trợ người dùng đăng ký, đăng nhập, đặt lịch lái thử, theo dõi đơn hàng và nhận thông báo thời gian thực; trong khi quản trị viên có thể quản lý đơn hàng, lịch hẹn và danh mục xe.

## Kiến trúc giải pháp
- `Assignment02`: Ứng dụng ASP.NET Core Razor Pages (net8.0) chịu trách nhiệm giao diện, định tuyến, xác thực cookie và tích hợp SignalR.
- `Business_Logic_Layer`: Tầng nghiệp vụ chứa DTO, Service Factory và các service quản lý người dùng, đơn hàng, lịch hẹn, xe và đại lý.
- `DataAccess_Layer`: Tầng truy cập dữ liệu dùng Entity Framework Core (SqlServer) với DbContext `EVDealerSystemContext`, mô hình hóa các thực thể `User`, `Order`, `Vehicle`, `Dealer`, `TestDriveAppointment`, `OrderHistory` và repository pattern.
- `Database.sql`: Script tạo/truy vấn cơ sở dữ liệu SQL Server dùng để khởi tạo dữ liệu mẫu.
- `.github/`: Workflow tự động cho CI/CD (nếu cần mở rộng).

```
PRN222_Ass2_Group03/
├── Assignment02/           # Razor Pages + SignalR front-end & presentation layer
├── Business_Logic_Layer/   # Services, DTOs và ServiceFactory
├── DataAccess_Layer/       # EF Core DbContext, models, repositories
├── Database.sql            # Script thiết lập cơ sở dữ liệu
├── ASS 2.docx              # Tài liệu mô tả yêu cầu bài tập
└── PRN222_Ass2_Group03.sln
```

## Công nghệ chính
- **.NET 8** với ASP.NET Core Razor Pages và middleware session + authentication cookie.
- **SignalR** (`RealTimeHub`, `RealTimeNotificationService`) cung cấp thông báo thời gian thực (ví dụ: cập nhật đơn hàng, lịch hẹn).
- **Entity Framework Core 8** (`SqlServer`, `Tools`, `Design`) cho truy cập dữ liệu và scaffolding.
- **Layered Architecture**: Presentation → Business Logic → Data Access nhằm tách biệt trách nhiệm, dễ bảo trì và kiểm thử.
- **LibMan & Static Assets** trong `wwwroot/` phục vụ Bootstrap, JavaScript, CSS cho giao diện.

# Account đăng nhập
## Username
moime ( Customer )
evm_staff1( Staff)
## Password
123456 ( cho tất cả account )