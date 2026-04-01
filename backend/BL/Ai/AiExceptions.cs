namespace Conversey.BL.Ai;

public class AiExceptions : Exception
{
    
}

public class AiException(string message, Exception exception) : Exception
{
    
}