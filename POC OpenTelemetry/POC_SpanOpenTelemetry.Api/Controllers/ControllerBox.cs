using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POC_SpanOpenTelemetry.Api.Models;

namespace POC_SpanOpenTelemetry.Api.Controllers;

public class ControllerBox(ILogger<ControllerBox> logger) : ControllerBase
{
    private readonly ILogger<ControllerBox> _logger = logger;


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public ActionResult<ErrorViewModel> Error()
    {
        return (new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}