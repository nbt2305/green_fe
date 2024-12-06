using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GreenGardenClient.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Http.Headers;
namespace GreenGardenClient.Controllers.AdminController
{

    public class FoodAndDrinkManagementController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        [BindProperty]
        public IFormFile PictureUrl { get; set; }

        // Constructor to inject IHttpClientFactory
        public FoodAndDrinkManagementController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
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
        public async Task<IActionResult> Index()
        {
            var gear = await GetDataFromApiAsync<List<FoodAndDrinkVMNew>>("https://be-green.chunchun.io.vn/api/FoodAndDrink/GetAllFoodAndDrink");
            var userRole = HttpContext.Session.GetInt32("RoleId");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userRole != 1 && userRole != 2)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.FoodAndDrink = gear;


            return View("Index");
        }
        [HttpGet]
        public async Task<IActionResult> UpdateFoodAndDrink(int itemId)
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
            var apiUrl = $"https://be-green.chunchun.io.vn/api/FoodAndDrink/GetFoodAndDrinkDetail?itemId={itemId}";

            try
            {
                // Tạo HttpClient
                var client = _clientFactory.CreateClient();

                // Gọi API để lấy chi tiết thiết bị
                var response = await client.GetAsync(apiUrl);

                // Kiểm tra trạng thái API response
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve data from API: {response.StatusCode} - {response.ReasonPhrase}";
                    return RedirectToAction("Error");
                }

                // Parse nội dung trả về từ API
                var content = await response.Content.ReadAsStringAsync();
                var gear = JsonConvert.DeserializeObject<FoodAndDrinkVMNew>(content);

                // Xác nhận dữ liệu từ API
                if (gear == null)
                {
                    TempData["ErrorMessage"] = "Invalid data received from API!";
                    return RedirectToAction("Error");
                }

                // Gọi API lấy danh sách thể loại thiết bị
                var campingCategories = await GetDataFromApiAsync<List<CategoryVM>>("https://be-green.chunchun.io.vn/api/FoodAndDrink/GetAllFoodAndDrinkCategories");

                // Gán dữ liệu vào ViewBag để truyền sang View
                ViewBag.CampingGear = gear;
                ViewBag.Categories = campingCategories;

                // Trả về View với dữ liệu
                return View("UpdateFoodAndDrink", gear);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error connecting to API: {ex.Message}";
                return RedirectToAction("Error");
            }
            catch (JsonException ex)
            {
                TempData["ErrorMessage"] = $"Error parsing JSON response: {ex.Message}";
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateFoodAndDrink(UpdateFoodAndDrinkVM model, IFormFile PictureUrl, string CurrentPictureUrl)
        {

            //if (PictureUrl != null)
            //{
            //    // Lấy tên file từ tệp được tải lên
            //    string fileName = PictureUrl.FileName;

            //    // Sử dụng Stream để tải trực tiếp lên Cloudinary
            //    using (var stream = PictureUrl.OpenReadStream())
            //    {
            //        // Cấu hình tài khoản Cloudinary
            //        var accountVM = new AccountVM
            //        {
            //            CloudName = "dxpsghdhb", // Thay bằng giá trị thực tế
            //            ApiKey = "312128264571836",
            //            ApiSecret = "nU5ETmubjnFSHIcwRPIDjjjuN8Y"
            //        };

            //        // Ánh xạ từ AccountVM sang Account
            //        var account = new CloudinaryDotNet.Account(
            //            accountVM.CloudName,
            //            accountVM.ApiKey,
            //            accountVM.ApiSecret
            //        );

            //        // Tạo đối tượng Cloudinary
            //        var cloudinary = new Cloudinary(account);

            //        // Thiết lập thông số upload
            //        var uploadParams = new ImageUploadParams()
            //        {
            //            File = new FileDescription(fileName, stream), // Đặt stream và tên file
            //            PublicId = "Food/" + Path.GetFileNameWithoutExtension(fileName), // Tùy chọn đặt tên file trên Cloudinary
            //            Folder = "Food", // Thư mục trong Cloudinary (tùy chọn)
            //        };

            //        // Thực hiện upload
            //        var uploadResult = await cloudinary.UploadAsync(uploadParams);

            //        // Cập nhật đường dẫn hình ảnh từ Cloudinary vào model
            //        model.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
            //    }
            //}
            //else
            //{
            //    // Sử dụng ảnh hiện tại nếu không có ảnh mới
            //    model.ImgUrl = CurrentPictureUrl;
            //}

            if (PictureUrl != null)
            {
                // Check for valid image file
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedMimeTypes.Contains(PictureUrl.ContentType))
                {
                    return BadRequest("Invalid file type.");
                }

                // Check file size (optional)
                if (PictureUrl.Length > 10 * 1024 * 1024) // Increase the size limit to 10 MB
                {
                    // Optionally, resize the image if it's too large (resize to 1MB max, for example)
                    using (var stream = PictureUrl.OpenReadStream())
                    {
                        // Specify the ImageSharp.Image type to avoid ambiguity with MediaTypeNames.Image
                        var image = SixLabors.ImageSharp.Image.Load(stream); // Use SixLabors.ImageSharp.Image.Load

                        // Resize the image to fit within a 1MB size (you can adjust the max size as needed)
                        int maxSizeInKB = 1024; // Target size of 1MB
                        int maxWidth = 1000; // Max width
                        int maxHeight = 1000; // Max height

                        if (image.Width > maxWidth || image.Height > maxHeight)
                        {
                            image.Mutate(x => x.Resize(new SixLabors.ImageSharp.Size(maxWidth, maxHeight))); // Use SixLabors.ImageSharp.Size
                        }

                        // Save the resized image to a memory stream
                        using (var resizedStream = new MemoryStream())
                        {
                            image.SaveAsJpeg(resizedStream); // Save as JPEG or any preferred format
                            resizedStream.Seek(0, SeekOrigin.Begin);

                            // Upload to Cloudinary
                            string fileName = PictureUrl.FileName;

                            var accountVM = new AccountVM
                            {
                                CloudName = "dxpsghdhb",
                                ApiKey = "312128264571836",
                                ApiSecret = "nU5ETmubjnFSHIcwRPIDjjjuN8Y"
                            };

                            var account = new CloudinaryDotNet.Account(
                                accountVM.CloudName,
                                accountVM.ApiKey,
                                accountVM.ApiSecret
                            );

                            var cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(fileName, resizedStream),
                                PublicId = "Food/" + Path.GetFileNameWithoutExtension(fileName),
                                Folder = "Food"
                            };

                            var uploadResult = await cloudinary.UploadAsync(uploadParams);

                            // Check for upload errors
                            if (uploadResult.Error != null)
                            {
                                return BadRequest("Upload failed: " + uploadResult.Error.Message);
                            }

                            model.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
                        }
                    }
                }
                else
                {
                    // Proceed with the original upload if file size is within limit
                    string fileName = PictureUrl.FileName;

                    using (var stream = PictureUrl.OpenReadStream())
                    {
                        var accountVM = new AccountVM
                        {
                            CloudName = "dxpsghdhb",
                            ApiKey = "312128264571836",
                            ApiSecret = "nU5ETmubjnFSHIcwRPIDjjjuN8Y"
                        };

                        var account = new CloudinaryDotNet.Account(
                            accountVM.CloudName,
                            accountVM.ApiKey,
                            accountVM.ApiSecret
                        );

                        var cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(fileName, stream),
                            PublicId = "Food/" + Path.GetFileNameWithoutExtension(fileName),
                            Folder = "Food"
                        };

                        var uploadResult = await cloudinary.UploadAsync(uploadParams);

                        // Check for upload errors
                        if (uploadResult.Error != null)
                        {
                            return BadRequest("Upload failed: " + uploadResult.Error.Message);
                        }

                        model.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
                    }
                }
            }
            else
            {
                // Use the current profile picture URL if no new picture is uploaded
                model.ImgUrl = CurrentPictureUrl;
            }
            // Gọi API để cập nhật thông tin món ăn
            string apiUrl = "https://be-green.chunchun.io.vn/api/FoodAndDrink/UpdateFoodOrDrink";
            try
            {

                // Gọi API với phương thức PUT
                var response = await _clientFactory.CreateClient().PutAsJsonAsync(apiUrl, model);


                if (response.IsSuccessStatusCode)
                {
                    TempData["NotificationSuccess"] = "Đồ ăn- đồ uống đã được thay đổi thành công!";
                    // Nếu thành công, chuyển hướng về trang Index
                    return RedirectToAction("UpdateFoodAndDrink", new { itemId = model.ItemId });
                }
                else
                {
                    // Thêm lỗi nếu API không trả về thành công
                    ModelState.AddModelError("", "Failed to update event.");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Ghi log và hiển thị thông báo lỗi chung
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model); // Hiển thị lại form để người dùng biết lỗi
            }
        }




        [HttpPost] // Dùng POST cho form submit
        public async Task<IActionResult> ChangeStatus(int itemId)
        {
            string apiUrl = $"https://be-green.chunchun.io.vn/api/FoodAndDrink/ChangeFoodStatus?itemId={itemId}";

            try
            {
                // Gọi API để thay đổi trạng thái thiết bị
                var response = await _clientFactory.CreateClient().PutAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    // Nếu thành công, chuyển hướng về trang chi tiết thiết bị
                    TempData["NotificationSuccess"] = "Trạng thái đồ ăn, đồ uống đã được thay đổi thành công!";
                    return RedirectToAction("UpdateFoodAndDrink", new { itemId });
                }
                else
                {
                    // Thêm lỗi nếu API không trả về thành công
                    TempData["NotificationError"] = "Không thể thay đổi trạng thái đồ ăn.";
                    return RedirectToAction("UpdateFoodAndDrink", new { itemId });
                }
            }
            catch (Exception ex)
            {
                // Ghi log và hiển thị thông báo lỗi chung
                TempData["Notification"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("UpdateFoodAndDrink", new { itemId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreateFoodAndDrink()
        {
            var categories = await GetDataFromApiAsync<List<CategoryVM>>("https://be-green.chunchun.io.vn/api/FoodAndDrink/GetAllFoodAndDrinkCategories");

            var userRole = HttpContext.Session.GetInt32("RoleId");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userRole != 1 && userRole != 2)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = categories;


            return View(new AddFoodAndDrinkVM());

        }
        [HttpPost]
        public async Task<IActionResult> CreateFoodAndDrink(AddFoodAndDrinkVM model, IFormFile PictureUrl)
        {
            var categories = await GetDataFromApiAsync<List<CategoryVM>>("https://be-green.chunchun.io.vn/api/FoodAndDrink/GetAllFoodAndDrinkCategories");
            ViewBag.Categories = categories;
            model.CreatedAt = DateTime.Now;
            model.Status = model.Status ?? true;

            if (PictureUrl != null)
            {
                // Check for valid image file
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedMimeTypes.Contains(PictureUrl.ContentType))
                {
                    return BadRequest("Invalid file type.");
                }

                // Check file size (optional)
                if (PictureUrl.Length > 10 * 1024 * 1024) // Increase the size limit to 10 MB
                {
                    // Optionally, resize the image if it's too large (resize to 1MB max, for example)
                    using (var stream = PictureUrl.OpenReadStream())
                    {
                        // Specify the ImageSharp.Image type to avoid ambiguity with MediaTypeNames.Image
                        var image = SixLabors.ImageSharp.Image.Load(stream); // Use SixLabors.ImageSharp.Image.Load

                        // Resize the image to fit within a 1MB size (you can adjust the max size as needed)
                        int maxSizeInKB = 1024; // Target size of 1MB
                        int maxWidth = 1000; // Max width
                        int maxHeight = 1000; // Max height

                        if (image.Width > maxWidth || image.Height > maxHeight)
                        {
                            image.Mutate(x => x.Resize(new SixLabors.ImageSharp.Size(maxWidth, maxHeight))); // Use SixLabors.ImageSharp.Size
                        }

                        // Save the resized image to a memory stream
                        using (var resizedStream = new MemoryStream())
                        {
                            image.SaveAsJpeg(resizedStream); // Save as JPEG or any preferred format
                            resizedStream.Seek(0, SeekOrigin.Begin);

                            // Upload to Cloudinary
                            string fileName = PictureUrl.FileName;

                            var accountVM = new AccountVM
                            {
                                CloudName = "dxpsghdhb",
                                ApiKey = "312128264571836",
                                ApiSecret = "nU5ETmubjnFSHIcwRPIDjjjuN8Y"
                            };

                            var account = new CloudinaryDotNet.Account(
                                accountVM.CloudName,
                                accountVM.ApiKey,
                                accountVM.ApiSecret
                            );

                            var cloudinary = new Cloudinary(account);

                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(fileName, resizedStream),
                                PublicId = "Food/" + Path.GetFileNameWithoutExtension(fileName),
                                Folder = "Food"
                            };

                            var uploadResult = await cloudinary.UploadAsync(uploadParams);

                            // Check for upload errors
                            if (uploadResult.Error != null)
                            {
                                return BadRequest("Upload failed: " + uploadResult.Error.Message);
                            }

                            model.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
                        }
                    }
                }
                else
                {
                    // Proceed with the original upload if file size is within limit
                    string fileName = PictureUrl.FileName;

                    using (var stream = PictureUrl.OpenReadStream())
                    {
                        var accountVM = new AccountVM
                        {
                            CloudName = "dxpsghdhb",
                            ApiKey = "312128264571836",
                            ApiSecret = "nU5ETmubjnFSHIcwRPIDjjjuN8Y"
                        };

                        var account = new CloudinaryDotNet.Account(
                            accountVM.CloudName,
                            accountVM.ApiKey,
                            accountVM.ApiSecret
                        );

                        var cloudinary = new Cloudinary(account);

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(fileName, stream),
                            PublicId = "Food/" + Path.GetFileNameWithoutExtension(fileName),
                            Folder = "Food"
                        };

                        var uploadResult = await cloudinary.UploadAsync(uploadParams);

                        // Check for upload errors
                        if (uploadResult.Error != null)
                        {
                            return BadRequest("Upload failed: " + uploadResult.Error.Message);
                        }

                        model.ImgUrl = uploadResult.SecureUrl.AbsoluteUri;
                    }
                }
            }
            else
            {
                model.ImgUrl = "Colorful Modern Camping Club Logo.png"; // Hoặc bỏ qua giá trị ImgUrl nếu cần
                ModelState.AddModelError("PictureUrl", "File ảnh không hợp lệ hoặc không được chọn.");
            }


            // Gửi dữ liệu đến API
            string apiUrl = "https://be-green.chunchun.io.vn/api/FoodAndDrink/AddFoodOrDrink";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Dữ liệu gửi
                    var requestData = new
                    {
                        ItemName = model.ItemName,
                        Description = model.Description,
                        ImgUrl = model.ImgUrl,
                        Price = model.Price,
                        CategoryId = model.CategoryId,
                        Status = model.Status,
                        CreatedAt = model.CreatedAt
                    };


                    // Gửi POST request
                    var response = await client.PostAsJsonAsync(apiUrl, requestData);

                    // Kiểm tra phản hồi
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["NotificationSuccess"] = "Món ăn đã được thêm thành công.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Error Response: {response.StatusCode} - {responseContent}");
                        ModelState.AddModelError("", "Không thể thêm món ăn. Vui lòng thử lại.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi kết nối API.");
                    return View(model);
                }

            }
        }
    }
}




