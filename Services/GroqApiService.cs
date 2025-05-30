using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaAzora.Services
{
    public class GroqApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";
        private const string Model = "deepseek-r1-distill-llama-70b";

        public GroqApiService()
        {
            _httpClient = new HttpClient();
            _apiKey = "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";
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateQuestionExplanationAsync(string questionText, string[] answerOptions, string correctAnswer, string userAnswer)
        {
            try
            {
                var prompt = BuildExplanationPrompt(questionText, answerOptions, correctAnswer, userAnswer);

                var requestData = new
                {
                    model = Model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert educator who provides clear, concise explanations. Focus on being critical and informative. Provide only your final answer without showing your thinking process. Use plain text without any markdown formatting." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseObj.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentElement))
                    {
                        var fullResponse = contentElement.GetString() ?? "No explanation available.";

                        // Extract only the final message, filtering out thinking process
                        var extractedResponse = ExtractFinalMessage(fullResponse);

                        // Clean the response from markdown and escape characters
                        return CleanAIResponse(extractedResponse);
                    }
                }

                return "Unable to generate explanation at this time.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating explanation: {ex.Message}");
                return "Error generating explanation. Please try again later.";
            }
        }

        public async Task<AIInsightsResponse> GenerateInsightsAsync(TestResultSummary testSummary)
        {
            try
            {
                var prompt = BuildInsightsPrompt(testSummary);

                var requestData = new
                {
                    model = Model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert educational advisor who provides constructive feedback on test performance. Provide specific, actionable insights in plain text without any markdown formatting." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.8
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseObj.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentElement))
                    {
                        var rawResponse = contentElement.GetString() ?? "";
                        var cleanedResponse = CleanAIResponse(rawResponse);
                        return ParseInsightsResponse(cleanedResponse);
                    }
                }

                return GetDefaultInsights(testSummary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating insights: {ex.Message}");
                return GetDefaultInsights(testSummary);
            }
        }
        private string BuildExplanationPrompt(string questionText, string[] answerOptions, string correctAnswer, string userAnswer)
        {
            var optionsText = string.Join("\n", answerOptions);

            return $@"Question: {questionText}

Answer Options:
{optionsText}

Correct Answer: {correctAnswer}
Student's Answer: {userAnswer}

Please provide a concise, critical explanation that:
1. Explains why the correct answer is right
2. Briefly explains why the other options are wrong
3. Provides any key concepts the student should understand

IMPORTANT: Use plain text only. For mathematical expressions, write them clearly without LaTeX markup. 
For example, write 'x squared minus 5x plus 6' instead of '\(x^2-5x+6\)' or use simple notation like 'x^2 - 5x + 6'.
Do not use any markdown formatting, LaTeX notation, or special mathematical symbols.

Keep it under 200 words and be direct and educational.";
        }

        private string BuildInsightsPrompt(TestResultSummary summary)
        {
            var categoryPerformanceText = "";
            if (summary.CategoryPerformance.Any())
            {
                categoryPerformanceText = "\nCategory Performance:\n";
                foreach (var category in summary.CategoryPerformance.Values)
                {
                    categoryPerformanceText += $"- {category.CategoryName}: {category.CorrectAnswers}/{category.TotalQuestions} ({category.Percentage:F0}%)\n";
                }
            }

            return $@"Analyze this student's test performance:

Test: {summary.TestTitle}
Score: {summary.ScorePercentage:F1}%
Correct Answers: {summary.CorrectAnswers}/{summary.TotalQuestions}
Question Types Performance:
- Multiple Choice: {summary.MultipleChoiceCorrect}/{summary.MultipleChoiceTotal}
- Short Answer: {summary.ShortAnswerCorrect}/{summary.ShortAnswerTotal}{categoryPerformanceText}

Please provide insights in this exact format using plain text only (no markdown, no bold, no special formatting):

STRENGTHS:
[2-3 specific strengths based on performance, focusing on categories where the student performed well]

AREAS TO IMPROVE:
[2-3 specific areas that need work, highlighting weak categories and question types]

RECOMMENDATIONS:
[2-3 actionable recommendations for improvement, suggesting specific study topics based on category performance]

Keep each section concise and specific to the performance data. Use category information to provide more targeted feedback. Use plain text only.";
        }

        private AIInsightsResponse ParseInsightsResponse(string response)
        {
            var insights = new AIInsightsResponse();

            try
            {
                var sections = response.Split(new[] { "STRENGTHS:", "AREAS TO IMPROVE:", "RECOMMENDATIONS:" },
                    StringSplitOptions.RemoveEmptyEntries);

                if (sections.Length >= 2)
                {
                    insights.Strengths = sections[1].Split(new[] { "AREAS TO IMPROVE:", "RECOMMENDATIONS:" },
                        StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                }

                if (sections.Length >= 3)
                {
                    insights.AreasToImprove = sections[2].Split(new[] { "RECOMMENDATIONS:" },
                        StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                }

                if (sections.Length >= 4)
                {
                    insights.Recommendations = sections[3].Trim();
                }
            }
            catch
            {
                // If parsing fails, return the full response in strengths
                insights.Strengths = response;
            }

            return insights;
        }

        private AIInsightsResponse GetDefaultInsights(TestResultSummary summary)
        {
            var insights = new AIInsightsResponse();

            if (summary.ScorePercentage >= 80)
            {
                insights.Strengths = "Excellent overall performance with strong understanding of the material.";
                insights.AreasToImprove = "Continue practicing to maintain this high level of performance.";
                insights.Recommendations = "Challenge yourself with more advanced topics and help others learn.";
            }
            else if (summary.ScorePercentage >= 60)
            {
                insights.Strengths = "Good foundation with solid understanding of key concepts.";
                insights.AreasToImprove = "Focus on areas where you lost points to improve accuracy.";
                insights.Recommendations = "Review incorrect answers and practice similar problems.";
            }
            else
            {
                insights.Strengths = "You've identified areas for growth and learning opportunities.";
                insights.AreasToImprove = "Fundamental concepts need more attention and practice.";
                insights.Recommendations = "Review study materials thoroughly and seek additional help if needed.";
            }

            return insights;
        }

        private string ExtractFinalMessage(string fullResponse)
        {
            if (string.IsNullOrWhiteSpace(fullResponse))
                return "No explanation available.";

            // For deepseek-r1 model, the thinking process is often enclosed in <think> tags
            // or separated by specific markers. Extract only the final answer.

            // Remove thinking process if it exists
            var response = fullResponse;

            // Remove content between <think> and </think> tags
            var thinkPattern = @"<think>.*?</think>";
            response = Regex.Replace(response, thinkPattern, "",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove content between <thinking> and </thinking> tags
            var thinkingPattern = @"<thinking>.*?</thinking>";
            response = Regex.Replace(response, thinkingPattern, "",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Look for common final answer markers
            var finalAnswerMarkers = new[] { "Final answer:", "Answer:", "Explanation:", "In conclusion:", "Therefore:" };

            foreach (var marker in finalAnswerMarkers)
            {
                var markerIndex = response.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (markerIndex >= 0)
                {
                    response = response.Substring(markerIndex + marker.Length).Trim();
                    break;
                }
            }

            // Clean up the response
            response = response.Trim();

            // If response is empty or too short, return the original
            if (string.IsNullOrWhiteSpace(response) || response.Length < 10)
            {
                return fullResponse.Trim();
            }

            return response;
        }
        private string CleanAIResponse(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return rawResponse;

            var cleaned = rawResponse;

            // First, preserve LaTeX expressions by temporarily replacing them
            var latexExpressions = new List<(string original, string readable)>();
            var latexPlaceholder = "LATEX_PLACEHOLDER_";

            // Find and preserve inline LaTeX expressions \(...\) - more specific pattern
            var inlineLatexPattern = @"\\?\(([^)]*(?:[x^2\-+*/=0-9a-zA-Z\s\\]+)[^)]*)\)";
            cleaned = Regex.Replace(cleaned, inlineLatexPattern, match =>
            {
                var expression = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(expression) && IsLikelyMathExpression(expression))
                {
                    var readable = ConvertLatexToReadable(expression);
                    latexExpressions.Add((match.Value, readable));
                    return $"{latexPlaceholder}{latexExpressions.Count - 1}";
                }
                return match.Value; // Return original if not a math expression
            });

            // Find and preserve display LaTeX expressions \[...\] - more specific pattern
            var displayLatexPattern = @"\\?\[([^\]]*(?:[x^2\-+*/=0-9a-zA-Z\s\\]+)[^\]]*)\]";
            cleaned = Regex.Replace(cleaned, displayLatexPattern, match =>
            {
                var expression = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(expression) && IsLikelyMathExpression(expression))
                {
                    var readable = ConvertLatexToReadable(expression);
                    latexExpressions.Add((match.Value, readable));
                    return $"{latexPlaceholder}{latexExpressions.Count - 1}";
                }
                return match.Value; // Return original if not a math expression
            });

            // Find and preserve dollar sign LaTeX expressions $...$ - more specific pattern
            var dollarLatexPattern = @"\$([^$]*(?:[x^2\-+*/=0-9a-zA-Z\s\\]+)[^$]*)\$";
            cleaned = Regex.Replace(cleaned, dollarLatexPattern, match =>
            {
                var expression = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(expression) && IsLikelyMathExpression(expression))
                {
                    var readable = ConvertLatexToReadable(expression);
                    latexExpressions.Add((match.Value, readable));
                    return $"{latexPlaceholder}{latexExpressions.Count - 1}";
                }
                return match.Value; // Return original if not a math expression
            });

            // Remove markdown bold formatting (**text** or __text__)
            cleaned = Regex.Replace(cleaned, @"\*\*(.*?)\*\*", "$1", RegexOptions.Singleline);
            cleaned = Regex.Replace(cleaned, @"__(.*?)__", "$1", RegexOptions.Singleline);

            // Remove markdown italic formatting (*text* or _text_)
            cleaned = Regex.Replace(cleaned, @"(?<!\*)\*(?!\*)([^*]+?)\*(?!\*)", "$1", RegexOptions.Singleline);
            cleaned = Regex.Replace(cleaned, @"(?<!_)_(?!_)([^_]+?)_(?!_)", "$1", RegexOptions.Singleline);

            // Remove markdown headers (# ## ### etc.)
            cleaned = Regex.Replace(cleaned, @"^#{1,6}\s*", "", RegexOptions.Multiline);

            // Remove markdown code blocks (```code``` or `code`)
            cleaned = Regex.Replace(cleaned, @"```[\s\S]*?```", "", RegexOptions.Singleline);
            cleaned = Regex.Replace(cleaned, @"`([^`]+?)`", "$1", RegexOptions.Singleline);

            // Remove markdown links [text](url)
            cleaned = Regex.Replace(cleaned, @"\[([^\]]+?)\]\([^\)]+?\)", "$1", RegexOptions.Singleline);

            // Remove markdown lists (- item or * item or + item)
            cleaned = Regex.Replace(cleaned, @"^[\s]*[-\*\+]\s*", "", RegexOptions.Multiline);

            // Remove numbered lists (1. item, 2. item, etc.)
            cleaned = Regex.Replace(cleaned, @"^[\s]*\d+\.\s*", "", RegexOptions.Multiline);

            // Remove escape characters (but preserve LaTeX backslashes in placeholders)
            cleaned = cleaned.Replace("\\n", "\n");
            cleaned = cleaned.Replace("\\t", "\t");
            cleaned = cleaned.Replace("\\r", "\r");
            cleaned = cleaned.Replace("\\\"", "\"");
            cleaned = cleaned.Replace("\\'", "'");

            // Remove extra whitespace and normalize line breaks
            cleaned = Regex.Replace(cleaned, @"\s+", " ", RegexOptions.Singleline);
            cleaned = Regex.Replace(cleaned, @"\n\s*\n", "\n\n", RegexOptions.Multiline);

            // Restore LaTeX expressions in human-readable format
            for (int i = 0; i < latexExpressions.Count; i++)
            {
                cleaned = cleaned.Replace($"{latexPlaceholder}{i}", latexExpressions[i].readable);
            }

            // Trim and return
            return cleaned.Trim();
        }

        private bool IsLikelyMathExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            // Check if the expression contains mathematical symbols or patterns
            var mathPatterns = new[]
            {
                @"[x-z]\^?\d*", // variables like x, y, z with optional exponents
                @"\d+[x-z]", // coefficients with variables
                @"[+\-*/=]", // mathematical operators
                @"\\[a-zA-Z]+", // LaTeX commands like \sqrt, \frac
                @"\^[\d\{]", // exponents
                @"[\(\)\[\]{}].*[\(\)\[\]{}]" // expressions with brackets
            };

            return mathPatterns.Any(pattern => Regex.IsMatch(expression, pattern));
        }

        private string ConvertLatexToReadable(string latexExpression)
        {
            if (string.IsNullOrWhiteSpace(latexExpression))
                return latexExpression;

            var readable = latexExpression;

            // Convert common LaTeX commands to readable text
            readable = Regex.Replace(readable, @"\\sqrt\{([^}]+)\}", "sqrt($1)", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\frac\{([^}]+)\}\{([^}]+)\}", "($1)/($2)", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\cdot", "*", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\times", "*", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\div", "/", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\pm", "±", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\infty", "∞", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\pi", "π", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\alpha", "α", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\beta", "β", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\gamma", "γ", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\delta", "δ", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\theta", "θ", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\lambda", "λ", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\mu", "μ", RegexOptions.IgnoreCase);
            readable = Regex.Replace(readable, @"\\sigma", "σ", RegexOptions.IgnoreCase);

            // Handle exponents - convert x^2 or x^{2} to x²
            readable = Regex.Replace(readable, @"([a-zA-Z0-9])\^\{?([0-9])\}?", match =>
            {
                var baseVar = match.Groups[1].Value;
                var exponent = match.Groups[2].Value;
                return exponent switch
                {
                    "1" => baseVar + "¹",
                    "2" => baseVar + "²",
                    "3" => baseVar + "³",
                    "4" => baseVar + "⁴",
                    "5" => baseVar + "⁵",
                    "6" => baseVar + "⁶",
                    "7" => baseVar + "⁷",
                    "8" => baseVar + "⁸",
                    "9" => baseVar + "⁹",
                    "0" => baseVar + "⁰",
                    _ => baseVar + "^" + exponent
                };
            });

            // Remove remaining LaTeX backslashes and braces for simple expressions
            readable = Regex.Replace(readable, @"\\([a-zA-Z]+)", "$1");
            readable = readable.Replace("{", "").Replace("}", "");

            // Clean up extra spaces
            readable = Regex.Replace(readable, @"\s+", " ").Trim();

            return readable;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class TestResultSummary
    {
        public string TestTitle { get; set; } = string.Empty;
        public float ScorePercentage { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int MultipleChoiceCorrect { get; set; }
        public int MultipleChoiceTotal { get; set; }
        public int ShortAnswerCorrect { get; set; }
        public int ShortAnswerTotal { get; set; }
        public Dictionary<string, CategoryPerformance> CategoryPerformance { get; set; } = new();
    }

    public class CategoryPerformance
    {
        public string CategoryName { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage => TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
    }

    public class AIInsightsResponse
    {
        public string Strengths { get; set; } = string.Empty;
        public string AreasToImprove { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
    }
}