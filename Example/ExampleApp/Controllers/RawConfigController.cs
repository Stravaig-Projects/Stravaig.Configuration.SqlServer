using ExampleApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApp.Controllers;

public class RawConfigController : Controller
{
    private readonly IConfigurationRoot _configRoot;

    public RawConfigController(IConfigurationRoot configRoot)
    {
        _configRoot = configRoot;
    }
    // GET
    public IActionResult Index()
    {
        var model = new RawConfigModel()
        {
            Providers = _configRoot.Providers.Select(p => p.ToString() ?? "*** Unknown ***").ToArray(),
            Values = _configRoot.AsEnumerable().OrderBy(kvp => kvp.Key).ToArray()
        };       
        return View(model);
    }
}