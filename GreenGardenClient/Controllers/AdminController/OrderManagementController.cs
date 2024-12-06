using GreenGardenClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace GreenGardenClient.Controllers.AdminController
{
    public class OrderManagementController : Controller
    {
        private HttpClient _httpClient;

        public OrderManagementController()
        {
            _httpClient = new HttpClient();

        }
        private T GetDataFromApi<T>(string url)
        {

            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<T>().Result;
        }




        public IActionResult Error()
        {
            return View("Error");

        }
        public IActionResult Previous()
        {
            if (HttpContext.Session.GetString("ComboCart") != null)
            {
                return RedirectToAction("OrderCombo");
            }
            else
            {
                return RedirectToAction("OrderTicket");
            }


        }
        public IActionResult OrderOnline()
        {

            try
            {

                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                List<OrderVM> orderdata = new List<OrderVM>();
                List<ActivityVM> activities = new List<ActivityVM>();
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    if (HttpContext.Session.GetInt32("RoleId").Value == 1)
                    {
                        orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrderOnline")
                                            .Where(s => s.ActivityId == 1).OrderByDescending(s => s.OrderDate)
                                            .ToList();
                    }
                    else if (HttpContext.Session.GetInt32("RoleId").Value == 2)
                    {
                        orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrderOnline")
                    .Where(s => s.ActivityId == 1 && s.StatusOrder == true).OrderByDescending(s => s.OrderDate)
                    .ToList();
                    }
                    activities = GetDataFromApi<List<ActivityVM>>("http://103.20.97.182:5124/api/Activity/GetAllActivities")
                        .Where(s => s.ActivityId == 1 || s.ActivityId == 2 || s.ActivityId == 1002)
                        .ToList();

                    // Duyệt qua bản sao của orderdata
                    foreach (var item in orderdata.ToList())
                    {
                        if (item != null && item.OrderUsageDate.HasValue && item.OrderUsageDate.Value.Date < DateTime.Now.Date && item.StatusOrder == false)
                        {
                            string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{item.OrderId}/{1002}";
                            HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                            item.ActivityId = 1002;
                            TempData["NotificationError"] += $"Đơn {item.OrderId} đã bị hủy do tới ngày đặt trước nhưng chưa cọc.\n";
                            orderdata.Remove(item);
                            return RedirectToAction("OrderOnline");

                            // Xóa item khỏi danh sách gốc
                        }
                        else if (item != null && item.OrderUsageDate.HasValue && item.OrderUsageDate.Value.Date < DateTime.Now.Date && item.StatusOrder == true)
                        {
                            string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{item.OrderId}/{1002}";
                            HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                            item.ActivityId = 1002;
                            TempData["NotificationError"] += $"Đơn {item.OrderId} đã bị hủy do quá ngày sử dụng.\n";
                            orderdata.Remove(item);
                            return RedirectToAction("OrderOnline");
                            // Xóa item khỏi danh sách gốc
                        }
                    }

                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
                ViewBag.dataorder = orderdata;

                ViewBag.activities = activities;
                if (TempData["NotificationSuccess"] != null)
                {
                    ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                }
                if (TempData["NotificationError"] != null)
                {
                    ViewBag.NotificationSuccess = TempData["NotificationError"];
                }
                return View("OrderOnline");

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }
        public IActionResult OrderToday()
        {

            try
            {

                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                List<OrderVM> orderdata = new List<OrderVM>();
                List<ActivityVM> activities = new List<ActivityVM>();
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    if (HttpContext.Session.GetInt32("RoleId").Value == 1)
                    {
                        orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrderOnline")
                    .Where(s => s.ActivityId == 1 && s.OrderUsageDate.Value.ToString("yyyy/MM/dd").Equals(DateTime.Now.ToString("yyyy/MM/dd"))).OrderByDescending(s => s.OrderDate)
                    .ToList();
                    }
                    else if (HttpContext.Session.GetInt32("RoleId").Value == 2)
                    {
                        orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrderOnline")
                    .Where(s => s.ActivityId == 1 && s.StatusOrder == true && s.OrderUsageDate.Value.ToString("yyyy/MM/dd").Equals(DateTime.Now.ToString("yyyy/MM/dd"))).OrderByDescending(s => s.OrderDate)
                    .ToList();
                    }

                    activities = GetDataFromApi<List<ActivityVM>>("http://103.20.97.182:5124/api/Activity/GetAllActivities")
                        .Where(s => s.ActivityId == 1 || s.ActivityId == 2 || s.ActivityId == 1002)
                        .ToList();

                    // Duyệt qua bản sao của orderdata
                    foreach (var item in orderdata.ToList())
                    {
                        if (item != null && item.OrderUsageDate.HasValue && item.OrderUsageDate.Value.Date < DateTime.Now.Date && item.StatusOrder == false)
                        {
                            string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{item.OrderId}/{1002}";
                            HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                            item.ActivityId = 1002;
                            TempData["NotificationError"] += $"Đơn {item.OrderId} đã bị hủy do tới ngày đặt trước nhưng chưa cọc.\n";
                            orderdata.Remove(item);
                            return RedirectToAction("OrderCancel");

                            // Xóa item khỏi danh sách gốc
                        }
                        else if (item != null && item.OrderUsageDate.HasValue && item.OrderUsageDate.Value.Date < DateTime.Now.Date && item.StatusOrder == true)
                        {
                            string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{item.OrderId}/{1002}";
                            HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                            item.ActivityId = 1002;
                            TempData["NotificationError"] += $"Đơn {item.OrderId} đã bị hủy do quá ngày sử dụng.\n";
                            orderdata.Remove(item);
                            return RedirectToAction("OrderCancel");
                            // Xóa item khỏi danh sách gốc
                        }
                    }



                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
                ViewBag.dataorder = orderdata;

                ViewBag.activities = activities;
                if (TempData["NotificationSuccess"] != null)
                {
                    ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                }
                if (TempData["NotificationError"] != null)
                {
                    ViewBag.NotificationSuccess = TempData["NotificationError"];
                }
                return View("OrderToday");

            }
            catch
            {
                return RedirectToAction("Error");

            }


        }
        public IActionResult OrderUsing()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    List<OrderVM> orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrders");
                    orderdata = orderdata.Where(s => s.ActivityId == 2).OrderByDescending(s => s.OrderUsageDate).ToList();
                    List<ActivityVM> activities = GetDataFromApi<List<ActivityVM>>("http://103.20.97.182:5124/api/Activity/GetAllActivities");
                    activities = activities.Where(s => s.ActivityId == 2 || s.ActivityId == 3).ToList();

                    ViewBag.dataorder = orderdata;

                    ViewBag.activities = activities;
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }

                    return View("OrderUsing");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }


            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }


        }
        public IActionResult OrderCheckout()
        {
            try
            {


                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    List<OrderVM> orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrders");
                    orderdata = orderdata
                        .Where(s => s.ActivityId == 3 && s.OrderCheckoutDate != null).OrderByDescending(s => s.OrderCheckoutDate)
                        .ToList();


                    ViewBag.dataorder = orderdata;
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }

                    return View("OrderCheckOut");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }


        }

        public IActionResult OrderCancel()
        {
            try
            {


                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    List<OrderVM> orderdata = GetDataFromApi<List<OrderVM>>("http://103.20.97.182:5124/api/OrderManagement/GetAllOrders");
                    orderdata = orderdata.Where(s => s.ActivityId == 1002).OrderByDescending(s => s.OrderUsageDate).ToList();

                    ViewBag.dataorder = orderdata;

                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }
                    return View("OrderCancel");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }

        }


        [HttpPost]
        public async Task<IActionResult> UpdateActivityOrder(int idorder, int idactivity)
        {

            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order");
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateCart") ?? new List<OrderCampingGearDetailDTO>();

            var jwtToken = Request.Cookies["JWTToken"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            if (idactivity == 2)
            {
                if (order.OrderUsageDate.Value.ToString("dd/MM/yyyy").Equals(DateTime.Now.ToString("dd/MM/yyyy")) || ticketscart.IsNullOrEmpty())
                {

                    UpdateOrderDTO orderupdate = new UpdateOrderDTO()
                    {
                        OrderId = order.OrderId,
                        OrderUsageDate = DateTime.Now,
                        TotalAmount = order.TotalAmount,
                    };
                    var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
                    // Serialize request objects to JSON
                    var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");



                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        TempData["NotificationSuccess"] = $"Đơn #{idorder} đã chuyển sang trạng thái đang sử dụng.";
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);


                        string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{idorder}/{idactivity}";

                        HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                        return RedirectToAction("OrderUsing");
                    }
                    else
                    {
                        TempData["NotificationError"] = $"Đơn #{idorder} chuyển trang thái không thành công.";
                        return RedirectToAction("OrderDetail", new { id = order.OrderId });
                    }
                }
                else
                {

                    OrderDetailVM orderdata = GetDataFromApi<OrderDetailVM>($"http://103.20.97.182:5124/api/OrderManagement/GetOrderDetail/{idorder}");
                    string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");

                    List<GearVM> tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears");
                    List<OrderCampingGearByUsageDateDTO> ordergear = GetDataFromApi<List<OrderCampingGearByUsageDateDTO>>($"http://103.20.97.182:5124/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");
                    foreach (var item in tickets)
                    {
                        var ticket = ordergear.ToList().Where(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            foreach (var item1 in ticket)
                            {
                                item.QuantityAvailable = item.QuantityAvailable - item1.Quantity.Value;
                            }
                        }
                    }
                    foreach (var item in orderdata.OrderCampingGearDetails)
                    {
                        var ticket = tickets.FirstOrDefault(s => item.GearId == s.GearId);
                        item.QuantityAvaiable = ticket.QuantityAvailable;
                    }
                    UpdateOrderDTO updateorder = new UpdateOrderDTO()
                    {
                        OrderId = orderdata.OrderId,
                        OrderUsageDate = DateTime.Now,
                        TotalAmount = orderdata.TotalAmount,
                    };
                    HttpContext.Session.SetObjectAsJson("order2", updateorder);
                    HttpContext.Session.SetObjectAsJson("GearUpdateDateCart", orderdata.OrderCampingGearDetails);
                    return RedirectToAction("OrderDetail", new { id = order.OrderId });
                }


            }
            else if (idactivity == 3)
            {


                UpdateOrderDTO orderupdate = new UpdateOrderDTO()
                {
                    OrderId = order.OrderId,
                    OrderUsageDate = order.OrderUsageDate,
                    TotalAmount = order.TotalAmount,
                    OrderCheckoutDate = order.OrderCheckoutDate,
                };
                var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
                // Serialize request objects to JSON
                var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");


                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                if (response1.IsSuccessStatusCode)
                {
                    TempData["NotificationSuccess"] = $"Đơn #{idorder} đã thanh toán thành công.";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);


                    string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{idorder}/{idactivity}";

                    HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                    return RedirectToAction("OrderCheckOut");
                }
                else
                {
                    TempData["NotificationError"] = $"Đơn #{idorder} đã thanh toán thất bại.";
                    return RedirectToAction("OrderDetail", new { id = order.OrderId });
                }



            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{idorder}/{idactivity}";

                HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;
                TempData["NotificationError"] = $"Đơn #{idorder} đã hủy thành công.";

                return RedirectToAction("OrderCancel");

            }



        }
        [HttpPost]
        public IActionResult EnterDeposit(int idorder, double money, int page)
        {
            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/EnterDeposit/{idorder}/{money}";
                TempData["NotificationSuccess"] = "Đặt cọc thành công!";

                // Send the PUT request
                HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;

            }
            catch (Exception ex)
            {
                TempData["NotificationError"] = "Xảy ra lỗi trong quá trình cọc!";

            }
            if (page == 1)
            {
                return RedirectToAction("OrderToday");

            }
            else
            {
                return RedirectToAction("OrderOnline");

            }

        }
        [HttpPost]
        public IActionResult CancelDeposit(int idorder, int page)
        {
            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Construct the API URL with the parameters
                string apiUrl = $"http://103.20.97.182:5124/api/OrderManagement/CancelDeposit/{idorder}";
                TempData["NotificationSuccess"] = "Hủy đặt cọc thành công!";

                // Send the PUT request
                HttpResponseMessage response = _httpClient.PutAsync(apiUrl, null).Result;

            }
            catch (Exception ex)
            {
                TempData["NotificationError"] = "Xảy ra lỗi trong quá trình hủy cọc!";

            }
            if (page == 1)
            {
                return RedirectToAction("OrderToday");

            }
            else
            {
                return RedirectToAction("OrderOnline");

            }

        }
        public IActionResult CreateOrder()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
                    if (orders.OrderUsageDate != null)
                    {
                        string date = orders.OrderUsageDate.Value.ToString("yyyy-MM-ddTHH:mm");
                        ViewBag.date = date;

                    }
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationError = TempData["NotificationError"];
                    }
                    return View(orders);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }


        }
        [HttpPost]
        public IActionResult CreateOrder(string name, DateTime usagedate, string phone)
        {
            var cart = new OrderVM()
            {
                CustomerName = name,
                OrderUsageDate = usagedate,
                PhoneCustomer = phone
            };

            TempData["NotificationSuccess"] = "Lưu thành công thông tin khách hàng!";
            HttpContext.Session.SetObjectAsJson("OrderCart", cart);
            return RedirectToAction("CreateOrder");


        }
        public IActionResult OrderTicket()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
                    var combo = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("ComboCart") ?? new List<TicketVM>();

                    if (orders.CustomerName.IsNullOrEmpty() || orders.OrderUsageDate == null || orders.PhoneCustomer.IsNullOrEmpty())
                    {
                        TempData["NotificationError"] = "Bạn phải điền đầy đủ thông tin bao gồm tên,số điện thoại và ngày sử dụng!";
                        return RedirectToAction("CreateOrder");

                    }
                    else
                    {
                        if (!combo.IsNullOrEmpty())
                        {
                            TempData["NotificationError"] = "Bạn đang đặt combo vui lòng không đặt thêm vé!";

                            return RedirectToAction("CreateOrder");
                        }
                        else
                        {
                            var ticketscart = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();
                            List<TicketVM> tickets = GetDataFromApi<List<TicketVM>>("http://103.20.97.182:5124/api/Ticket/GetAllTickets");

                            foreach (var item in ticketscart)
                            {
                                var ticket = tickets.ToList().FirstOrDefault(s => s.TicketId == item.TicketId);
                                if (ticket != null)
                                {
                                    ticket.Quantity = item.Quantity;
                                }
                            }
                            if (TempData["NotificationSuccess"] != null)
                            {
                                ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                            }
                            if (TempData["NotificationError"] != null)
                            {
                                ViewBag.NotificationSuccess = TempData["NotificationError"];
                            }
                            ViewBag.tickets = tickets;
                            return View("OrderTicket");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }



        }
        [HttpPost]
        public IActionResult TicketDetailCart(int id, string name, decimal price, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();

            if (quantity > 0)
            {
                TempData["NotificationSuccess"] = "Đã thêm sản phẩm thành công";
                var item = cart.FirstOrDefault(t => t.TicketId == id);

                if (item != null)
                {


                    item.Quantity = quantity;
                }
                else
                {


                    cart.Add(new TicketVM() { TicketId = id, TicketName = name, Price = price, Quantity = quantity });

                }
            }
            else
            {
                var item = cart.FirstOrDefault(t => t.TicketId == id);
                if (item != null)
                {
                    TempData["NotificationError"] = "Đã bỏ sản phẩm khỏi đơn.";


                    cart.Remove(item);
                }

            }

            HttpContext.Session.SetObjectAsJson("TicketCart", cart);
            return RedirectToAction("OrderTicket");

        }
        public IActionResult TicketCart(List<int> TicketIds, List<string> Name, List<decimal> Prices, List<int> Quantities)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();


            for (int i = 0; i < TicketIds.Count; i++)
            {

                if (Quantities[i] > 0)
                {
                    var item = cart.FirstOrDefault(t => t.TicketId == TicketIds[i]);
                    if (item != null)
                    {
                        item.Quantity = Quantities[i];
                    }
                    else
                    {
                        cart.Add(new TicketVM() { TicketId = TicketIds[i], TicketName = Name[i], Price = Prices[i], Quantity = Quantities[i] });

                    }
                }
                else
                {
                    var item = cart.FirstOrDefault(t => t.TicketId == TicketIds[i]);
                    if (item != null)
                    {
                        cart.Remove(item);

                    }

                }


            }
            TempData["NotificationSuccess"] = "Thêm vào giỏ hàng thành công! Hãy tiếp tục đặt đồ cắm trại nào.";
            HttpContext.Session.SetObjectAsJson("TicketCart", cart);
            return RedirectToAction("OrderTicket");
        }
        public IActionResult OrderGear()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<GearVM>>("GearCart") ?? new List<GearVM>();
                    var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
                    if (orders.OrderUsageDate == null)
                    {
                        TempData["NotificationError"] = "Muốn thuê đồ phải đặt ngày";

                        return RedirectToAction("CreateOrder");
                    }
                    else
                    {
                        string formattedDate = orders.OrderUsageDate.Value.ToString("yyyy-MM-dd");

                        List<GearVM> tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears");
                        var jwtToken = Request.Cookies["JWTToken"];
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        List<OrderCampingGearByUsageDateDTO> ordergear = GetDataFromApi<List<OrderCampingGearByUsageDateDTO>>($"http://103.20.97.182:5124/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");
                        foreach (var item in tickets)
                        {
                            var ticket = ordergear.ToList().Where(s => s.GearId == item.GearId);
                            if (ticket != null)
                            {
                                foreach (var item1 in ticket)
                                {
                                    item.QuantityAvailable = item.QuantityAvailable - item1.Quantity.Value;
                                }
                            }
                        }

                        foreach (var item in ticketscart)
                        {
                            var ticket = tickets.ToList().FirstOrDefault(s => s.GearId == item.GearId);
                            if (ticket != null)
                            {
                                ticket.Quantity = item.Quantity;
                                ticket.QuantityAvailable -= item.Quantity;
                            }
                        }

                        if (TempData["NotificationSuccess"] != null)
                        {
                            ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                        }
                        if (TempData["NotificationError"] != null)
                        {
                            ViewBag.NotificationError = TempData["NotificationError"];
                        }
                        ViewBag.gears = tickets;
                        return View("OrderGear");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }

        }
        public IActionResult GearCart(List<int> id, List<string> name, List<decimal> price, List<int> quantity, List<int> quantityavai)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GearVM>>("GearCart") ?? new List<GearVM>();
            var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();


            for (int i = 0; i < id.Count; i++)
            {

                if (quantity[i] > 0)
                {
                    var item = cart.FirstOrDefault(t => t.GearId == id[i]);

                    if (item != null)
                    {
                        item.Quantity = quantity[i];
                    }
                    else
                    {
                        cart.Add(new GearVM() { GearId = id[i], GearName = name[i], RentalPrice = price[i], Quantity = quantity[i], QuantityAvailable = quantityavai[i] });

                    }
                }
                else
                {
                    var item = cart.FirstOrDefault(t => t.GearId == id[i]);
                    if (item != null)
                    {
                        cart.Remove(item);
                    }

                }



            }

            HttpContext.Session.SetObjectAsJson("GearCart", cart);
            TempData["NotificationSuccess"] = "Đã thêm sàn phẩm vào đơn!";

            return RedirectToAction("OrderGear");
        }
        public IActionResult OrderFood()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();

                    List<FoodAndDrinkVM> foodAndDrinks = GetDataFromApi<List<FoodAndDrinkVM>>("http://103.20.97.182:5124/api/FoodAndDrink/GetAllFoodAndDrink");

                    foreach (var item in ticketscart)
                    {
                        var ticket = foodAndDrinks.ToList().FirstOrDefault(s => s.ItemId == item.ItemId);
                        if (ticket != null)
                        {
                            ticket.Description = item.Description;
                            ticket.Quantity = item.Quantity;
                        }
                    }

                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationError = TempData["NotificationError"];
                    }
                    ViewBag.gears = foodAndDrinks;
                    return View("OrderFood");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }

        }
        public IActionResult FoodDetail(int id, int quantity)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var cart = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();

                    FoodAndDrinkVM foodAndDrinks = GetDataFromApi<List<FoodAndDrinkVM>>("http://103.20.97.182:5124/api/FoodAndDrink/GetAllFoodAndDrink").ToList().FirstOrDefault(f => f.ItemId == id);
                    if (cart.FirstOrDefault(s => s.ItemId == id) == null)
                    {
                        foodAndDrinks.Quantity = 0;
                    }
                    else
                    {
                        foodAndDrinks.Quantity = cart.FirstOrDefault(s => s.ItemId == id).Quantity;

                    }
                    return View("FoodDetail", foodAndDrinks);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }



        }
        public IActionResult TicketDetail(int id, int quantity)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var cart = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();


                    TicketVM foodAndDrinks = GetDataFromApi<List<TicketVM>>("http://103.20.97.182:5124/api/Ticket/GetAllTickets").ToList().FirstOrDefault(f => f.TicketId == id);
                    if (cart.FirstOrDefault(s => s.TicketId == id) == null)
                    {
                        foodAndDrinks.Quantity = 0;
                    }
                    else
                    {
                        foodAndDrinks.Quantity = cart.FirstOrDefault(s => s.TicketId == id).Quantity;


                    }
                    return View("TicketDetail", foodAndDrinks);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }


        }
        public IActionResult ComboDetail(int id, int quantity)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var cart = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();

                    ComboDetailVM foodAndDrinks = GetDataFromApi<ComboDetailVM>($"http://103.20.97.182:5124/api/Combo/GetComboDetail/{id}");

                    if (cart.FirstOrDefault(s => s.ComboId == id) == null)
                    {
                        foodAndDrinks.Quantity = 0;
                    }
                    else
                    {
                        foodAndDrinks.Quantity = cart.FirstOrDefault(s => s.ComboId == id).Quantity;


                    }
                    return View("ComboDetail", foodAndDrinks);

                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }
        }
        public IActionResult ComboFoodDetail(int id, int quantity)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var cart = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();


                    ComboFoodDetailVM foodAndDrinks = GetDataFromApi<ComboFoodDetailVM>($"http://103.20.97.182:5124/api/ComboFood/GetComboFoodDetail/{id}");
                    if (cart.FirstOrDefault(s => s.ComboId == id) == null)
                    {
                        foodAndDrinks.Quantity = 0;
                    }
                    else
                    {
                        foodAndDrinks.Quantity = cart.FirstOrDefault(s => s.ComboId == id).Quantity;


                    }
                    return View("ComboFoodDetail", foodAndDrinks);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }

        }
        [HttpPost]
        public IActionResult ComboFoodDetailCart(int id, string name, decimal price, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();



            if (quantity > 0)
            {
                TempData["NotificationSuccess"] = "Đã thêm sản phẩm vào đơn!";

                var item = cart.FirstOrDefault(t => t.ComboId == id);

                if (item != null)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Add(new ComboFoodVM() { ComboId = id, ComboName = name, Price = price, Quantity = quantity });

                }
            }
            else
            {
                TempData["NotificationError"] = "Đã bỏ sản phẩm khỏi đơn!";

                var item = cart.FirstOrDefault(t => t.ComboId == id);
                if (item != null)
                {
                    cart.Remove(item);
                }

            }





            HttpContext.Session.SetObjectAsJson("ComboFoodCart", cart);
            return RedirectToAction("OrderComboFood");
        }


        [HttpPost]
        public IActionResult ComboDetailCart(int id, string name, decimal price, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();



            if (quantity > 0)
            {
                TempData["NotificationSuccess"] = "Đã thêm sản phẩm vào đơn!";

                var item = cart.FirstOrDefault(t => t.ComboId == id);

                if (item != null)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Add(new ComboVM() { ComboId = id, ComboName = name, Price = price, Quantity = quantity });

                }
            }
            else
            {
                TempData["NotificationError"] = "Đã bỏ sản phẩm khỏi đơn!";

                var item = cart.FirstOrDefault(t => t.ComboId == id);
                if (item != null)
                {
                    cart.Remove(item);
                }

            }





            HttpContext.Session.SetObjectAsJson("ComboCart", cart);
            return RedirectToAction("OrderCombo");
        }
        [HttpPost]
        public IActionResult FoodDetailCart(int id, string name, decimal price, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();

            if (quantity > 0)
            {
                TempData["NotificationSuccess"] = "Đã thêm sản phẩm vào đơn!";

                var item = cart.FirstOrDefault(t => t.ItemId == id);

                if (item != null)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Add(new FoodAndDrinkVM() { ItemId = id, ItemName = name, Price = price, Description = "", Quantity = quantity });

                }
            }
            else
            {
                TempData["NotificationError"] = "Đã bỏ sản phẩm khỏi đơn!";

                var item = cart.FirstOrDefault(t => t.ItemId == id);
                if (item != null)
                {
                    cart.Remove(item);
                }

            }


            HttpContext.Session.SetObjectAsJson("FoodCart", cart);
            return RedirectToAction("OrderFood");

        }
        public IActionResult GearDetail(int id, int quantity, int quantityavai)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    GearVM tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears").ToList().FirstOrDefault(s => s.GearId == id);

                    tickets.Quantity = quantity;
                    tickets.QuantityAvailable = quantityavai;
                    return View("GearDetail", tickets);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }

        }
        [HttpPost]
        public IActionResult GearDetailCart(int id, string name, decimal price, int quantity, int quantityavai)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GearVM>>("GearCart") ?? new List<GearVM>();

            if (quantity > 0)
            {
                TempData["NotificationSuccess"] = "Đã thêm sản phẩm vào đơn!";

                var item = cart.FirstOrDefault(t => t.GearId == id);

                if (item != null)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Add(new GearVM { GearId = id, GearName = name, RentalPrice = price, Description = "", Quantity = quantity, QuantityAvailable = quantityavai });

                }
            }
            else
            {
                TempData["NotificationError"] = "Đã bỏ sản phẩm khỏi đơn!";

                var item = cart.FirstOrDefault(t => t.GearId == id);
                if (item != null)
                {
                    cart.Remove(item);
                }

            }


            HttpContext.Session.SetObjectAsJson("GearCart", cart);
            return RedirectToAction("OrderGear");

        }
        public IActionResult FoodCart(List<int> id, List<string> name, List<decimal> price, List<int> quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();


            for (int i = 0; i < id.Count; i++)
            {

                if (quantity[i] > 0)
                {
                    var item = cart.FirstOrDefault(t => t.ItemId == id[i]);

                    if (item != null)
                    {
                        item.Quantity = quantity[i];
                    }
                    else
                    {
                        cart.Add(new FoodAndDrinkVM() { ItemId = id[i], ItemName = name[i], Price = price[i], Description = "", Quantity = quantity[i] });

                    }
                }
                else
                {
                    var item = cart.FirstOrDefault(t => t.ItemId == id[i]);
                    if (item != null)
                    {
                        cart.Remove(item);
                    }

                }



            }
            TempData["NotificationSuccess"] = "Đá thêm sản phẩm vào đơn! ";

            HttpContext.Session.SetObjectAsJson("FoodCart", cart);
            return RedirectToAction("OrderFood");
        }
        public IActionResult OrderComboFood()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();

                    List<ComboFoodVM> tickets = GetDataFromApi<List<ComboFoodVM>>("http://103.20.97.182:5124/api/ComboFood/GetAllOrders\r\n");

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.ComboId == item.ComboId);
                        if (ticket != null)
                        {
                            ticket.Quantity = item.Quantity;
                        }
                    }
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }
                    ViewBag.gears = tickets;
                    return View("OrderComboFood");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }
        }
        public IActionResult ComboFoodCart(List<int> id, List<string> name, List<decimal> price, List<int> quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();


            for (int i = 0; i < id.Count; i++)
            {

                if (quantity[i] > 0)
                {
                    var item = cart.FirstOrDefault(t => t.ComboId == id[i]);

                    if (item != null)
                    {
                        item.Quantity = quantity[i];
                    }
                    else
                    {
                        cart.Add(new ComboFoodVM() { ComboId = id[i], ComboName = name[i], Price = price[i], Quantity = quantity[i] });

                    }
                }
                else
                {
                    var item = cart.FirstOrDefault(t => t.ComboId == id[i]);
                    if (item != null)
                    {
                        cart.Remove(item);
                    }

                }



            }
            TempData["NotificationSuccess"] = "Đã thêm sản phẩm vào đơn!";

            HttpContext.Session.SetObjectAsJson("ComboFoodCart", cart);
            return RedirectToAction("OrderComboFood");
        }
        public IActionResult OrderCombo()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
                    var combo = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();

                    if (orders.CustomerName.IsNullOrEmpty() || orders.OrderUsageDate == null || orders.PhoneCustomer.IsNullOrEmpty())
                    {
                        TempData["NotificationError"] = "Bạn phải điền đầy đủ thông tin bao gồm tên,số điện thoại và ngày sử dụng!";
                        return RedirectToAction("CreateOrder");

                    }
                    else
                    {
                        if (!combo.IsNullOrEmpty())
                        {
                            TempData["NotificationError"] = "Bạn đang đặt vé vui lòng không đặt thêm combo!";

                            return RedirectToAction("CreateOrder");
                        }
                        else
                        {
                            var ticketscart = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();

                            List<ComboVM> tickets = GetDataFromApi<List<ComboVM>>("http://103.20.97.182:5124/api/Combo/GetAllCombos\r\n");

                            foreach (var item in ticketscart)
                            {
                                var ticket = tickets.ToList().FirstOrDefault(s => s.ComboId == item.ComboId);
                                if (ticket != null)
                                {
                                    ticket.Quantity = item.Quantity;
                                }
                            }

                            if (TempData["NotificationSuccess"] != null)
                            {
                                ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                            }
                            if (TempData["NotificationError"] != null)
                            {
                                ViewBag.NotificationError = TempData["NotificationError"];
                            }
                            ViewBag.gears = tickets;
                            return View("OrderCombo");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }
        }
        public IActionResult ComboCart(List<int> id, List<string> name, List<decimal> price, List<int> quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();


            for (int i = 0; i < id.Count; i++)
            {

                if (quantity[i] > 0)
                {
                    var item = cart.FirstOrDefault(t => t.ComboId == id[i]);

                    if (item != null)
                    {
                        item.Quantity = quantity[i];
                    }
                    else
                    {
                        cart.Add(new ComboVM() { ComboId = id[i], ComboName = name[i], Price = price[i], Quantity = quantity[i] });

                    }
                }
                else
                {
                    var item = cart.FirstOrDefault(t => t.ComboId == id[i]);
                    if (item != null)
                    {
                        cart.Remove(item);
                    }

                }



            }
            TempData["NotificationSuccess"] = "Thêm vào giỏ hàng thành công.";

            HttpContext.Session.SetObjectAsJson("ComboCart", cart);
            return RedirectToAction("OrderCombo");
        }
        public IActionResult Cart()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var tickets = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();
                    var gears = HttpContext.Session.GetObjectFromJson<List<GearVM>>("GearCart") ?? new List<GearVM>();
                    var foods = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();
                    var combos = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();
                    var combofoods = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();
                    if (tickets.IsNullOrEmpty() && combos.IsNullOrEmpty())
                    {
                        TempData["NotificationError"] = "Phải đặt vé hoặc combo bao gồm vé mới có thể lên đơn.";

                        return RedirectToAction("CreateOrder");
                    }
                    else
                    {
                        var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
                        decimal total = 0;
                        foreach (var item in tickets)
                        {
                            total += item.Quantity * item.Price;
                        }
                        foreach (var item in gears)
                        {
                            total += item.Quantity * item.RentalPrice;
                        }
                        foreach (var item in foods)
                        {
                            total += item.Quantity * item.Price;
                        }
                        foreach (var item in combos)
                        {
                            total += item.Quantity * item.Price;
                        }
                        foreach (var item in combofoods)
                        {
                            total += item.Quantity * item.Price;
                        }

                        ViewBag.tickets = tickets;
                        ViewBag.gears = gears;
                        ViewBag.foods = foods;
                        ViewBag.order = orders;
                        ViewBag.combos = combos;
                        ViewBag.combofoods = combofoods;
                        ViewBag.Total = total;
                        var viewModel = new DepositVM
                        {

                            Total = 3_500_000
                        };
                        viewModel.CalculateRoundedValues();

                        return View("Cart", viewModel);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");

            }
        }
        [HttpPost]
        public async Task<IActionResult> Order(decimal deposit, decimal total)
        {
            // Retrieve session data
            var orders = HttpContext.Session.GetObjectFromJson<OrderVM>("OrderCart") ?? new OrderVM();
            var tickets = HttpContext.Session.GetObjectFromJson<List<TicketVM>>("TicketCart") ?? new List<TicketVM>();
            var gears = HttpContext.Session.GetObjectFromJson<List<GearVM>>("GearCart") ?? new List<GearVM>();
            var foods = HttpContext.Session.GetObjectFromJson<List<FoodAndDrinkVM>>("FoodCart") ?? new List<FoodAndDrinkVM>();
            var combofoods = HttpContext.Session.GetObjectFromJson<List<ComboFoodVM>>("ComboFoodCart") ?? new List<ComboFoodVM>();
            var combos = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboCart") ?? new List<ComboVM>();

            // Retrieve JWT Token from cookies


            // Ensure there's at least one item to process (tickets, gears, etc.)
            if (tickets.Any())
            {
                // Create order request
                var orderRequest = new CreateUniqueOrderRequest
                {
                    Order = new OrderAddDTO
                    {
                        EmployeeId = HttpContext.Session.GetInt32("UserId"),  // Assuming this is static for now
                        CustomerName = orders.CustomerName,
                        OrderUsageDate = orders.OrderUsageDate,
                        Deposit = deposit,
                        TotalAmount = total,
                        PhoneCustomer = orders.PhoneCustomer,
                    },
                    OrderTicket = tickets.Select(t => new OrderTicketAddlDTO
                    {
                        TicketId = t.TicketId,
                        Quantity = t.Quantity,
                    }).ToList(),
                    OrderCampingGear = gears.Select(g => new OrderCampingGearAddDTO
                    {
                        GearId = g.GearId,
                        Quantity = g.Quantity
                    }).ToList(),
                    OrderFood = foods.Select(f => new OrderFoodAddDTO
                    {
                        ItemId = f.ItemId,
                        Quantity = f.Quantity
                    }).ToList(),
                    OrderFoodCombo = combofoods.Select(cf => new OrderFoodComboAddDTO
                    {
                        ComboId = cf.ComboId,
                        Quantity = cf.Quantity
                    }).ToList()
                };

                try
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/CreateUniqueOrder\r\n";

                    // Serialize request object to JSON
                    var content = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");
                    Console.WriteLine(content);
                    // Make the API call
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    // Process API response
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        TempData["NotificationSuccess"] = "Tạo đơn thành công.";
                        // Xóa OrderCart
                        HttpContext.Session.Remove("OrderCart");

                        // Xóa TicketCart
                        HttpContext.Session.Remove("TicketCart");
                        HttpContext.Session.Remove("ComboCart");


                        // Xóa GearCart
                        HttpContext.Session.Remove("GearCart");

                        // Xóa FoodCart
                        HttpContext.Session.Remove("FoodCart");

                        // Xóa ComboFoodCart
                        HttpContext.Session.Remove("ComboFoodCart");
                        if (deposit > 0)
                        {
                            return RedirectToAction("OrderOnline");  // Return success message or redirect if needed

                        }
                        else
                        {
                            return RedirectToAction("OrderUsing");  // Return success message or redirect if needed

                        }
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["NotificationError"] = "Tạo đơn thất bại.";
                        return RedirectToAction("CreateOrder");  // Return success message or redirect if needed
                    }
                }
                catch (Exception ex)
                {

                    return RedirectToAction("Error");  // Return success message or redirect if needed
                }
            }
            else
            {
                var orderRequest = new CreateComboOrderRequest
                {
                    Order = new OrderAddDTO
                    {
                        EmployeeId = HttpContext.Session.GetInt32("UserId"),  // Assuming this is static for now
                        CustomerName = orders.CustomerName,
                        OrderUsageDate = orders.OrderUsageDate,
                        Deposit = deposit,
                        TotalAmount = total,
                        PhoneCustomer = orders.PhoneCustomer,
                    },
                    OrderCombo = combos.Select(t => new OrderComboAddDTO
                    {
                        ComboId = t.ComboId,
                        Quantity = t.Quantity,
                    }).ToList(),
                    OrderCampingGear = gears.Select(g => new OrderCampingGearAddDTO
                    {
                        GearId = g.GearId,
                        Quantity = g.Quantity
                    }).ToList(),
                    OrderFood = foods.Select(f => new OrderFoodAddDTO
                    {
                        ItemId = f.ItemId,
                        Quantity = f.Quantity
                    }).ToList(),
                    OrderFoodCombo = combofoods.Select(cf => new OrderFoodComboAddDTO
                    {
                        ComboId = cf.ComboId,
                        Quantity = cf.Quantity
                    }).ToList()
                };

                try
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/CreateComboOrder";

                    // Serialize request object to JSON
                    var content = new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json");

                    // Make the API call
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    // Process API response
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        TempData["NotificationSuccess"] = "Tạo đơn thành công.";
                        HttpContext.Session.Remove("OrderCart");

                        // Xóa TicketCart
                        HttpContext.Session.Remove("TicketCart");

                        // Xóa GearCart
                        HttpContext.Session.Remove("GearCart");

                        // Xóa FoodCart
                        HttpContext.Session.Remove("FoodCart");
                        HttpContext.Session.Remove("ComboCart");

                        // Xóa ComboFoodCart
                        HttpContext.Session.Remove("ComboFoodCart");
                        if (deposit > 0)
                        {


                            return RedirectToAction("OrderOnline");  // Return success message or redirect if needed

                        }
                        else
                        {
                            return RedirectToAction("OrderUsing");  // Return success message or redirect if needed

                        }  // Return success message or redirect if needed
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["NotificationError"] = "Tạo đơn thất bại.";
                        return RedirectToAction("CreateOrder");  // Return success message or redirect if needed
                    }
                }
                catch (Exception ex)
                {
                    TempData["Notification"] = "Xảy ra lỗi ngoài luồng khi tạo đơn.";

                    return RedirectToAction("Error");  // Return success message or redirect if needed
                }
            }

            // If no items to process, return the CreateOrder view
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCart()
        {
            // Xóa OrderCart
            HttpContext.Session.Remove("OrderCart");

            // Xóa TicketCart
            HttpContext.Session.Remove("TicketCart");

            // Xóa GearCart
            HttpContext.Session.Remove("GearCart");
            HttpContext.Session.Remove("ComboCart");

            // Xóa FoodCart
            HttpContext.Session.Remove("FoodCart");
            TempData["NotificationSuccess"] = "Đã khởi tạo lại đơn!";

            // Xóa ComboFoodCart
            HttpContext.Session.Remove("ComboFoodCart");
            return RedirectToAction("CreateOrder");

        }
        [HttpGet]
        public async Task<IActionResult> CancelForm(int itemid)
        {

            HttpContext.Session.Remove("GearUpdateDateCart");
            return RedirectToAction("OrderDetail", new { id = itemid });

        }
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {

                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateDateCart") ?? new List<OrderCampingGearDetailDTO>();
                    if (!ticketscart.IsNullOrEmpty())
                    {
                        ViewBag.gear = ticketscart;
                    }
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    OrderDetailVM orderdata = GetDataFromApi<OrderDetailVM>($"http://103.20.97.182:5124/api/OrderManagement/GetOrderDetail/{id}");


                    int daysDifference = (DateTime.Now.Date - orderdata.OrderUsageDate.Value.Date).Days;
                    if (daysDifference >= 1)
                    {
                        if (orderdata.ActivityId == 2)
                        {
                            decimal money_ticket = 0;
                            decimal money_ticketday = 0;
                            foreach (var item in orderdata.OrderTicketDetails)
                            {
                                money_ticketday += (item.Price * (decimal)item.Quantity) * (daysDifference + 1);
                                money_ticket += (item.Price * (decimal)item.Quantity);
                            }
                            orderdata.TotalAmount = orderdata.TotalAmount - money_ticket + money_ticketday;
                            orderdata.OrderCheckoutDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm"));
                            orderdata.AmountPayable = orderdata.TotalAmount - orderdata.Deposit;
                        }

                    }
                    UpdateOrderDTO updateorder = new UpdateOrderDTO()
                    {
                        OrderId = orderdata.OrderId,
                        OrderUsageDate = orderdata.OrderUsageDate,
                        TotalAmount = orderdata.TotalAmount,
                        OrderCheckoutDate = orderdata.OrderCheckoutDate != null ? orderdata.OrderCheckoutDate : DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                    };

                    if (orderdata.ActivityId == 2 || orderdata.ActivityId == 1)
                    {
                        HttpContext.Session.SetObjectAsJson("order", updateorder);
                        HttpContext.Session.SetObjectAsJson("TicketUpdateCart", orderdata.OrderTicketDetails);
                        HttpContext.Session.SetObjectAsJson("ComboUpdateCart", orderdata.OrderComboDetails);
                        HttpContext.Session.SetObjectAsJson("ComboFoodUpdateCart", orderdata.OrderFoodComboDetails);
                        HttpContext.Session.SetObjectAsJson("FoodUpdateCart", orderdata.OrderFoodDetails);
                        HttpContext.Session.SetObjectAsJson("GearUpdateCart", orderdata.OrderCampingGearDetails);
                    }
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }
                    ViewBag.date = daysDifference + 1;

                    return View("OrderDetail", orderdata);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }




        }
        [HttpGet]
        public IActionResult ChangeUsagedate(int idorder, DateTime? changedate)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    OrderDetailVM orderdata = GetDataFromApi<OrderDetailVM>($"http://103.20.97.182:5124/api/OrderManagement/GetOrderDetail/{idorder}");
                    string formattedDate = changedate.Value.ToString("yyyy-MM-dd");

                    List<GearVM> tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears");
                    List<OrderCampingGearByUsageDateDTO> ordergear = GetDataFromApi<List<OrderCampingGearByUsageDateDTO>>($"http://103.20.97.182:5124/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");
                    foreach (var item in tickets)
                    {
                        var ticket = ordergear.ToList().Where(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            foreach (var item1 in ticket)
                            {
                                item.QuantityAvailable = item.QuantityAvailable - item1.Quantity.Value;
                            }
                        }
                    }
                    foreach (var item in orderdata.OrderCampingGearDetails)
                    {
                        var ticket = tickets.FirstOrDefault(s => item.GearId == s.GearId);
                        item.QuantityAvaiable = ticket.QuantityAvailable;
                    }
                    UpdateOrderDTO updateorder = new UpdateOrderDTO()
                    {
                        OrderId = orderdata.OrderId,
                        OrderUsageDate = changedate,
                        TotalAmount = orderdata.TotalAmount,
                    };

                    HttpContext.Session.SetObjectAsJson("order", updateorder);
                    HttpContext.Session.SetObjectAsJson("GearUpdateDateCart", orderdata.OrderCampingGearDetails);

                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationError"];
                    }
                    return View("ChangeUsagedate", orderdata);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }
        public IActionResult UpdateGearChangeDate()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateDateCart") ?? new List<OrderCampingGearDetailDTO>();
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
                    string formattedDate = order.OrderUsageDate.Value.ToString("yyyy-MM-dd");
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    List<GearVM> tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears");
                    List<OrderCampingGearByUsageDateDTO> ordergear = GetDataFromApi<List<OrderCampingGearByUsageDateDTO>>($"http://103.20.97.182:5124/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");
                    foreach (var item in tickets)
                    {
                        var ticket = ordergear.ToList().Where(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            foreach (var item1 in ticket)
                            {
                                item.QuantityAvailable = item.QuantityAvailable - item1.Quantity.Value;
                            }
                        }
                    }

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            if (item.Quantity >= ticket.QuantityAvailable)
                            {
                                ticket.Quantity = ticket.QuantityAvailable;
                                ticket.QuantityAvailable = 0;
                            }
                            else
                            {
                                ticket.Quantity = item.Quantity.Value;
                                ticket.QuantityAvailable -= item.Quantity.Value;
                            }
                        }
                    }

                    ViewBag.id = order.OrderId;
                    ViewBag.gears = tickets;
                    return View("UpdateGearChangeDate");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateGearChangeDate(List<int> id, List<decimal> price, List<int> quantity)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateDateCart") ?? new List<OrderCampingGearDetailDTO>();

            decimal total = 0;
            List<OrderCampingGearAddDTO> tickets = new List<OrderCampingGearAddDTO>();
            for (int i = 0; i < id.Count; i++)
            {


                if (quantity[i] > 0)
                {

                    tickets.Add(new OrderCampingGearAddDTO { GearId = id[i], OrderId = order.OrderId, Quantity = quantity[i] });
                    total += (decimal)quantity[i] * price[i];
                }



            }
            decimal totalticket = 0;
            foreach (var item in ticketscart)
            {
                totalticket += (decimal)item.Quantity.Value * item.Price;
            }

            if (tickets.Count == 0)
            {
                tickets.Add(new OrderCampingGearAddDTO
                {
                    GearId = 0,
                    OrderId = order.OrderId,
                    Quantity = 0
                });
            }
            UpdateOrderDTO orderupdate = new UpdateOrderDTO()
            {
                OrderId = order.OrderId,
                OrderUsageDate = order.OrderUsageDate,
                TotalAmount = order.TotalAmount - totalticket + total,
            };
            Console.WriteLine("Dddddddddddddd2" + orderupdate.OrderUsageDate);
            var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateCampingGear";
            var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
            Console.WriteLine(tickets.Count);
            // Serialize request objects to JSON
            var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
            var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Call to UpdateTicket API
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // Proceed to the next API call if the first one is successful
                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        // Both updates were successful
                        TempData["NotificationSuccess"] = "Đơn đã cập nhập lại và chuyển ngày thành công.";
                        HttpContext.Session.Remove("Order");
                        HttpContext.Session.Remove("GearUpdateDateCart");

                        return RedirectToAction("OrderDetail", new { id = order.OrderId });
                    }
                    else
                    {
                        // Handle error for the second API call
                        var errorMessage1 = await response1.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                else
                {
                    // Handle error for the first API call
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpdateGearChangeDate2(List<int> id, List<decimal> price, List<int> quantity)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order2") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateDateCart") ?? new List<OrderCampingGearDetailDTO>();

            decimal total = 0;
            List<OrderCampingGearAddDTO> tickets = new List<OrderCampingGearAddDTO>();
            for (int i = 0; i < id.Count; i++)
            {


                if (quantity[i] > 0)
                {

                    tickets.Add(new OrderCampingGearAddDTO { GearId = id[i], OrderId = order.OrderId, Quantity = quantity[i] });
                    total += (decimal)quantity[i] * price[i];
                }



            }
            decimal totalticket = 0;
            foreach (var item in ticketscart)
            {
                totalticket += (decimal)item.Quantity.Value * item.Price;
            }

            if (tickets.Count == 0)
            {
                tickets.Add(new OrderCampingGearAddDTO
                {
                    GearId = 0,
                    OrderId = order.OrderId,
                    Quantity = 0
                });
            }
            UpdateOrderDTO orderupdate = new UpdateOrderDTO()
            {
                OrderId = order.OrderId,
                OrderUsageDate = order.OrderUsageDate,
                TotalAmount = order.TotalAmount - totalticket + total,
            };
            Console.WriteLine("Dddddddddddddd2" + orderupdate.OrderUsageDate);
            var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateCampingGear";
            var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
            Console.WriteLine(tickets.Count);
            // Serialize request objects to JSON
            var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
            var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Call to UpdateTicket API
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // Proceed to the next API call if the first one is successful
                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        string apiUrl3 = $"http://103.20.97.182:5124/api/OrderManagement/UpdateActivityOrder/{order.OrderId}/{2}";

                        var response2 = _httpClient.PutAsync(apiUrl3, null).Result;
                        // Both updates were successful
                        TempData["NotificationSuccess"] = $"Đơn {order.OrderId} đã chuyển sang trạng thái đang sử dụng.";
                        HttpContext.Session.Remove("Order2");
                        HttpContext.Session.Remove("GearUpdateDateCart");

                        return RedirectToAction("OrderUsing");
                    }
                    else
                    {
                        // Handle error for the second API call
                        var errorMessage1 = await response1.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                else
                {
                    // Handle error for the first API call
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
            }

        }
        [HttpGet]
        public IActionResult UpdateTicket()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();

                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderTicketDetailDTO>>("TicketUpdateCart") ?? new List<OrderTicketDetailDTO>();
                    List<TicketVM> tickets = GetDataFromApi<List<TicketVM>>("http://103.20.97.182:5124/api/Ticket/GetAllTickets");

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.TicketId == item.TicketId);
                        if (ticket != null)
                        {
                            ticket.Quantity = item.Quantity.Value;
                        }
                    }
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationError = TempData["NotificationError"];
                    }
                    ViewBag.id = order.OrderId;
                    ViewBag.tickets = tickets;
                    return View("UpdateTicket");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }



        }
        [HttpPost]
        public async Task<IActionResult> UpdateTicket(List<int> TicketIds, List<decimal> Prices, List<int> Quantities)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderTicketDetailDTO>>("TicketUpdateCart") ?? new List<OrderTicketDetailDTO>();
            bool check = false;
            for (int i = 0; i < Quantities.Count; i++)
            {
                if (Quantities[i] != 0)
                {
                    check = true; // Tìm thấy ít nhất một phần tử khác 0
                    break;        // Thoát vòng lặp sớm
                }
            }
            if (check == false)
            {
                TempData["NotificationError"] = "Cập nhập lại vé thất bại!Không được hủy tất cả vé.";

                return RedirectToAction("UpdateTicket");
            }
            else
            {
                decimal total = 0;
                List<OrderTicketAddlDTO> tickets = new List<OrderTicketAddlDTO>();
                for (int i = 0; i < TicketIds.Count; i++)
                {


                    if (Quantities[i] > 0)
                    {

                        tickets.Add(new OrderTicketAddlDTO { TicketId = TicketIds[i], OrderId = order.OrderId, Quantity = Quantities[i] });
                        total += (decimal)Quantities[i] * Prices[i];
                    }



                }
                decimal totalticket = 0;
                foreach (var item in ticketscart)
                {
                    totalticket += (decimal)item.Quantity.Value * item.Price;
                }

                if (tickets.Count == 0)
                {
                    tickets.Add(new OrderTicketAddlDTO
                    {
                        TicketId = 0,
                        OrderId = order.OrderId,
                        Quantity = 0
                    });
                }
                UpdateOrderDTO orderupdate = new UpdateOrderDTO()
                {
                    OrderId = order.OrderId,
                    OrderUsageDate = order.OrderUsageDate,
                    TotalAmount = order.TotalAmount - totalticket + total,
                };
                var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateTicket";
                var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
                Console.WriteLine(tickets.Count);
                // Serialize request objects to JSON
                var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
                var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

                try
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    // Call to UpdateTicket API
                    var response = await _httpClient.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Proceed to the next API call if the first one is successful
                        var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                        if (response1.IsSuccessStatusCode)
                        {
                            // Both updates were successful
                            TempData["NotificationSuccess"] = "Cập nhập lại vé thành công.";
                            return RedirectToAction("OrderDetail", new { id = order.OrderId });
                        }
                        else
                        {
                            // Handle error for the second API call
                            var errorMessage1 = await response1.Content.ReadAsStringAsync();
                            ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                            return View("ErrorView"); // Replace "ErrorView" with your actual error view
                        }
                    }
                    else
                    {
                        // Handle error for the first API call
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                        return View("ErrorView"); // Replace "ErrorView" with your actual error view
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the process
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }

            }



        }
        public IActionResult UpdateGear()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateCart") ?? new List<OrderCampingGearDetailDTO>();
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
                    string formattedDate = order.OrderUsageDate.Value.ToString("yyyy-MM-dd");
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    List<GearVM> tickets = GetDataFromApi<List<GearVM>>("http://103.20.97.182:5124/api/CampingGear/GetAllCampingGears");
                    List<OrderCampingGearByUsageDateDTO> ordergear = GetDataFromApi<List<OrderCampingGearByUsageDateDTO>>($"http://103.20.97.182:5124/api/OrderManagement/GetListOrderGearByUsageDate/{formattedDate}");
                    foreach (var item in tickets)
                    {
                        var ticket = ordergear.ToList().Where(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            foreach (var item1 in ticket)
                            {
                                item.QuantityAvailable = item.QuantityAvailable - item1.Quantity.Value;
                            }
                        }
                    }

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.GearId == item.GearId);
                        if (ticket != null)
                        {
                            ticket.Quantity = item.Quantity.Value;
                        }
                    }

                    ViewBag.id = order.OrderId;
                    ViewBag.gears = tickets;
                    return View("UpdateGear");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateGear(List<int> id, List<decimal> price, List<int> quantity)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderCampingGearDetailDTO>>("GearUpdateCart") ?? new List<OrderCampingGearDetailDTO>();

            decimal total = 0;
            List<OrderCampingGearAddDTO> tickets = new List<OrderCampingGearAddDTO>();
            for (int i = 0; i < id.Count; i++)
            {


                if (quantity[i] > 0)
                {

                    tickets.Add(new OrderCampingGearAddDTO { GearId = id[i], OrderId = order.OrderId, Quantity = quantity[i] });
                    total += (decimal)quantity[i] * price[i];
                }



            }
            decimal totalticket = 0;
            foreach (var item in ticketscart)
            {
                totalticket += (decimal)item.Quantity.Value * item.Price;
            }

            if (tickets.Count == 0)
            {
                tickets.Add(new OrderCampingGearAddDTO
                {
                    GearId = 0,
                    OrderId = order.OrderId,
                    Quantity = 0
                });
            }
            UpdateOrderDTO orderupdate = new UpdateOrderDTO()
            {
                OrderId = order.OrderId,
                OrderUsageDate = order.OrderUsageDate,
                TotalAmount = order.TotalAmount - totalticket + total,
            };
            var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateCampingGear";
            var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
            Console.WriteLine(tickets.Count);
            // Serialize request objects to JSON
            var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
            var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Call to UpdateTicket API
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // Proceed to the next API call if the first one is successful
                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        // Both updates were successful
                        TempData["NotificationSuccess"] = "Cập nhập lại đồ dùng cắm trại thành công.";
                        return RedirectToAction("OrderDetail", new { id = order.OrderId });
                    }
                    else
                    {
                        // Handle error for the second API call
                        var errorMessage1 = await response1.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                else
                {
                    // Handle error for the first API call
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
            }

        }

        public IActionResult UpdateFood()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderFoodDetailDTO>>("FoodUpdateCart") ?? new List<OrderFoodDetailDTO>();
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();

                    List<FoodAndDrinkVM> foodAndDrinks = GetDataFromApi<List<FoodAndDrinkVM>>("http://103.20.97.182:5124/api/FoodAndDrink/GetAllFoodAndDrink");

                    foreach (var item in ticketscart)
                    {
                        var ticket = foodAndDrinks.ToList().FirstOrDefault(s => s.ItemId == item.ItemId);
                        if (ticket != null)
                        {
                            ticket.Description = item.Description;
                            ticket.Quantity = item.Quantity.Value;
                        }
                    }

                    ViewBag.id = order.OrderId;

                    ViewBag.gears = foodAndDrinks;
                    return View("UpdateFood");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateFood(List<int> id, List<decimal> price, List<int> quantity, List<string> description)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderFoodDetailDTO>>("FoodUpdateCart") ?? new List<OrderFoodDetailDTO>();

            decimal total = 0;
            List<OrderFoodAddDTO> tickets = new List<OrderFoodAddDTO>();
            for (int i = 0; i < id.Count; i++)
            {


                if (quantity[i] > 0)
                {

                    tickets.Add(new OrderFoodAddDTO { ItemId = id[i], OrderId = order.OrderId, Quantity = quantity[i] });
                    total += (decimal)quantity[i] * price[i];
                }



            }
            decimal totalticket = 0;
            foreach (var item in ticketscart)
            {
                totalticket += (decimal)item.Quantity.Value * item.Price;
            }

            if (tickets.Count == 0)
            {
                tickets.Add(new OrderFoodAddDTO
                {
                    ItemId = 0,
                    OrderId = order.OrderId,
                    Quantity = 0
                });
            }
            UpdateOrderDTO orderupdate = new UpdateOrderDTO()
            {
                OrderId = order.OrderId,
                OrderUsageDate = order.OrderUsageDate,
                TotalAmount = (order.TotalAmount - totalticket) + total,
            };
            var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateFoodAndDrink";
            var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
            Console.WriteLine(tickets.Count);
            // Serialize request objects to JSON
            var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
            var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Call to UpdateTicket API
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // Proceed to the next API call if the first one is successful
                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        // Both updates were successful
                        TempData["NotificationSuccess"] = "Cập nhập lại đồ ăn thành công.";
                        return RedirectToAction("OrderDetail", new { id = order.OrderId });
                    }
                    else
                    {
                        // Handle error for the second API call
                        var errorMessage1 = await response1.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                else
                {
                    // Handle error for the first API call
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
            }


        }


        public IActionResult UpdateComboFood()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2)))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderFoodComboDetailDTO>>("ComboFoodUpdateCart") ?? new List<OrderFoodComboDetailDTO>();
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();

                    List<ComboFoodVM> tickets = GetDataFromApi<List<ComboFoodVM>>("http://103.20.97.182:5124/api/ComboFood/GetAllOrders");

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.ComboId == item.ComboId);
                        if (ticket != null)
                        {
                            ticket.Quantity = item.Quantity.Value;
                        }
                    }


                    ViewBag.gears = tickets;
                    ViewBag.id = order.OrderId;

                    return View("UpdateComboFood");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateComboFood(List<int> id, List<decimal> price, List<int> quantity)
        {
            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderFoodComboDetailDTO>>("ComboFoodUpdateCart") ?? new List<OrderFoodComboDetailDTO>();

            decimal total = 0;
            List<OrderFoodComboAddDTO> tickets = new List<OrderFoodComboAddDTO>();
            for (int i = 0; i < id.Count; i++)
            {


                if (quantity[i] > 0)
                {

                    tickets.Add(new OrderFoodComboAddDTO { ComboId = id[i], OrderId = order.OrderId, Quantity = quantity[i] });
                    total += (decimal)quantity[i] * price[i];
                }



            }
            decimal totalticket = 0;
            foreach (var item in ticketscart)
            {
                totalticket += (decimal)item.Quantity.Value * item.Price;
            }

            if (tickets.Count == 0)
            {
                tickets.Add(new OrderFoodComboAddDTO
                {
                    ComboId = 0,
                    OrderId = order.OrderId,
                    Quantity = 0
                });
            }
            UpdateOrderDTO orderupdate = new UpdateOrderDTO()
            {
                OrderId = order.OrderId,
                OrderUsageDate = order.OrderUsageDate,
                TotalAmount = (order.TotalAmount - totalticket) + total,
            };
            var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateFoodCombo";
            var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";

            var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
            var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                // Call to UpdateTicket API
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // Proceed to the next API call if the first one is successful
                    var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                    if (response1.IsSuccessStatusCode)
                    {
                        // Both updates were successful
                        TempData["NotificationSuccess"] = "Cập nhập lại combo đồ ăn thành công.";
                        return RedirectToAction("OrderDetail", new { id = order.OrderId });

                    }
                    else
                    {
                        // Handle error for the second API call
                        var errorMessage1 = await response1.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                else
                {
                    // Handle error for the first API call
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
            }
        }
        public IActionResult UpdateCombo()
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    var ticketscart = HttpContext.Session.GetObjectFromJson<List<ComboVM>>("ComboUpdateCart") ?? new List<ComboVM>();
                    var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();

                    List<ComboVM> tickets = GetDataFromApi<List<ComboVM>>("http://103.20.97.182:5124/api/Combo/GetAllCombos");

                    foreach (var item in ticketscart)
                    {
                        var ticket = tickets.ToList().FirstOrDefault(s => s.ComboId == item.ComboId);
                        if (ticket != null)
                        {
                            ticket.Quantity = item.Quantity;
                        }
                    }
                    if (TempData["NotificationSuccess"] != null)
                    {
                        ViewBag.NotificationSuccess = TempData["NotificationSuccess"];
                    }
                    if (TempData["NotificationError"] != null)
                    {
                        ViewBag.NotificationError = TempData["NotificationError"];
                    }
                    ViewBag.id = order.OrderId;

                    ViewBag.gears = tickets;
                    return View("UpdateCombo");
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCombo(List<int> id, List<decimal> price, List<int> quantity)
        {

            var order = HttpContext.Session.GetObjectFromJson<UpdateOrderDTO>("order") ?? new UpdateOrderDTO();
            var ticketscart = HttpContext.Session.GetObjectFromJson<List<OrderComboDetailDTO>>("ComboUpdateCart") ?? new List<OrderComboDetailDTO>();
            bool check = false;
            for (int i = 0; i < quantity.Count; i++)
            {
                if (quantity[i] != 0)
                {
                    check = true; // Tìm thấy ít nhất một phần tử khác 0
                    break;        // Thoát vòng lặp sớm
                }
            }
            if (check == false)
            {
                TempData["NotificationError"] = "Cập nhập lại combo thất bại!Không được hủy tất cả combo.";

                return RedirectToAction("UpdateCombo");
            }
            else
            {
                decimal total = 0;
                List<OrderComboAddDTO> tickets = new List<OrderComboAddDTO>();
                for (int i = 0; i < id.Count; i++)
                {


                    if (quantity[i] > 0)
                    {

                        tickets.Add(new OrderComboAddDTO { ComboId = id[i], OrderId = order.OrderId, Quantity = quantity[i], Description = "string" });
                        total += (decimal)quantity[i] * price[i];
                    }



                }
                decimal totalticket = 0;
                foreach (var item in ticketscart)
                {
                    totalticket += (decimal)item.Quantity.Value * item.Price;
                }

                if (tickets.Count == 0)
                {
                    tickets.Add(new OrderComboAddDTO
                    {
                        ComboId = 0,
                        OrderId = order.OrderId,
                        Quantity = 0,
                        Description = "string"
                    });
                }
                UpdateOrderDTO orderupdate = new UpdateOrderDTO()
                {
                    OrderId = order.OrderId,
                    OrderUsageDate = order.OrderUsageDate,
                    TotalAmount = order.TotalAmount - totalticket + total,
                };
                var apiUrl = "http://103.20.97.182:5124/api/OrderManagement/UpdateCombo";
                var apiUrl1 = "http://103.20.97.182:5124/api/OrderManagement/UpdateOrder";
                Console.WriteLine(tickets.Count);
                // Serialize request objects to JSON
                var content = new StringContent(JsonConvert.SerializeObject(tickets), Encoding.UTF8, "application/json");
                var content1 = new StringContent(JsonConvert.SerializeObject(orderupdate), Encoding.UTF8, "application/json");

                try
                {
                    var jwtToken = Request.Cookies["JWTToken"];
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    // Call to UpdateTicket API
                    var response = await _httpClient.PutAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Proceed to the next API call if the first one is successful
                        var response1 = await _httpClient.PostAsync(apiUrl1, content1);

                        if (response1.IsSuccessStatusCode)
                        {
                            // Both updates were successful
                            TempData["NotificationSuccess"] = "Cập nhập lại combo thành công.";
                            return RedirectToAction("OrderDetail", new { id = order.OrderId });
                        }
                        else
                        {
                            // Handle error for the second API call
                            var errorMessage1 = await response1.Content.ReadAsStringAsync();
                            ViewBag.ErrorMessage = $"Error updating order: {errorMessage1}";
                            return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                        }
                    }
                    else
                    {
                        // Handle error for the first API call
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        ViewBag.ErrorMessage = $"Error updating tickets: {errorMessage}";
                        return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the process
                    return RedirectToAction("Error"); // Replace "ErrorView" with your actual error view
                }
            }

        }
        public decimal RoundToNearestTen(decimal amount)
        {
            return Math.Round(amount / 10) * 10;
        }

    }
}
