using Microsoft.AspNetCore.Mvc;
using ParkingLot.Services;

namespace ParkingLot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ParkingService _parkingService;

        public HomeController(ParkingService parkingService)
        {
            _parkingService = parkingService;
        }

        public async Task<IActionResult> Index()
        {
            var snapshot = await _parkingService.GetSnapshotAsync();
            return View(snapshot);
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn([FromBody] TagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.TagNumber))
                return Json(new { success = false, errorMessage = "Tag number is required." });

            var result = await _parkingService.CheckInAsync(request.TagNumber);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut([FromBody] TagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.TagNumber))
                return Json(new { success = false, errorMessage = "Tag number is required." });

            var result = await _parkingService.CheckOutAsync(request.TagNumber);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            var stats = await _parkingService.GetStatsAsync();
            return Json(stats);
        }
    }

    public class TagRequest
    {
        public string? TagNumber { get; set; }
    }
}
