namespace Conversey.BL.Ai;

public class AiException(string message, Exception exception) : Exception(message, exception)
{
    
}