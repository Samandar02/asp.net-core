using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace Common.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class Register : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration configuration;

    public Register(UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    SignInManager<IdentityUser> signInManager,
    IConfiguration conf
    )
   {
     _userManager = userManager;
     _roleManager= roleManager;
     _signInManager = signInManager;
     configuration = conf;
   }
    [HttpPost]
    public async Task<ActionResult<string>> SignIn(CredentialsSignIn model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
            return BadRequest("The User has not been signed up");
        bool result = await _userManager.CheckPasswordAsync(user, model.Password);
        if(!result)
            return BadRequest("Password is not match");
        var roles = await _userManager.GetRolesAsync(user);
        List<Claim> claims = new List<Claim>(){
            new Claim("userId",user.Id),
            new Claim("userName",user.UserName)
        };
        foreach (var item in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, item));
        }
        await _signInManager.SignInAsync(user,false);
        return Ok(GenerateToken(claims));
    }
    [HttpPost]
    public async Task<ActionResult<string>> SignUp(CredentialsSignUp model)
    {
        var UserChecking = await _userManager.FindByNameAsync(model.UserName);
        if(UserChecking != null)
            return BadRequest("The User alerady has been signed up");
        IdentityUser user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.Telephone
        };
        var result = await _userManager.CreateAsync(user,model.Password);
        if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        if (!await _roleManager.RoleExistsAsync(Roles.User))
            await _roleManager.CreateAsync(new IdentityRole(Roles.User));

        if (model.isAdmin)
            await _userManager.AddToRoleAsync(user,Roles.Admin);
        else
            await _userManager.AddToRoleAsync(user,Roles.User);
            

        if(!result.Succeeded)
            return BadRequest(result.Errors);
       
       return Ok("User Created Successfully");
    }

    private string GenerateToken(List<Claim> claims)
    {
        var jwt = new JwtSecurityToken(
            issuer:configuration["Valids:Issuer"],
            audience:configuration["Valids:Audience"],
            claims:claims,
            expires:DateTime.Now.AddHours(3),
            signingCredentials:new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SecretKey"]))
                ,SecurityAlgorithms.HmacSha256
            )
        );
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

}
