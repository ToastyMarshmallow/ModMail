using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModMail.LogViewer.Models;

namespace ModMail.LogViewer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    [Route("logout")]
    public IActionResult Logout()
    {
        LogoutUser();
        return Redirect("/");
    }

    public void LogoutUser()
    {
        
    }

    [Route("Account/AccessDenied")]
    public IActionResult AccountAccessDenied()
    {
        LogoutUser();
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}