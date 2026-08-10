namespace AQMS.Web.Dtos;

//Data Transfer Object über das der Worker zurück meldet, wie eine Schaltung ausgegangen ist
//welcher befehl, welcher ausgang, optional eine erfolgs bzw fehlermeldung;
//Request Body für POST requests
public class CommandResultDto
{
    public long CommandId { get; set; }

    public bool Success { get; set; }

    public string? ResultMessage { get; set; }
}
