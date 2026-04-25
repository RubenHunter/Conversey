namespace Conversey.BL.Ai;

public class AiException(string message, Exception exception) : Exception(message, exception)
{
    
}

public class AiRankingException(string message, Exception exception)
    : AiException(message, exception);