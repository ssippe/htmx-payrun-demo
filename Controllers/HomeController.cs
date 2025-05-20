using Microsoft.AspNetCore.Mvc;
using payrun_htmx.Models;
using System.Diagnostics;

namespace payrun_htmx.Controllers;
/*
 problem with losing cursor position.
hx-preserve doesn't work for input=text
probably should only reload the things that need reloading (totals) not the other stuff.

- store full obj
- end point to update tax override
- end point to update earningline.
-- add earning id
 */
public class EarningsModel
{
    public decimal Rate { get; set; }
    public decimal Hours { get; set; }
    public Guid Id { get; set; }
    public decimal Total => Rate * Hours;
}

public class GridModel
{
    public List<EarningsModel> Earnings { get; set; } = new List<EarningsModel>();
    public decimal DefaultTax => Earnings.Sum(f => f.Total) * .2m;
    public decimal? OverrideTax { get; set; }
    public decimal TotalTax => OverrideTax ?? DefaultTax;
    public decimal TotalEarnings => Earnings.Sum(f => f.Total);
    public decimal NetPay => TotalEarnings - TotalTax;
    public int SaveCount { get; set; } = 0;
}

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static GridModel GridModel = new GridModel
    {
        Earnings = new List<EarningsModel>
        {
            new EarningsModel { Hours = 40, Rate = 25, Id = Guid.CreateVersion7() },
            new EarningsModel { Hours = 35, Rate = 30, Id = Guid.CreateVersion7() }
        },
    };

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(GridModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult GridCalc(GridModel gridModel)
    {
        Thread.Sleep(1000);
        gridModel.SaveCount++;
        return View("grid", gridModel);
    }

    [HttpPost]
    public IActionResult UpdateOverrideTax(decimal? overrideTax)
    {
        GridModel.OverrideTax = overrideTax;
        GridModel.SaveCount++;
        Thread.Sleep(1000);
        ViewData["ShowTotalsGridModel"] = GridModel;
        return View("taxRow", GridModel);
    }

    [HttpPost]
    public IActionResult UpdateEarningRow(EarningsModel earnings)
    {
        var e = GridModel.Earnings.Single(f => f.Id == earnings.Id);
        e.Rate = earnings.Rate;
        e.Hours = earnings.Hours;
        GridModel.SaveCount++;
        Thread.Sleep(1000);
        ViewData["ShowTotalsGridModel"] = GridModel;
        return View("earningsRow", earnings);
    }
}
