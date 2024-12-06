using GreenGardenClient.Hubs;
using GreenGardenClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace GreenGardenClient.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IHttpClientFactory _clientFactory;// Đảm bảo bạn đã khai báo IHttpClientFactory

        private readonly IHubContext<CartHub> _hubContext;

        public ServiceController(ILogger<ServiceController> logger, IHttpClientFactory clientFactory, IHubContext<CartHub> hubContext)
        {
            _hubContext = hubContext;
            _logger = logger;
            _clientFactory = clientFactory;// Khởi tạo IHttpClientFactory
        }
        public IActionResult Index()
        {
            return View();
        }
        private async Task<T> GetDataFromApiAsync<T>(string apiUrl)
        {
            using (var client = _clientFactory.CreateClient())
            {
                var jwtToken = Request.Cookies["JWTToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return data!;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Không thể lấy dữ liệu từ API: {response.StatusCode}";
                    return default!;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderGear(int? categoryId, int? sortBy, int? priceRange, int page = 1, int pageSize = 6)
        {
            // Retrieve cart items
            var cartItems = GetCartItems();
            ViewBag.CurrentCategoryId = categoryId;

            // Build API URL with query parameters
            var queryParams = new List<string>
    {
        categoryId.HasValue ? $"categoryId={categoryId.Value}" : null,
        sortBy.HasValue ? $"sortBy={sortBy.Value}" : null,
        priceRange.HasValue ? $"priceRange={priceRange.Value}" : null,
        $"page={page}",
        $"pageSize={pageSize}"
    };
            string apiUrl = "http://103.75.186.149:5000/api/CampingGear/GetCampingGearsBySort?" +
                            string.Join("&", queryParams.Where(param => param != null));

            // Fetch data from the API
            var apiResponse = await GetDataFromApiAsync<PaginatedResponse<GearVM>>(apiUrl);

            // Fetch categories
            var campingCategories = await GetDataFromApiAsync<List<GearCategoryVM>>(
                "http://103.75.186.149:5000/api/CampingGear/GetAllCampingGearCategories");

            // Check cart usage date
            DateTime? cartUsageDate = cartItems.FirstOrDefault()?.UsageDate;
            if (cartUsageDate.HasValue)
            {
                // Format usage date for the API
                string formattedDate = cartUsageDate.Value.ToString("MM-dd-yyyy");

                // Fetch usage details by date
                var gearUsage = await GetDataFromApiAsync<List<OrderCampingGearByUsageDateDTO>>(
                    $"http://103.75.186.149:5000/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");

                if (gearUsage != null)
                {
                    // Update available quantities based on usage
                    foreach (var item in apiResponse?.Data ?? new List<GearVM>())
                    {
                        var tickets = gearUsage.Where(s => s.GearId == item.GearId).ToList();
                        foreach (var ticket in tickets)
                        {
                            item.QuantityAvailable -= ticket.Quantity.Value;
                            if (item.QuantityAvailable < 0) item.QuantityAvailable = 0;
                        }
                    }

                    // Update cart items with available quantities
                    foreach (var item in cartItems)
                    {
                        var ticket = apiResponse?.Data?.FirstOrDefault(s => s.GearId == item.Id && item.TypeCategory == "GearCategory");
                        if (ticket != null && ticket.QuantityAvailable >= item.Quantity)
                        {
                            ticket.Quantity = item.Quantity;
                            ticket.QuantityAvailable -= item.Quantity;
                        }
                    }
                }
            }

            // Update ViewBag with pagination and data
            ViewBag.CampingGears = apiResponse?.Data ?? new List<GearVM>();
            ViewBag.TotalPages = apiResponse?.TotalPages ?? 1;
            ViewBag.CurrentPage = apiResponse?.CurrentPage ?? 1;
            ViewBag.PageSize = apiResponse?.PageSize ?? pageSize;

            ViewBag.SortBy = sortBy?.ToString();
            ViewBag.PriceRange = priceRange?.ToString();
            ViewBag.CampingCategories = campingCategories;

            return View("OrderGear");
        }



        [HttpGet]
        public async Task<IActionResult> OrderFoodAndDrink(int? categoryId, int? sortBy, int? priceRange, int page = 1, int pageSize = 6)
        {
            try
            {
                // Fetch categories from API
                var foodAndDrinkCategories = await GetDataFromApiAsync<List<FoodAndDrinkCategoryVM>>(
                    "http://103.75.186.149:5000/api/FoodAndDrink/GetAllFoodAndDrinkCategories");

                // Save current category to ViewBag
                ViewBag.CurrentCategoryId = categoryId;

                // Construct API URL with filters
                var queryParams = new List<string>
        {
            categoryId.HasValue ? $"categoryId={categoryId.Value}" : null,
            sortBy.HasValue ? $"sortBy={sortBy.Value}" : null,
            priceRange.HasValue ? $"priceRange={priceRange.Value}" : null,
            $"page={page}",
            $"pageSize={pageSize}"
        };
                string apiUrl = "http://103.75.186.149:5000/api/FoodAndDrink/GetFoodAndDrinksBySort?" +
                                string.Join("&", queryParams.Where(param => param != null));

                // Fetch filtered data from API
                var apiResponse = await GetDataFromApiAsync<PaginatedResponse<FoodAndDrinkVM>>(apiUrl);

                // Update ViewBag for rendering
                ViewBag.FoodAndDrink = apiResponse?.Data ?? new List<FoodAndDrinkVM>();
                ViewBag.TotalPages = apiResponse?.TotalPages ?? 1;
                ViewBag.CurrentPage = apiResponse?.CurrentPage ?? 1;
                ViewBag.PageSize = apiResponse?.PageSize ?? pageSize;

                ViewBag.SortBy = sortBy?.ToString();

                ViewBag.PriceRange = priceRange?.ToString();
                ViewBag.FoodAndDrinkCategories = foodAndDrinkCategories;

                return View("OrderFoodAndDrink");
            }
            catch (Exception ex)
            {
                // Log the error and return a user-friendly error view or message
                ViewBag.ErrorMessage = "An error occurred while processing your request.";
                return View("Error");
            }
        }





        [HttpGet("FoodDetail")]
        public async Task<IActionResult> FoodDetail(int itemId)
        {
            var apiUrl = $"http://103.75.186.149:5000/api/FoodAndDrink/GetFoodAndDrinkDetail?itemId={itemId}";

            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var foodAndDrink = JsonConvert.DeserializeObject<FoodAndDrinkVM>(content);

                    if (foodAndDrink != null)
                    {
                        ViewBag.FoodAndDrink = foodAndDrink;
                        return View("FoodDetail", foodAndDrink); // Updated view name
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid data!";
                        return RedirectToAction("Error"); // Redirect to a dedicated error view
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode}";
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
        [HttpGet("CampingGearDetail")]
        public async Task<IActionResult> CampingGearDetail(int gearId)
        {
            // Retrieve cart items from session
            var cartItems = GetCartItems();
            var apiUrl = $"http://103.75.186.149:5000/api/CampingGear/GetCampingGearDetail?id={gearId}";

            try
            {
                // Check if there's a usage date in the cart
                DateTime? cartUsageDate = cartItems.FirstOrDefault()?.UsageDate;

                GearVM campingGear = null;

                // Fetch gear details from API
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    campingGear = JsonConvert.DeserializeObject<GearVM>(content);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode}";
                    return RedirectToAction("Error");
                }

                if (campingGear == null)
                {
                    TempData["ErrorMessage"] = "Invalid data!";
                    return RedirectToAction("Error");
                }

                // Update quantity available based on the usage date
                if (cartUsageDate.HasValue)
                {
                    // Format date for API request
                    string formattedDate = cartUsageDate.Value.ToString("MM-dd-yyyy");

                    // Fetch gear usage data from API
                    var usageResponse = await GetDataFromApiAsync<List<OrderCampingGearByUsageDateDTO>>(
                        $"http://103.75.186.149:5000/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");

                    if (usageResponse != null)
                    {
                        var gearUsage = usageResponse.Where(u => u.GearId == gearId).ToList();

                        foreach (var usage in gearUsage)
                        {
                            campingGear.QuantityAvailable -= usage.Quantity ?? 0;
                        }

                        // Ensure QuantityAvailable does not go below zero
                        if (campingGear.QuantityAvailable < 0)
                        {
                            campingGear.QuantityAvailable = 0;
                        }
                    }

                    // Adjust QuantityAvailable based on items in the cart
                    var cartItem = cartItems.FirstOrDefault(c => c.Id == gearId && c.TypeCategory == "GearCategory");
                    if (cartItem != null)
                    {
                        if (campingGear.QuantityAvailable >= cartItem.Quantity)
                        {
                            campingGear.QuantityAvailable -= cartItem.Quantity;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Not enough stock available!";
                            return RedirectToAction("Error");
                        }
                    }
                }

                // Pass the gear details to the view
                ViewBag.CampingGear = campingGear;
                return View("CampingGearDetail", campingGear);
            }
            catch (Exception ex)
            {
                // Handle any errors and redirect to an error view
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }


        [HttpGet("TicketDetail")]
        public async Task<IActionResult> TicketDetail(int ticketId)
        {
            var apiUrl = $"http://103.75.186.149:5000/api/Ticket/GetTicketDetail?id={ticketId}";

            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var ticket = JsonConvert.DeserializeObject<TicketVM>(content);

                    if (ticket != null)
                    {
                        ViewBag.Ticket = ticket;
                        return View("TicketDetail", ticket); // Updated view name
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid data!";
                        return RedirectToAction("Error"); // Redirect to a dedicated error view
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode}";
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> OrderHistory(bool? statusOrder = null, int? activityId = null)
        {
            // Retrieve the CustomerId from the session
            var customerId = HttpContext.Session.GetInt32("UserId");
            var RoleId = HttpContext.Session.GetInt32("RoleId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            // Ensure customerId is found
            if (RoleId != 3)
            {
                return RedirectToAction("Error");
            }

            ViewBag.CurrentCategoryId = activityId; // Set current category for highlighting
            ViewBag.OrderStatus = statusOrder; // Set current order status for sorting




            // Build API URL dynamically based on filters
            string apiUrl = $"http://103.75.186.149:5000/api/OrderManagement/GetCustomerOrders?customerId={customerId.Value}";
            if (activityId.HasValue)
            {
                apiUrl += $"&activityId={activityId.Value}";
            }
            if (statusOrder.HasValue)
            {
                apiUrl += $"&statusOrder={statusOrder.Value.ToString().ToLower()}";
            }

            try
            {


                // Fetch data from APIs
                var order = await GetDataFromApiAsync<List<CustomerOrderVM>>(apiUrl);
                var activity = await GetDataFromApiAsync<List<ActivityVM>>("http://103.75.186.149:5000/api/Activity/GetAllActivities");

                // Pass data to View
                ViewBag.CustomerOrder = order;
                ViewBag.Activity = activity;
                ViewBag.CustomerId = customerId;

                return View("OrderHistory");
            }
            catch (HttpRequestException httpEx)
            {
                TempData["ErrorMessage"] = $"Request error: {httpEx.Message}";
                return RedirectToAction("Error");
            }
            catch (JsonException jsonEx)
            {
                TempData["ErrorMessage"] = $"Error processing data: {jsonEx.Message}";
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
        public async Task<IActionResult> OrderDetailHistory(int orderId)
        {

            // Ensure customerId is found
            var customerId = HttpContext.Session.GetInt32("UserId");
            var RoleId = HttpContext.Session.GetInt32("RoleId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            // Ensure customerId is found
            if ( RoleId != 3)
            {
                return RedirectToAction("Error");
            }
            var apiUrl = $"http://103.75.186.149:5000/api/OrderManagement/GetCustomerOrderDetail/{orderId}";

            try
            {
                var client = _clientFactory.CreateClient();
                var jwtToken = Request.Cookies["JWTToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Retrieve JWT token from cookies              

                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var orderDetail = JsonConvert.DeserializeObject<OrderDetailVM>(content);

                    if (orderDetail != null)
                    {
                        ViewBag.OrderDetail = orderDetail;
                        return View("OrderDetailHistory", orderDetail); // Updated view name
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid data!";
                        return RedirectToAction("Error"); // Redirect to a dedicated error view
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode}";
                    return RedirectToAction("Error");
                }
            }
            catch (HttpRequestException httpEx)
            {
                TempData["ErrorMessage"] = $"Request error: {httpEx.Message}";
                return RedirectToAction("Error");
            }
            catch (JsonException jsonEx)
            {
                TempData["ErrorMessage"] = $"Error processing data: {jsonEx.Message}";
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var RoleId = HttpContext.Session.GetInt32("RoleId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            // Ensure customerId is found
            if (RoleId != 3)
            {
                return RedirectToAction("Error");
            }
            var apiUrl = $"http://103.75.186.149:5000/api/OrderManagement/ChangeCustomerActivity?orderId={orderId}";

            try
            {
                var client = _clientFactory.CreateClient();

                // Lấy JWT token từ cookies và thêm vào header
                var jwtToken = Request.Cookies["JWTToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                // Gửi yêu cầu hủy đơn hàng
                var response = await client.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Lỗi khi hủy đơn hàng: {errorMessage}" });
                }
            }
            catch (HttpRequestException httpEx)
            {
                return Json(new { success = false, message = $"Lỗi khi gửi yêu cầu: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }


        public async Task<IActionResult> OrderTicket()
        {
            var ticket = await GetDataFromApiAsync<List<TicketVM>>("http://103.75.186.149:5000/api/Ticket/GetAllCustomerTickets");
            var ticketCategory = await GetDataFromApiAsync<List<TicketCategoryVM>>("http://103.75.186.149:5000/api/Ticket/GetAllTicketCategories");

            ViewBag.Ticket = ticket;
            ViewBag.TicketCategories = ticketCategory;


            return View("OrderTicket");
        }
        [HttpGet]
        public async Task<IActionResult> OrderTicket(int? categoryId, int? sortBy, int page = 1, int pageSize = 3)
        {
            ViewBag.CurrentCategoryId = categoryId; // Save the current category for UI state

            // Build API URL with query parameters
            var queryParams = new List<string>
    {
        categoryId.HasValue ? $"categoryId={categoryId.Value}" : null,
        sortBy.HasValue ? $"sort={sortBy.Value}" : null,
        $"page={page}",
        $"pageSize={pageSize}"
    };
            string apiUrl = "http://103.75.186.149:5000/api/Ticket/GetTicketsByCategoryAndSort?" +
                            string.Join("&", queryParams.Where(param => param != null));

            // Fetch ticket data with pagination
            var apiResponse = await GetDataFromApiAsync<PaginatedResponse<TicketVM>>(apiUrl);

            // Fetch ticket categories
            var ticketCategories = await GetDataFromApiAsync<List<TicketCategoryVM>>("http://103.75.186.149:5000/api/Ticket/GetAllTicketCategories");

            // Update ViewBag with necessary data for the view
            ViewBag.Ticket = apiResponse?.Data ?? new List<TicketVM>(); // Paginated ticket list
            ViewBag.TicketCategories = ticketCategories; // Ticket categories
            ViewBag.SortBy = sortBy?.ToString() ?? "0"; // Current sort option

            // Pagination details
            ViewBag.TotalPages = apiResponse?.TotalPages ?? 1;
            ViewBag.CurrentPage = apiResponse?.CurrentPage ?? 1;
            ViewBag.PageSize = apiResponse?.PageSize ?? pageSize;

            return View("OrderTicket");
        }

        public async Task<IActionResult> ComboList()
        {
            var combo = await GetDataFromApiAsync<List<ComboVM>>("http://103.75.186.149:5000/api/Combo/GetAllCustomerCombos");

            ViewBag.Combo = combo;


            return View("ComboList");
        }
        [HttpGet("ComboDetail")]
        public async Task<IActionResult> ComboDetail(int comboId)
        {
            var apiUrl = $"http://103.75.186.149:5000/api/Combo/GetComboDetail/{comboId}";

            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var combo = JsonConvert.DeserializeObject<ComboDetailVM>(content);

                    if (combo != null)
                    {
                        ViewBag.Combo = combo;
                        return View("ComboDetail", combo); // Updated view name
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid data!";
                        return RedirectToAction("Error"); // Redirect to a dedicated error view
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode}";
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"System error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }
        public async Task<IActionResult> Cart()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");
            var RoleId = HttpContext.Session.GetInt32("RoleId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            // Ensure customerId is found
            if (RoleId != 3)
            {
                return RedirectToAction("Error");
            }
            var cartItems = GetCartItems();

            // Lấy ngày sử dụng đầu tiên
            DateTime? cartUsageDate = cartItems.FirstOrDefault()?.UsageDate;

            // Lấy danh sách dụng cụ cắm trại
            var campingGears = await GetDataFromApiAsync<List<GearVM>>(
                "http://103.75.186.149:5000/api/CampingGear/GetAllCustomerCampingGears");

            // Kiểm tra xem có sản phẩm nào là "Gear" và "GearCategory" không
            if (cartItems.Any(item => item.Type == "Gear" && item.TypeCategory == "GearCategory"))
            {
                if (cartUsageDate.HasValue)
                {
                    string formattedDate = cartUsageDate.Value.ToString("MM-dd-yyyy");
                    var gearUsage = await GetDataFromApiAsync<List<OrderCampingGearByUsageDateDTO>>(
                        $"http://103.75.186.149:5000/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");

                    if (gearUsage != null)
                    {
                        // Cập nhật số lượng khả dụng dựa trên ngày sử dụng
                        foreach (var gear in campingGears)
                        {
                            var usage = gearUsage.Where(u => u.GearId == gear.GearId);
                            if (usage != null)
                            {
                                foreach (var used in usage)
                                {
                                    gear.QuantityAvailable -= used.Quantity.Value;
                                    if (gear.QuantityAvailable < 0)
                                    {
                                        gear.QuantityAvailable = 0;
                                    }
                                }
                            }
                        }

                        // Cập nhật số lượng khả dụng trong giỏ hàng
                        foreach (var item in cartItems.Where(i => i.Type == "Gear" && i.TypeCategory == "GearCategory"))
                        {
                            var gear = campingGears.FirstOrDefault(g => g.GearId == item.Id);
                            if (gear != null)
                            {
                                item.QuantityAvailable = gear.QuantityAvailable; // Set số lượng còn lại
                                if (item.Quantity > gear.QuantityAvailable)
                                {
                                    item.Quantity = gear.QuantityAvailable; // Giới hạn số lượng
                                }
                            }
                        }
                    }
                }
            }

            SaveCartItems(cartItems);
            return View(cartItems);
        }


        private List<CartItem> GetCartItems()
        {

            var session = HttpContext.Session.GetString("Cart");
            if (session == null)
            {
                return new List<CartItem>();
            }
            return JsonConvert.DeserializeObject<List<CartItem>>(session);
        }

        // Cập nhật giỏ hàng vào Session
        private void SaveCartItems(List<CartItem> cartItems)
        {
            var session = JsonConvert.SerializeObject(cartItems);
            HttpContext.Session.SetString("Cart", session);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCartAsync(int Id, string Name, string image, string CategoryName, string Type, string TypeCategory, decimal price, int quantity, string usageDate, string redirectAction)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetInt32("RoleId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng!" });
            }

            if (userRole != 3)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập với quyền khách hàng để thêm vào giỏ hàng!" });
            }

            // Lấy danh sách sản phẩm trong giỏ hàng từ session
            var cartItems = GetCartItems();
            if (Type == "FoodAndDrink" || Type == "Gear")
            {
                // Kiểm tra giỏ hàng có vé hoặc combo chưa
                bool hasTicketOrCombo = cartItems.Any(c => c.Type == "Ticket" || c.Type == "Combo");

                if (!hasTicketOrCombo)
                {
                    return Json(new { success = false, message = "Bạn cần thêm vé hoặc combo vào giỏ hàng trước khi thêm đồ ăn hoặc dụng cụ!" });
                }
            }
            bool hasDifferentCombo = cartItems.Any(c => c.Type == "Combo" && c.TypeCategory == "ComboCategory" && c.Id != Id);
            bool hasTicket = cartItems.Any(c => c.Type == "Ticket" && c.TypeCategory == "TicketCategory");
            if (Type == "Combo")
            {
                if (hasDifferentCombo)
                {
                    return Json(new { success = false, message = "Giỏ hàng chỉ cho phép một loại combo duy nhất." });
                }
                else if (hasTicket)
                {
                    return Json(new { success = false, message = "Không thể thêm combo vào giỏ hàng vì đã có vé trong giỏ hàng." });
                }


            }

            // Nếu đang thêm một vé, không giới hạn loại vé khác nhau
            if (Type == "Ticket")
            {
                bool hasCombo = cartItems.Any(c => c.Type == "Combo" && c.TypeCategory == "ComboCategory");
                if (hasCombo)
                {
                    return Json(new { success = false, message = "Không thể thêm vé vào giỏ hàng vì đã có combo trong giỏ hàng." });
                }
            }
            // Kiểm tra nếu giỏ hàng chứa vé hoặc combo


            // Chuyển đổi usageDate từ chuỗi sang DateTime
            DateTime parsedDateTime;
            if (!DateTime.TryParseExact(usageDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                parsedDateTime = DateTime.Now; // Giá trị mặc định nếu chuyển đổi thất bại
            }

            // Kiểm tra nếu sản phẩm đã tồn tại trong giỏ hàng
            var existingItem = cartItems.FirstOrDefault(c => c.Id == Id && c.Type == Type && c.TypeCategory == TypeCategory);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity; // Cập nhật số lượng nếu sản phẩm đã tồn tại
            }
            else
            {
                // Thêm sản phẩm mới vào giỏ hàng
                var newItem = new CartItem
                {
                    Id = Id,
                    Name = Name,
                    CategoryName = CategoryName,
                    Type = Type,
                    Image = image,
                    TypeCategory = TypeCategory,
                    Price = price,
                    Quantity = quantity,
                    UsageDate = parsedDateTime
                };
                cartItems.Add(newItem);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveCartUpdate", cartItems);

            // Lưu giỏ hàng cập nhật vào session
            SaveCartItems(cartItems);
            int cartItemCount = cartItems.Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
            // Trả về JSON cho AJAX với thông báo thành công
            return Json(new { success = true, message = "Thêm vào giỏ hàng thành công!", cartItemCount = cartItems.Count });
        }
        [HttpPost]
        public IActionResult AddDateToCart(string usageDate)
        {
            var cartItems = GetCartItems();
            DateTime parsedDate;

            // Try to parse only the date, ignoring time (ensure format is yyyy-MM-dd)
            if (!DateTime.TryParseExact(usageDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                // If parsing fails, use the current date as a default
                parsedDate = DateTime.Now.Date; // Ensure we only get the date part
            }

            // Create a new cart item with only the date
            var newItem = new CartItem
            {
                UsageDate = parsedDate
            };

            // Add the new item to the cart
            cartItems.Add(newItem);

            // Save the updated cart items
            SaveCartItems(cartItems);

            // Redirect to the "OrderTicket" page or another page as necessary
            return RedirectToAction("OrderTicket");
        }
        [HttpGet]
        public IActionResult GetCartItemCount()
        {
            var cart = GetCartItems();
            int itemCount = cart.Sum(c => c.Quantity);
            return Json(itemCount);
        }

        [HttpPost]
        public IActionResult Reset()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            // Ensure customerId is found
            if (!customerId.HasValue)
            {
                return RedirectToAction("Error");
            }
            // Xóa toàn bộ sản phẩm trong giỏ hàng
            HttpContext.Session.Remove("Cart"); // Nếu giỏ hàng lưu trong Session
            HttpContext.Session.SetInt32("CartItemCount", 0);
            return RedirectToAction("OrderTicket"); // "Booking" là tên controller, "Index" là action.
        }


        [HttpPost]
        public IActionResult UpdateCartUsageDate(string usageDate)
        {
            var cartItems = GetCartItems(); // Lấy danh sách giỏ hàng từ session

            // Chuyển đổi ngày sử dụng thành DateTime
            if (DateTime.TryParseExact(usageDate, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                // Cập nhật ngày sử dụng cho từng mặt hàng trong giỏ hàng
                foreach (var item in cartItems)
                {
                    item.UsageDate = parsedDateTime; // Cập nhật ngày sử dụng
                }

                SaveCartItems(cartItems); // Lưu giỏ hàng vào session
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Ngày sử dụng không hợp lệ." });
        }




        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int ticketId)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(c => c.Id == ticketId);
            if (item != null)
            {
                cartItems.Remove(item);
            }
            bool hasTicketOrCombo = cartItems.Any(c => c.Type == "Ticket" || c.Type == "Combo");

            // If no "ticket" or "combo" items exist, clear the cart and notify the frontend
            if (!hasTicketOrCombo)
            {
                cartItems.Clear(); // Clear all items
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.SetInt32("CartItemCount", 0);// Clear the cart from the session 
                SaveCartItems(cartItems);
                return Json(new { isCartCleared = true }); // Indicate cart has been cleared
            }
            SaveCartItems(cartItems);
            int cartItemCount = cartItems.Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public IActionResult UpdateCartItemQuantity(int ticketId, int newQuantity, string type, string typeCategory)
        {
            // Retrieve the cart items
            var cartItems = GetCartItems();

            // Find the specific item in the cart based on the composite key
            var item = cartItems.FirstOrDefault(c => c.Id == ticketId && c.Type == type && c.TypeCategory == typeCategory);

            if (item != null)
            {
                // Update the quantity
                item.Quantity = newQuantity;
            }
            else
            {
                // If no item matches, consider adding it as a new entry if desired
            }
            int cartItemCount = cartItems.Sum(c => c.Quantity);
            HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
            // Save the updated cart items
            SaveCartItems(cartItems);

            return Ok();
        }



        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Order()
        {
            // Check if the user is logged in
            var customerId = HttpContext.Session.GetInt32("UserId");
            if (customerId == null)
            {
                TempData["Notification"] = "Vui lòng đăng nhập để thực hiện đặt hàng!";
                return RedirectToAction("Login", "Common"); // Redirect to login if not logged in
            }

            // Retrieve cart items from session
            var cartItems = GetCartItems();

            // Ensure there's at least one item in the cart
            if (!cartItems.Any())
            {
                TempData["Notification"] = "Giỏ hàng của bạn trống!";
                return RedirectToAction("Cart");
            }

            // Check for tickets or combos in the cart
            var tickets = cartItems.Where(c => c.Type == "Ticket" && c.TypeCategory == "TicketCategory").ToList();
            var combos = cartItems.Where(c => c.Type == "Combo" && c.TypeCategory == "ComboCategory").ToList();

            // If there is no ticket and no combo, notify the user and redirect to the cart
            if (!tickets.Any() && !combos.Any())
            {
                TempData["Notification"] = "Giỏ hàng của bạn cần có ít nhất một vé hoặc combo để đặt hàng!";
                return RedirectToAction("Cart");
            }

            try
            {
                // Create a new HttpClient instance from the factory
                var client = _clientFactory.CreateClient();
                var jwtToken = Request.Cookies["JWTToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Classify other cart items
                var gears = cartItems.Where(c => c.Type == "Gear" && c.TypeCategory == "GearCategory").ToList();
                var foods = cartItems.Where(c => c.Type == "FoodAndDrink" && c.TypeCategory == "FoodAndDrinkCategory").ToList();
                var comboFoods = cartItems.Where(c => c.TypeCategory == "Combo").ToList();

                // Assuming all cart items share the same usage date
                var usageDate = cartItems.First().UsageDate;

                // Calculate total amount
                var totalAmount = cartItems.Sum(item => item.TotalPrice);

                // Prepare order request based on the types of items in the cart
                if (tickets.Any())
                {
                    var orderRequest = new CheckOut
                    {
                        Order = new CustomerOrderAddDTO
                        {
                            CustomerId = customerId.Value,
                            CustomerName = HttpContext.Session.GetString("Fullname"),
                            OrderDate = DateTime.Now,
                            OrderUsageDate = usageDate,
                            Deposit = 0,
                            TotalAmount = totalAmount,
                            PhoneCustomer = HttpContext.Session.GetString("NumberPhone")
                        },
                        OrderTicket = tickets.Select(t => new CustomerOrderTicketAddlDTO
                        {
                            TicketId = t.Id,
                            Quantity = t.Quantity,
                            ImgUrl = t.Image,
                        }).ToList(),
                        OrderCampingGear = gears.Select(g => new CustomerOrderCampingGearAddDTO
                        {
                            GearId = g.Id,
                            Quantity = g.Quantity,
                            ImgUrl = g.Image,
                        }).ToList(),
                        OrderFood = foods.Select(f => new CustomerOrderFoodAddDTO
                        {
                            ItemId = f.Id,
                            Quantity = f.Quantity,
                            Description = f.Name,
                            ImgUrl = f.Image,
                        }).ToList(),
                        OrderFoodCombo = comboFoods.Select(cf => new CustomerOrderFoodComboAddDTO
                        {
                            ComboId = cf.Id,
                            Quantity = cf.Quantity,
                            ImgUrl = cf.Image,
                        }).ToList()
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");

                    // Use the client from IHttpClientFactory to make the API call
                    var apiUrl = "http://103.75.186.149:5000/api/OrderManagement/CheckOut";
                    var response = await client.PostAsync(apiUrl, content);

                    // Handle the API response
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContext.Session.Remove("Cart");
                        HttpContext.Session.SetInt32("CartItemCount", 0);
                        TempData["Notification"] = "Đặt hàng thành công!";
                        return RedirectToAction("OrderHistory");
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["Notification"] = $"Lỗi khi đặt hàng: {errorMessage}";
                        return RedirectToAction("Cart");
                    }
                }
                else
                {
                    var orderRequest = new CheckOutComboOrderRequest
                    {
                        Order = new CustomerOrderAddDTO
                        {
                            CustomerId = customerId.Value,
                            CustomerName = HttpContext.Session.GetString("Fullname"),
                            OrderDate = DateTime.Now,
                            OrderUsageDate = usageDate,
                            Deposit = 0,
                            TotalAmount = totalAmount,
                            PhoneCustomer = HttpContext.Session.GetString("NumberPhone")
                        },
                        OrderCombo = combos.Select(t => new CustomerOrderComboAddDTO
                        {
                            ComboId = t.Id,
                            Quantity = t.Quantity,
                            ImgUrl = t.Image
                        }).ToList(),
                        OrderCampingGear = gears.Select(g => new CustomerOrderCampingGearAddDTO
                        {
                            GearId = g.Id,
                            Quantity = g.Quantity,
                            ImgUrl = g.Image
                        }).ToList(),
                        OrderFood = foods.Select(f => new CustomerOrderFoodAddDTO
                        {
                            ItemId = f.Id,
                            Quantity = f.Quantity,
                            Description = f.Name,
                            ImgUrl = f.Image
                        }).ToList(),
                        OrderFoodCombo = comboFoods.Select(cf => new CustomerOrderFoodComboAddDTO
                        {
                            ComboId = cf.Id,
                            Quantity = cf.Quantity,
                            ImgUrl = cf.Image
                        }).ToList()
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");

                    // Use the client from IHttpClientFactory to make the API call
                    var apiUrl = "http://103.75.186.149:5000/api/OrderManagement/CheckOutComboOrder";
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        HttpContext.Session.Remove("Cart");
                        HttpContext.Session.SetInt32("CartItemCount", 0);
                        TempData["Notification"] = "Đặt hàng thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["Notification"] = $"Lỗi khi đặt hàng: {errorMessage}";
                        return RedirectToAction("Cart");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Notification"] = "Xảy ra lỗi ngoài luồng khi tạo đơn.";
                return RedirectToAction("Error");
            }
        }



    }
}