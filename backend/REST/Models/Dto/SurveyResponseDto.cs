namespace Conversey.REST.Models.Dto;

public class SurveyAnswerSubmissionRequestDto
{
    public int ProjectId { get; set; }
    public string YouthId { get; set; }
    public IReadOnlyCollection<SurveyAnswerDto> Answers { get; set; } = Array.Empty<SurveyAnswerDto>();
}

public class SurveyAnswerDto
{
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public string OpenTextValue { get; set; }
}
