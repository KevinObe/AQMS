namespace AQMS.Worker.Dtos;

//worker-lokale Result-Reporting DTO an POST /api/commands/result;
//property-namen identisch zur Web-DTO, sonst serialisiert PostAsJsonAsync falsch;
internal class CommandResultDto
{
    public long CommandId { get; set; }

    public bool Success { get; set; }

    public string? ResultMessage { get; set; }
}