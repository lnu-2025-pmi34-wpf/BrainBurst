namespace BrainBurst.BLL.Interfaces.Abstractions;

using BrainBurst.BLL.DTO;

public interface IQuizGenerator
{
    // повертає пари (question, answer) з тексту
    Task<IReadOnlyList<(string Question, string Answer, IReadOnlyList<string> Tags)>> GenerateFromTextAsync(
        string text, CancellationToken ct);
}