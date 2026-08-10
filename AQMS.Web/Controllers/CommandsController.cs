using AQMS.Web.Dtos;
using AQMS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AQMS.Web.Controllers;

//commandsController - Dieser controller spricht nicht mehr direkt mit der DB, nur noch mit dem CommandService
//2 Methoden: Get /pending und POST /result -> die im service gebauten methoden werden von hier angesteuert
//controller ist nötig, um mittels http requests übers netzwerk mit dem service zu kommunizieren, die beiden sind logisch voneinander getrennt;
//die trennung ermöglicht wiederverwendbarkeit des services und die austauschbarkeit übers netz bzw transport übers netzwerk
//konkrete aufgabe hier ist, dass der controller nun die antworten vom service korrekt übersetzt -> bsp antwort null von service methode zu GET = kein passendes Device gefunden;  

//Api Route Registrieren auf die der Controller angewandt wird;
[ApiController]
[Route("api/commands")]

public class CommandsController : ControllerBase
{
    //methode 1 GET /pending
    //worker kann so seine ausstehenden befehle abfragen
    //service response null = device existiert nicht, service response List = alle pending commands in einer liste;
    //return value übersetzung für worker response status: null = BadRequest 400; list = Ok 200;  

    //registrieren des commandservice per Konstruktor-Injection;
    private readonly CommandService _commandService;

    //Konstruktor
    public CommandsController(CommandService commandService)
    {
        _commandService = commandService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult> GetPending()
    {
        var commandServiceResult = await _commandService.GetPendingCommands();

        //wenn Ok, Result der Abfrage zurückgeben; 
        return Ok(commandServiceResult);
    }

    //methode 2 POST /result
    //service return value ist ein enum mit dem Ergebnis des commands -> Success, CommandNotFound, AlreadyProcessed;
    //return value übersetzung für worker response status: success = 200 Ok, CommandNotFound = 404, AlreadyProcessed = 409 Conflict AlreadyProcessed;

    [HttpPost("result")]
    public async Task<ActionResult> SubmitResult([FromBody] CommandResultDto dto)
    {
        var commandServiceResultEnum = await _commandService.ProcessCommandResult(dto);

        switch (commandServiceResultEnum)
        {
            case CommandResult.Success:
                return Ok();

            case CommandResult.CommandNotFound:
                return NotFound("Command not found.");

            case CommandResult.AlreadyProcessed:
                return Conflict($"Command already Processed.");
            default:
                return StatusCode(500);
        }
    }
}
