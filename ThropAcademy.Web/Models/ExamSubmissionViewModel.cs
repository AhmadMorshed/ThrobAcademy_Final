public class ExamSubmissionViewModel
{
    public int ExamRequestId { get; set; }
    public int CourseId { get; set; }
    public List<QuestionAnswerModel> Answers { get; set; }
}

public class QuestionAnswerModel
{
    public int QuestionId { get; set; }
    public string SelectedOption { get; set; } // يحمل النص المختار (True/False أو نص الخيار)
}