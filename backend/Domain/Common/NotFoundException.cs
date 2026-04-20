namespace Conversey.BL.Domain.Common;

public class NotFoundException(string what) : Exception($"{what} was not found.");