using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Service.Interfaces.GeminiAI 
{ 
    public interface IGeminiService 
    { 
        Task<List<int>> GetSmartSelectionAsync(string userPrompt, string questionsJson); 
    } 
}