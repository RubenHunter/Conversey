namespace Conversey.BL.Subplatform.Survey;

public class YouthNotFoundException(string youthIdentifier)
    : Exception($"Youth with id {youthIdentifier} was not found.");