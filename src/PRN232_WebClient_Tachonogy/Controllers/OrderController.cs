using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    public class OrderController(IOrderService orderService, ILogger<OrderController> logger) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var filter = new OrderFilterDto { PageIndex = page, PageSize = pageSize };
            var result = await orderService.GetOrdersAsync(filter, cancellationToken);

            if (result.Success)
                return View(result.Data);

            ViewBag.Error = result.Error?.Message;
            return View(new PagedResult<OrderDto>([], 0, 0, 0));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var result = await orderService.GetByIdAsync(id, cancellationToken);
            if (!result.Success || result.Data == null)
                return NotFound();

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
        {
            new CreateOrderItemRequest()
        }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await orderService.CreateAsync(dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error creating order");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Order created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var order = await orderService.GetByIdAsync(id, cancellationToken);
            if (!order.Success || order.Data == null)
                return NotFound();

            var dto = new UpdateOrderRequest
            {
                Id = order.Data.Id,
                Status = order.Data.Status switch
                {
                    "Pending" => 0,
                    "Paid" => 1,
                    "Cancelled" => 2,
                    "Completed" => 3,
                    _ => null
                }
            };

            ViewBag.StatusList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Pending", Value = "0" },
        new SelectListItem { Text = "Paid", Value = "1" },
        new SelectListItem { Text = "Cancelled", Value = "2" },
        new SelectListItem { Text = "Completed", Value = "3" }
    };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateOrderRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await orderService.UpdateAsync(dto.Id, dto, cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error updating order");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Order updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await orderService.DeleteAsync(id, cancellationToken);
            if (!result.Success)
                TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting order";
            else
                TempData["SuccessMessage"] = "Order deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}