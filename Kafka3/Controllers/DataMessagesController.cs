using Microsoft.AspNetCore.Mvc;
using Kafka3.Data;

namespace Kafka3.Controllers;

[ApiController]
[Route("[controller]")]
public class DataMessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DataMessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetMessages()
    {
        var messages = _context.DataMessages.ToList();
        return Ok(messages);
    }
}
