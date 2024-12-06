﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GreenGardenClient.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;  // For image resizing
using System.Net.Http.Headers;
namespace GreenGardenClient.Controllers.AdminController
{

    public class GearManagementController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        [BindProperty]
        public IFormFile PictureUrl { get; set; }

        // Constructor to inject IHttpClientFactory
        public GearManagementController(IHttpClientFactory clientFactory)
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
            var gear = await GetDataFromApiAsync<List<GearVM>>("https://be_green.chunchun.io.vn/api/CampingGear/GetAllCampingGears");
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
            ViewBag.CampingGear = gear;


            return View("Index");
        }
        [HttpGet("GearDetail")]
        public async Task<IActionResult> GearDetail(int gearId)
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
            var apiUrl = $"https://be_green.chunchun.io.vn/api/CampingGear/GetCampingGearDetail?id={gearId}";

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
                var gear = JsonConvert.DeserializeObject<GearDetailVM>(content);

                // Xác nhận dữ liệu từ API
                if (gear == null)
                {
                    TempData["ErrorMessage"] = "Invalid data received from API!";
                    return RedirectToAction("Error");
                }

                // Gọi API lấy danh sách thể loại thiết bị
                var campingCategories = await GetDataFromApiAsync<List<GearCategoryVM>>("https://be_green.chunchun.io.vn/api/CampingGear/GetAllCampingGearCategories");

                // Gán dữ liệu vào ViewBag để truyền sang View
                ViewBag.CampingGear = gear;
                ViewBag.CampingCategories = campingCategories;

                // Trả về View với dữ liệu
                return View("GearDetail", gear);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error connecting to API: {ex.Message}";
                return RedirectToAction("Error");
            }
            catch (System.Text.Json.JsonException ex)
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
        public async Task<IActionResult> UpdateGear(UpdateGearVM model, IFormFile PictureUrl, string CurrentPictureUrl)
        {



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
                                PublicId = "Gear/" + Path.GetFileNameWithoutExtension(fileName),
                                Folder = "Gear"
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
                            PublicId = "Gear/" + Path.GetFileNameWithoutExtension(fileName),
                            Folder = "Gear"
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
            string apiUrl = "https://be_green.chunchun.io.vn/api/CampingGear/UpdateCampingGear";
            try
            {

                // Gọi API với phương thức PUT
                var response = await _clientFactory.CreateClient().PutAsJsonAsync(apiUrl, model);


                if (response.IsSuccessStatusCode)
                {
                    TempData["NotificationSuccess"] = "Thiết bị đã được thay đổi thành công!";
                    // Nếu thành công, chuyển hướng về trang Index
                    return RedirectToAction("GearDetail", new { gearId = model.GearId });
                }
                else
                {
                    // Thêm lỗi nếu API không trả về thành công
                    ModelState.AddModelError("", "Failed to update event.");
                    return View(model); // Hiển thị lại form để người dùng biết lỗi
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
        public async Task<IActionResult> ChangeStatus(int gearId)
        {
            string apiUrl = $"https://be_green.chunchun.io.vn/api/CampingGear/ChangeGearStatus?gearId={gearId}";

            try
            {
                // Gọi API để thay đổi trạng thái thiết bị
                var response = await _clientFactory.CreateClient().PutAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    // Nếu thành công, chuyển hướng về trang chi tiết thiết bị
                    TempData["NotificationSuccess"] = "Trạng thái thiết bị đã được thay đổi thành công!";
                    return RedirectToAction("GearDetail", new { gearId });
                }
                else
                {
                    // Thêm lỗi nếu API không trả về thành công
                    TempData["NotificationError"] = "Không thể thay đổi trạng thái thiết bị.";
                    return RedirectToAction("GearDetail", new { gearId });
                }
            }
            catch (Exception ex)
            {
                // Ghi log và hiển thị thông báo lỗi chung
                TempData["Notification"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("GearDetail", new { gearId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreateGear()
        {
            var campingCategories = await GetDataFromApiAsync<List<GearCategoryVM>>("https://be_green.chunchun.io.vn/api/CampingGear/GetAllCampingGearCategories");
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
            // Gán lại danh sách danh mục cho ViewBag
            ViewBag.CampingCategories = campingCategories;

            return View(new AddGearVM());
        }



        [HttpPost]
        public async Task<IActionResult> CreateGear(AddGearVM model, IFormFile PictureUrl)
        {
            var campingCategories = await GetDataFromApiAsync<List<GearCategoryVM>>("https://be_green.chunchun.io.vn/api/CampingGear/GetAllCampingGearCategories");

            // Gán lại danh sách danh mục cho ViewBag
            ViewBag.CampingCategories = campingCategories;
            // Set default values for CreatedAt and Status
            model.CreatedAt = DateTime.Now;
            model.Status = model.Status ?? true;  // Set to true if not provided
            ViewBag.GearCategoryId = model.GearCategoryId;


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
                                PublicId = "Gear/" + Path.GetFileNameWithoutExtension(fileName),
                                Folder = "Gear"
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
                            PublicId = "Gear/" + Path.GetFileNameWithoutExtension(fileName),
                            Folder = "Gear"
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

            // Prepare request data for API
            string apiUrl = "https://be_green.chunchun.io.vn/api/CampingGear/AddCampingGear";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var requestData = new
                    {
                        GearName = model.GearName,
                        Description = model.Description,
                        ImgUrl = model.ImgUrl,
                        RentalPrice = model.RentalPrice,
                        GearCategoryId = model.GearCategoryId,
                        QuantityAvailable = model.QuantityAvailable,
                        Status = model.Status,
                        CreatedAt = model.CreatedAt
                    };

                    // Send POST request
                    var response = await client.PostAsJsonAsync(apiUrl, requestData);

                    ViewBag.Gear = model;
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["NotificationSuccess"] = "Thiết bị đã được thêm thành công.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Không thể thêm thiết bị. Vui lòng thử lại.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi kết nối API: {ex.Message}");
                    return View(model);
                }
            }
        }

    }

}


