using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace GreenGardenClient.Controllers.AdminController
{

    public class UserManagementController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public UserManagementController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private T GetDataFromApi<T>(string url)
        {
            var client = _clientFactory.CreateClient(); // Create an HttpClient instance
            using var response = client.GetAsync(url).Result; // Call GetAsync on the client
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<T>().Result;
        }


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
            var client = _clientFactory.CreateClient();
            var jwtToken = Request.Cookies["JWTToken"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            List<Account> userdata = GetDataFromApi<List<Account>>("http://103.20.97.182:5124/api/User/GetAllCustomers");
            return View(userdata);
        }
        [HttpPost("BlockUser/{id}")]
        public async Task<IActionResult> BlockUser(int id)
        {

            Console.WriteLine($"Received id: {id}");
            string apiUrl = $"http://103.20.97.182:5124/api/User/BlockUser/{id}";

            try
            {
                var client = _clientFactory.CreateClient();


                var response = await client.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Chặn người dùng thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi mở khoá người dùng";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost("UnBlockUser/{id}")]
        public async Task<IActionResult> UnBlockUser(int id)
        {
            Console.WriteLine($"Received id: {id}"); // Log nhận ID từ request

            string apiUrl = $"http://103.20.97.182:5124/api/User/UnBlockUser/{id}";

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
                    TempData["SuccessMessage"] = "Mở khoá người dùng thành công";
                    Console.WriteLine(TempData["SuccessMessage"]); // Log giá trị để kiểm tra
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

        [HttpGet]
        public IActionResult UpdateUser(int id)
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
            var user = GetDataFromApi<Account>($"http://103.20.97.182:5124/api/User/GetUserById/{id}");

            // Check if user data is null and redirect to an error page if not found
            if (user == null)
            {
                return RedirectToAction("Error", "Home"); // Redirect if the user is not found
            }

            // Pass the user to the view
            return View(user);
        }

    }
}
