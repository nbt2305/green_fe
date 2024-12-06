using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace GreenGardenClient.Controllers.AdminController
{
    public class EmployeeManagementController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public EmployeeManagementController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Private method to fetch data from API
        private T GetDataFromApi<T>(string url)
        {
            var client = _clientFactory.CreateClient();

            // Retrieve JWT token from cookies
            var jwtToken = Request.Cookies["JWTToken"];

            // Check if JWT token is present; if not, return default (null)
            if (string.IsNullOrEmpty(jwtToken))
            {
                return default(T);
            }

            // Set Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                using var response = client.GetAsync(url).Result;

                // Check if access is forbidden, return default if unauthorized
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return default(T);
                }

                response.EnsureSuccessStatusCode(); // Throws if not successful

                return response.Content.ReadFromJsonAsync<T>().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return default(T); // Return default value in case of an error
            }
        }

        // Main action method
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetInt32("RoleId");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Redirect to login page if UserId is not found in session
                return RedirectToAction("Index", "Home");
            }

            if (userRole != 1 && userRole != 2)
            {
                return RedirectToAction("Index", "Home");
            }

            var userdata = GetDataFromApi<List<Account>>("https://be-green.chunchun.io.vn/api/User/GetAllEmployees");

            // Check if userdata is null, meaning there was an error or forbidden access
            if (userdata == null)
            {
                return RedirectToAction("Error", "Home"); // Redirect to error page if no data
            }

            return View(userdata); // Otherwise, return the view with userdata
        }

        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateEmployee(Employee model)
        {
            // Lấy thông tin RoleId từ Session
            var userRole = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Chỉ cho phép RoleId là 1 hoặc 2 truy cập
            if (userRole != 1 && userRole != 2)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!Regex.IsMatch(model.Password, @"^(?=.*[A-Z])(?=.*[\W_]).{6,}$"))
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự, bao gồm 1 chữ cái viết hoa và 1 ký tự đặc biệt.");
                return View(model);
            }
            // URL API để thêm nhân viên
            string apiUrl = "https://be-green.chunchun.io.vn/api/User/AddEmployee";

            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Chuẩn bị dữ liệu gửi lên API
                    var requestData = new
                    {
                        model.FirstName,
                        model.LastName,
                        model.Email,
                        model.Password,
                        model.PhoneNumber,
                        model.Address,
                        model.DateOfBirth,
                        model.Gender
                    };

                    // Gửi yêu cầu POST đến API
                    var response = await client.PostAsJsonAsync(apiUrl, requestData);

                    // Xử lý trường hợp API trả về lỗi
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Error Response: {responseContent}");

                        // Xác định lỗi cụ thể dựa vào nội dung phản hồi

                        ModelState.AddModelError("Email", "Email đã tồn tại hoặc không đúng định dạng.");


                        ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại.");


                        return View(model);
                    }

                    // Xử lý thành công
                    TempData["SuccessMessage"] = "Tạo nhân viên thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log lỗi và chuyển hướng tới trang lỗi
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    return RedirectToAction("Error", "Home");
                }
            }
        }



        [HttpPost("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {

            Console.WriteLine($"Received id: {id}");
            string apiUrl = $"https://be-green.chunchun.io.vn/api/User/DeleteUser/{id}";

            try
            {
                var client = _clientFactory.CreateClient();


                // Gửi yêu cầu DELETE thay vì POST
                var response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xoá nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error Response: {errorMessage}"); // Log lỗi từ API
                    TempData["ErrorMessage"] = $"Failed to delete user: {errorMessage}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        public IActionResult BlogManagement()
        {
            return View();
        }


        [HttpGet]
        public IActionResult UpdateEmployee(int id)
        {
            var userRole = HttpContext.Session.GetInt32("RoleId");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Redirect to login page if UserId is not found in session
                return RedirectToAction("Index", "Home");
            }

            if (userRole != 1 && userRole != 2)
            {
                return RedirectToAction("Index", "Home");
            }
            // Fetch user data from the API asynchronously
            var user = GetDataFromApi<Account>($"https://be-green.chunchun.io.vn/api/User/GetUserById/{id}");

            // Check if user data is null and redirect to an error page if not found
            if (user == null)
            {
                return RedirectToAction("Error", "Home"); // Redirect if the user is not found
            }

            // Pass the user to the view
            return View(user);
        }
        [HttpPost("BlockEmployee/{id}")]
        public async Task<IActionResult> BlockEmployee(int id)
        {
            Console.WriteLine($"Received id: {id}");
            string apiUrl = $"https://be-green.chunchun.io.vn/api/User/BlockUser/{id}";

            try
            {
                var client = _clientFactory.CreateClient();


                var response = await client.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Chặn nhân viên thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to block user: {errorMessage}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost("UnBlockEmployee/{id}")]
        public async Task<IActionResult> UnBlockEmployee(int id)
        {
            Console.WriteLine($"Received id: {id}"); // Log nhận ID từ request

            string apiUrl = $"https://be-green.chunchun.io.vn/api/User/UnBlockUser/{id}";

            try
            {
                // Tạo HttpClient từ _clientFactory
                var client = _clientFactory.CreateClient();
                Console.WriteLine($"Sending POST request to API: {apiUrl}");

                // Gửi POST request đến API
                var response = await client.PostAsync(apiUrl, null);

                // Kiểm tra trạng thái phản hồi
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("API call succeeded.");
                    TempData["SuccessMessage"] = "Bỏ chặn nhân viên thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Đọc thông báo lỗi từ API
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API call failed with error: {errorMessage}");
                    TempData["ErrorMessage"] = $"Failed to Unblock user: {errorMessage}";
                    return RedirectToAction("Index");
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Lỗi liên quan đến HTTP
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
                TempData["ErrorMessage"] = "Network error occurred. Please try again.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Lỗi chung
                Console.WriteLine($"Internal server error: {ex.Message}");
                TempData["ErrorMessage"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
