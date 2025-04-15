using SCWebService.Models;
using SCWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace SCWebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDBUserService _userService;

    public UserController(MongoDBUserService MongoDBUserService) =>
        _userService = MongoDBUserService;

    [HttpGet]
    public async Task<List<User>> Get()
    {
        var users = await _userService.GetAsync();
        return users;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<IActionResult> Post(User newuser)
    {
        await _userService.CreateAsync(newuser);

        return CreatedAtAction(nameof(Get), new { id = newuser.Id }, newuser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updateduser)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        updateduser.Id = user.Id;

        await _userService.UpdateAsync(id, updateduser);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }
}
