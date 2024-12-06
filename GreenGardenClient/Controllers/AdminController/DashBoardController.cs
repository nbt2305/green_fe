using GreenGardenClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace GreenGardenClient.Controllers.AdminController
{
    public class DashBoardController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public DashBoardController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }


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
        public IActionResult Index(string datetime)
        {
            try
            {
                if (HttpContext.Session.GetInt32("RoleId") != null && (HttpContext.Session.GetInt32("RoleId").Value == 1 || HttpContext.Session.GetInt32("RoleId").Value == 2))
                {
                    if (datetime == null)
                    {
                        datetime = "0";
                    }


                    ProfitVM profitVM = GetDataFromApi<ProfitVM>($"http://103.20.97.182:5124/api/DashBoard/GetProfit/{datetime}");
                    List<Account> userdata = GetDataFromApi<List<Account>>("http://103.20.97.182:5124/api/DashBoard/GetListCustomer\r\n");
                    List<EventVM> events = new List<EventVM>();

                    // Fetch events from the API
                    var allEvents = GetDataFromApi<List<EventVM>>("http://103.20.97.182:5124/api/Event/GetAllEvents");

                    // Ensure allEvents is not null before filtering
                    if (allEvents != null)
                    {
                        events = allEvents
                            .Where(s => s.EventDate.Value.ToString("yyyy/MM") == DateTime.Now.ToString("yyyy/MM"))
                            .ToList();
                    }

                    // Ensure events is initialized to an empty list if no matching events are found
                    if (events == null || events.Count == 0)
                    {
                        events = new List<EventVM>();
                    }


                    ViewBag.datetime = datetime;
                    ViewBag.listuser = userdata;
                    ViewBag.listevent = events;

                    return View(profitVM);
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "OrderManagement");
            }
        }
    }
}
