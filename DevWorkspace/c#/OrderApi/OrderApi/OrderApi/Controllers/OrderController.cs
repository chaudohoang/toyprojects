using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private static List<Order> _orders = new()
        {
            new Order { Id = 1, CustomerName = "Nguyen Van A", Status = "pending" },
            new Order { Id = 2, CustomerName = "Tran Thi B",  Status = "done"    }
        };

        // GET /api/orders
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_orders);
        }

        // GET /api/orders/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST /api/orders
        [HttpPost]
        public IActionResult Create([FromBody] Order newOrder)
        {
            newOrder.Id = _orders.Max(o => o.Id) + 1;
            _orders.Add(newOrder);
            return CreatedAtAction(nameof(GetById), new { id = newOrder.Id }, newOrder);
        }

        // PUT /api/orders/1/status
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            order.Status = status;
            return Ok(order);
        }
    }
}