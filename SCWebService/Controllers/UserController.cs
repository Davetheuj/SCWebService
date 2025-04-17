using SCWebService.Models;
using SCWebService.Services;
using Microsoft.AspNetCore.Mvc;

namespace SCWebService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDBUserService _userService;

    public UserController(MongoDBUserService MongoDBUserService) =>
        _userService = MongoDBUserService;


    [HttpPost("/register")]
    public async Task<IActionResult> Post(User newUser)
    {
        //Check if a user already exists with the same userName
        var user = await _userService.GetAsyncUnsecured(newUser.userName);

        if (user is not null)
        {
            var result = new JsonResult("taken");
            return result;
        }

        newUser.userMMR = 800;

        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser._id }, newUser);
    }

    [HttpPost("/login")]
    public async Task<ActionResult<User>> Get(User existingUser)
    {
        var user = await _userService.GetAsyncSecure(existingUser);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost("/update_board_preset")]
    public async Task<IActionResult> UpdateBoardPreset(User updatedUser)
    {
        await _userService.UpdateAsync(updatedUser);
        return Accepted();
    }

}
