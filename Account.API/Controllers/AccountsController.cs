using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Models;
using Account.API.Data;
using AccountModel = Shared.Models.Models.Account;

namespace Account.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountDbContext _context;

    public AccountsController(AccountDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountModel>>> GetAccounts()
    {
        return Ok(new List<AccountModel>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountModel>> GetAccount(int id)
    {
        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<AccountModel>> CreateAccount(CreateAccountRequest request)
    {
        return StatusCode(501, "This endpoint is deprecated. Use /api/users and /api/persons instead.");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, UpdateAccountRequest request)
    {
        return StatusCode(501, "This endpoint is deprecated. Use /api/users and /api/persons instead.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        return StatusCode(501, "This endpoint is deprecated. Use /api/users and /api/persons instead.");
    }
}
