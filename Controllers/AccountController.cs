using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using _66022380.Models;
using _66022380.Models.Db;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using _66022380.Helpers;
using System.IO;
namespace _66022380.Controllers;

public class AccountController : Controller
{
    private readonly BakerydbContext _db;
    private readonly ILogger<AccountController> _logger;

    public AccountController(BakerydbContext db, ILogger<AccountController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // บันทึกการเข้าสู่ระบบลงในไฟล์
    private void LogUserLogin(string username, bool success, string ipAddress)
    {
        try
        {
            string logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            string logFile = Path.Combine(logDir, $"login_{DateTime.Now:yyyy-MM-dd}.log");
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Username: {username} | Success: {success} | IP: {ipAddress}\n";

            using (var writer = System.IO.File.AppendText(logFile))
            {
                writer.WriteLine(logMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error logging user login: {ex.Message}");
        }
    }

    // ─── หน้า User ───────────────────────────────────────
    public IActionResult Home() => View();
    public IActionResult Signup() => View();

    public IActionResult Profile(int id = 0)
    {
        if (id <= 0)
        {
            id = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : 0;
        }

        if (id <= 0)
            return RedirectToAction("Login");

        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
            return RedirectToAction("Login");

        var addresses = _db.Addresses.Where(a => a.UserId == id).ToList();

        var profileModel = new ProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Addresses = addresses
        };

        return View(profileModel);
    }

    [HttpPost]
    public IActionResult Profile([FromBody] ProfileViewModel model)

    {
        if (model.UserId <= 0)
            return Json(new { success = false, message = "ข้อมูลผู้ใช้ไม่ถูกต้อง" });

        var user = _db.Users.Find(model.UserId);
        if (user == null)
            return Json(new { success = false, message = "ไม่พบข้อมูลผู้ใช้" });

        // Update profile info only if they are provided (not empty)
        if (!string.IsNullOrWhiteSpace(model.Username))
            user.Username = model.Username;
        if (!string.IsNullOrWhiteSpace(model.Email))
            user.Email = model.Email;
        if (!string.IsNullOrWhiteSpace(model.Phone))
            user.Phone = model.Phone;

        // Check if user wants to change password
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            // Validate current password was provided
            if (string.IsNullOrEmpty(model.CurrentPassword))
                return Json(new { success = false, message = "กรุณากรอกรหัสผ่านปัจจุบัน" });

            // Compare passwords with robust handling for whitespace and encoding
            string storedPassword = (user.Password ?? "").Trim();
            string enteredPassword = (model.CurrentPassword ?? "").Trim();

            // Use case-sensitive comparison with ordinal rules
            bool passwordsMatch = string.Equals(storedPassword, enteredPassword, StringComparison.Ordinal);

            if (!passwordsMatch)
                return Json(new { success = false, message = "รหัสผ่านปัจจุบันไม่ถูกต้อง" });

            // Validate new password matches confirmation
            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "รหัสผ่านใหม่ไม่ตรงกัน" });

            // Validate password length
            if (model.NewPassword.Length < 6)
                return Json(new { success = false, message = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร" });

            // Update password
            user.Password = model.NewPassword;
        }

        _db.SaveChanges();

        return Json(new { success = true, message = "บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว!" });
    }

    [HttpPost]
    public IActionResult SaveAddress(int userId, int addressId, string addressLine, string district, string province, string postalCode)
    {
        if (addressId > 0)
        {
            var address = _db.Addresses.Find(addressId);
            if (address != null && address.UserId == userId)
            {
                address.AddressLine = addressLine;
                address.District = district;
                address.Province = province;
                address.PostalCode = postalCode;
                _db.SaveChanges();
            }
        }
        else
        {
            var newAddress = new Address
            {
                UserId = userId,
                AddressLine = addressLine,
                District = district,
                Province = province,
                PostalCode = postalCode
            };
            _db.Addresses.Add(newAddress);
            _db.SaveChanges();
        }

        return Json(new { success = true, message = "บันทึกที่อยู่เรียบร้อยแล้ว!" });
    }

    [HttpPost]
    public IActionResult DeleteAddress(int addressId, int userId)
    {
        var address = _db.Addresses.Find(addressId);
        if (address != null && address.UserId == userId)
        {
            _db.Addresses.Remove(address);
            _db.SaveChanges();
            return Json(new { success = true, message = "ลบที่อยู่เรียบร้อยแล้ว!" });
        }

        return Json(new { success = false, message = "ไม่พบที่อยู่หรือคุณไม่มีสิทธิ์ลบ" });
    }

    public IActionResult Menu()
    {
        var products = _db.Stocks
            .Include(s => s.Category)
            .ToList();
        return View(products);
    }
    public IActionResult lab8() => View();
    public IActionResult Delivery()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return View(new DeliveryTrackingViewModel());

        var orders = _db.Orders
            .Include(o => o.Address)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .Where(o => o.UserId == userId && o.Status != "Cancelled")
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .ToList();

        var activeOrder = orders.FirstOrDefault(o => o.Status != "Completed");

        return View(BuildDeliveryTrackingViewModel(activeOrder, orders));
    }
    public IActionResult Checkout() => View();

    [HttpGet]
    public IActionResult GetUserAddresses(int userId)
    {
        var addresses = _db.Addresses.Where(a => a.UserId == userId).ToList();
        return Json(new { success = true, addresses = addresses });
    }

    [HttpGet]
    public IActionResult GetUserPromos(int userId)
    {
        var promos = _db.UserPromotions
            .Include(up => up.Promotion)
            .Where(up => up.UserId == userId && up.IsUsed == 0 && up.Promotion != null)
            .Select(up => new
            {
                up.PromotionId,
                up.Promotion.PromotionName,
                up.Promotion.Description,
                up.Promotion.DiscountValue,
                up.Promotion.DiscountType
            })
            .ToList();
        return Json(new { success = true, promos = promos });
    }

    // ─── หน้า Admin ──────────────────────────────────────
    public IActionResult Dashbordadmin() => View("~/Views/admin/Dashbordadmin.cshtml");
    public IActionResult Stock() => View("~/Views/admin/Stock.cshtml");
    public IActionResult Order()
    {
        var orders = _db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View("~/Views/admin/Order.cshtml", orders);
    }

    // ─── Auth ─────────────────────────────────────────────
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        var user = _db.Users.FirstOrDefault(u =>
            (u.Username == username || u.Email == username) &&
            u.Password == password
        );

        if (user == null)
        {
            LogUserLogin(username, false, ipAddress);
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        LogUserLogin(username, true, ipAddress);

        if (user.RoleId == 1)
            return RedirectToAction("Dashbordadmin");

        return RedirectToAction("Home");
    }

    public IActionResult Lab10(int id = 0)
    {
        var check = (from us in _db.Users
                     where us.UserId == id
                     select new UserViewModel
                     {
                         UserId = us.UserId,
                         Username = us.Username,
                         Email = us.Email,
                         Phone = us.Phone
                     }).FirstOrDefault();
        return View(check);
    }

    [HttpPost]
    public IActionResult Lab10(UserViewModel model)
    {
        if (model.UserId > 0)
        {

            var user = _db.Users.Find(model.UserId);
            if (user != null)
            {
                user.Username = model.Username;
                user.Email = model.Email;
                user.Phone = model.Phone;
                _db.SaveChanges();
            }
        }
        else
        {
            // Add new user
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                Password = "default123",
                RoleId = 3
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();
        }

        return RedirectToAction("Member");
    }

    public IActionResult Lab10D(String id)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserId == int.Parse(id));

        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }

        return RedirectToAction("Member", "Account");
    }



    [HttpPost]
    public IActionResult Signup(string username, string email, string phone, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match";
            return View();
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            Phone = phone,
            Password = password,
            RoleId = 3
        };

        _db.Users.Add(newUser);
        _db.SaveChanges();

        return RedirectToAction("Login");

    }

    // สั่งซื้อสินค้า
    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest model)
    {
        if (model == null || model.Items == null || !model.Items.Any())
            return Json(new { success = false, message = "ข้อมูลออเดอร์ไม่ถูกต้อง" });

        var order = new Order
        {
            UserId = model.UserId,
            AddressId = model.AddressId,
            PromotionId = model.PromotionId,
            TotalAmount = model.TotalAmount,
            OrderDate = DateTime.Now,
            Status = "Pending",
            PaymentStatus = "Pending"
        };

        _db.Orders.Add(order);
        _db.SaveChanges(); // Get the OrderId

        // Mark promotion as used
        if (model.PromotionId.HasValue && model.PromotionId > 0)
        {
            // Find the first unused user_promotion record for this user and promotion
            var userPromo = _db.UserPromotions.FirstOrDefault(up => up.UserId == model.UserId && up.PromotionId == model.PromotionId && up.IsUsed == 0);
            if (userPromo != null)
            {
                userPromo.IsUsed = 1;
                userPromo.UsedAt = DateTime.Now;
            }
        }

        foreach (var item in model.Items)
        {
            var product = _db.Stocks.FirstOrDefault(s => s.ProductName == item.ProductName);
            if (product != null)
            {
                var orderDetail = new Orderdetail
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _db.Orderdetails.Add(orderDetail);
            }
        }
        _db.SaveChanges();

        return Json(new { success = true, orderId = order.OrderId });
    }

    // ดึงข้อมูล User ปัจจุบันจาก Session
    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out int userId) && userId > 0)
        {
            return Json(new { success = true, userId = userId });
        }
        return Json(new { success = false, userId = 0 });
    }

    // หน้าชำระเงิน
    public IActionResult Payment(int orderId)
    {
        var order = _db.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return RedirectToAction("Home");

        // Generate QR payload
        var phone = "0943253900"; // ← เปลี่ยนเป็นเบอร์ร้านคุณ
        var payload = PromptPayHelper.GeneratePayload(phone, order.TotalAmount ?? 0);

        // Generate QR image เป็น base64
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10);
        var qrBase64 = Convert.ToBase64String(qrBytes);

        ViewBag.QrBase64 = qrBase64;
        ViewBag.OrderId = orderId;
        ViewBag.TotalAmount = order.TotalAmount;

        return View(order);
    }

    // อัปโหลดสลิป
    [HttpPost]
    public async Task<IActionResult> UploadSlip(int orderId, IFormFile slipImage)
    {
        try
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            if (slipImage == null || slipImage.Length == 0)
                return Json(new { success = false, message = "กรุณาเลือกไฟล์" });

            // บันทึกไฟล์
            var fileName = $"slip_{orderId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(slipImage.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "slips");
            
            // สร้างโฟลเดอร์ถ้ายังไม่มี
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await slipImage.CopyToAsync(stream);

            // Update Order
            order.SlipImagePath = "/uploads/slips/" + fileName;
            order.PaymentStatus = "PendingVerify";
            _db.SaveChanges();

            return Json(new { success = true, message = "อัปโหลดสลิปเรียบร้อย รอ Admin ยืนยัน" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
        }
    }

    // Admin ยืนยันการชำระเงิน พร้อมลด stock
    [HttpPost]
    public IActionResult ConfirmPayment(int orderId)
    {
        try
        {
            var order = _db.Orders
                .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            // ลด stock สำหรับแต่ละสินค้าในออเดอร์
            foreach (var detail in order.Orderdetails)
            {
                var product = detail.Product;
                if (product != null)
                {
                    product.Stock1 = Math.Max(0, (product.Stock1 ?? 0) - (detail.Quantity ?? 0));
                }
            }

            order.PaymentStatus = "Paid";
            order.Status = "Paid";
            _db.Update(order);
            _db.SaveChanges();

            _logger.LogInformation($"Order {orderId} payment confirmed - stock reduced");
            return Json(new { success = true, message = "ยืนยันการชำระเงินและลด Stock เรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError($"Error confirming payment for order {orderId}: {innerMsg}");
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    // ดึงรายละเอียดออเดอร์
    [HttpGet]
    public IActionResult GetOrderDetails(int orderId)
    {
        var order = _db.Orders
            .Include(o => o.User)
            .Include(o => o.Orderdetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefault(o => o.OrderId == orderId);

        if (order == null)
            return Json(new { success = false, message = "ไม่พบออเดอร์" });

        return Json(new
        {
            success = true,
            orderId = order.OrderId,
            customerName = order.User?.Username,
            customerEmail = order.User?.Email,
            customerPhone = order.User?.Phone,
            orderDate = order.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            totalAmount = order.TotalAmount,
            status = order.Status,
            paymentStatus = order.PaymentStatus,
            slipImagePath = order.SlipImagePath,
            items = order.Orderdetails.Select(d => new
            {
                productName = d.Product?.ProductName,
                quantity = d.Quantity,
                unitPrice = d.UnitPrice,
                subtotal = d.Quantity * d.UnitPrice
            }).ToList()
        });
    }

    // ยอมรับออเดอร์และเปลี่ยนเป็น Preparing
    [HttpPost]
    public IActionResult AcceptOrder(int orderId)
    {
        try
        {
            // Fetch order and include orderdetails to avoid lazy loading issues
            var order = _db.Orders
                .Include(o => o.Orderdetails)
                .FirstOrDefault(o => o.OrderId == orderId);
            
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            // Check if already processed
            if (order.Status == "Preparing" || order.Status == "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ประมวลผลแล้ว" });

            // Update status safely
            order.Status = "Preparing";
            _db.Update(order);
            _db.SaveChanges();
            
            _logger.LogInformation($"Order {orderId} accepted - status changed to Preparing");
            return Json(new { success = true, message = "ยอมรับออเดอร์เรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError($"Error accepting order {orderId}: {innerMsg}");
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    // จัดส่งออเดอร์
    [HttpPost]
    public IActionResult ShipOrder(int orderId)
    {
        try
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบ Order" });

            if (order.Status != "Preparing")
                return Json(new { success = false, message = "ออเดอร์ต้องอยู่ในสถานะ Preparing เท่านั้น" });

            order.Status = "Shipped";
            _db.Update(order);
            _db.SaveChanges();

            _logger.LogInformation($"Order {orderId} shipped - status changed to Shipped");
            return Json(new { success = true, message = "จัดส่งสินค้าเรียบร้อย" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError($"Error shipping order {orderId}: {innerMsg}");
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    [HttpPost]
    public IActionResult CompleteOrder(int orderId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdString, out var userId) || userId <= 0)
            return Json(new { success = false, message = "กรุณาเข้าสู่ระบบก่อน" });

        try
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);
            if (order == null)
                return Json(new { success = false, message = "ไม่พบออเดอร์" });

            if (order.Status != "Shipped")
                return Json(new { success = false, message = "ออเดอร์นี้ยังไม่อยู่ในสถานะจัดส่ง" });

            order.Status = "Completed";
            _db.Update(order);
            _db.SaveChanges();

            return Json(new { success = true, message = "ปิดออเดอร์เรียบร้อย ขอบคุณที่ใช้บริการ" });
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError($"Error completing order {orderId}: {innerMsg}");
            return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + innerMsg });
        }
    }

    private DeliveryTrackingViewModel BuildDeliveryTrackingViewModel(Order? order, List<Order> orders)
    {
        var history = orders.Select(o => new DeliveryOrderHistoryItem
        {
            OrderId = o.OrderId,
            OrderNumber = $"ORD-{o.OrderId}",
            OrderStatus = o.Status ?? "Pending",
            PaymentStatus = o.PaymentStatus ?? "Pending",
            CreatedAtText = o.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            TotalAmount = o.TotalAmount ?? 0,
            DeliveryAddress = string.Join(", ", new[]
            {
                o.Address?.AddressLine,
                o.Address?.District,
                o.Address?.Province,
                o.Address?.PostalCode
            }.Where(part => !string.IsNullOrWhiteSpace(part))),
            ItemSummary = string.Join(", ", o.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}")),
            IsActive = order != null && o.OrderId == order.OrderId
        }).ToList();

        if (order == null)
        {
            return new DeliveryTrackingViewModel
            {
                OrderHistory = history
            };
        }

        var normalizedStatus = order.Status ?? "Pending";
        var normalizedPaymentStatus = order.PaymentStatus ?? "Pending";
        var trackingStage = normalizedStatus switch
        {
            "Completed" => 5,
            "Shipped" => 4,
            "Preparing" => 3,
            "Paid" => 2,
            _ => 1
        };

        var statusTitle = trackingStage switch
        {
            5 => "จัดส่งสำเร็จ",
            4 => "กำลังจัดส่ง",
            3 => "กำลังเตรียมออเดอร์",
            2 => "ชำระเงินสำเร็จ",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบสลิป",
            _ => "รอชำระเงิน"
        };

        var statusMessage = trackingStage switch
        {
            5 => "ผู้ใช้ยืนยันว่าได้รับออเดอร์แล้ว รายการนี้ถูกปิดและย้ายไปอยู่ในประวัติการสั่งซื้อ",
            4 => "ร้านจัดส่งออเดอร์แล้ว สามารถติดตามจากการ์ดนี้ได้ทันที",
            3 => "แอดมินรับออเดอร์แล้ว ตอนนี้ร้านกำลังเตรียมสินค้า",
            2 => "แอดมินยืนยันสลิปแล้ว รอร้านรับออเดอร์เพื่อเริ่มเตรียมสินค้า",
            _ when normalizedPaymentStatus == "PendingVerify" => "อัปโหลดสลิปแล้ว รอแอดมินตรวจสอบการชำระเงิน",
            _ => "ระบบสร้างออเดอร์แล้ว กรุณาไปหน้า Payment เพื่อชำระเงินและอัปโหลดสลิป"
        };

        var etaText = trackingStage switch
        {
            5 => "เสร็จสิ้น",
            4 => "กำลังจัดส่ง",
            3 => "เตรียมส่งเร็ว ๆ นี้",
            2 => "รอร้านรับออเดอร์",
            _ when normalizedPaymentStatus == "PendingVerify" => "รอตรวจสอบ",
            _ => "รอชำระเงิน"
        };

        var addressParts = new[]
        {
            order.Address?.AddressLine,
            order.Address?.District,
            order.Address?.Province,
            order.Address?.PostalCode
        }
        .Where(part => !string.IsNullOrWhiteSpace(part));

        return new DeliveryTrackingViewModel
        {
            HasOrder = true,
            OrderId = order.OrderId,
            OrderNumber = $"ORD-{order.OrderId}",
            OrderStatus = normalizedStatus,
            PaymentStatus = normalizedPaymentStatus,
            StatusTitle = statusTitle,
            StatusMessage = statusMessage,
            EtaText = etaText,
            TrackingStage = trackingStage,
            CreatedAtText = order.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "-",
            TotalAmount = order.TotalAmount ?? 0,
            DeliveryAddress = string.Join(", ", addressParts),
            ItemSummary = string.Join(", ", order.Orderdetails.Select(d => $"{d.Product?.ProductName} x{d.Quantity}")),
            ShowPaymentAction = normalizedPaymentStatus != "Paid",
            PaymentUrl = Url.Action("Payment", "Account", new { orderId = order.OrderId }) ?? $"/Account/Payment?orderId={order.OrderId}",
            ShowDeliveryActions = normalizedStatus == "Shipped",
            IsCompleted = normalizedStatus == "Completed",
            OrderHistory = history
        };
    }
    

    // ─── Admin Data ───────────────────────────────────────
    public IActionResult Member()
    {
        var users = _db.Users.Include(u => u.Role).ToList();
        return View("~/Views/admin/Member.cshtml", users);
    }

    // ─── System ───────────────────────────────────────────
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
