using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai;
using System.Text;
using System.Text.Json;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

internal static class AiPromptDefaults
{
    internal static string BuildModerationSystemPrompt()
    {
        return """
You are a strict content safety classifier for a youth platform. Your task is to flag ANY harmful, toxic, or unsafe content.

Analyze the text against these categories:
- sexual: sexually explicit content, sexual harassment, or sexualized language
- hate_and_discrimination: slurs, hate speech, racism, homophobia, transphobia, bigotry, or discrimination based on identity
- violence_and_threats: threats of violence, encouragement of violence, or glorification of harm
- dangerous_and_criminal_content: illegal activity, self-harm instructions, or dangerous pranks
- self_harm: content promoting or encouraging self-harm or suicide
- pii: personal identifiable information like phone numbers, addresses, or full names

Also mark hate_and_discrimination as true for: personal insults involving slurs, name-calling with protected characteristics, profanity-laced harassment, hostile derogatory language, or general offensive/crude language targeting others.

CRITICAL: Be conservative. If you are unsure whether content violates a category, mark it as violating. False positives are safer than false negatives.

Return ONLY a JSON object with this exact schema:
{"flagged":true,"categories":{"sexual":false,"hate_and_discrimination":true,"violence_and_threats":false,"dangerous_and_criminal_content":false,"self_harm":false,"pii":false}}

No markdown, no code blocks, no explanation — just the raw JSON.
""";
    }

    internal static string DescribeNudgingMode(NudgingMode nudgingMode)
    {
        return nudgingMode switch
        {
            NudgingMode.Minimal => "Minimal (accept almost any valid idea, prompt only if severely incomplete)",
            NudgingMode.Light => "Light (ask for a bit more detail if the idea is extremely brief)",
            NudgingMode.Medium => "Medium (standard: request elaboration if the idea lacks context or motivation)",
            NudgingMode.Strong => "Strong (demand clear impact, target audience, and concrete implementation)",
            NudgingMode.Deep => "Deep (strict: require extensive detail, evidence, or a robust scenario to approve)",
            _ => "Medium (standard: request elaboration if the idea lacks context or motivation)"
        };
    }

    internal static string BuildNudgingSystemPrompt(NudgingMode nudgingMode)
    {
        return $"You help youth improve the quality of their idea before publishing. Ask exactly one concrete follow-up question when the idea is too shallow, vague, or underspecified. If the idea is already acceptable for the configured nudging strength, approve it. Never invent multiple questions. Return strict JSON only with the shape {{\"isApproved\":true}} or {{\"isApproved\":false,\"question\":\"...\"}}. Nudging strength: {DescribeNudgingMode(nudgingMode)}.";
    }

    internal static string BuildNudgingUserPrompt(IdeaNudgeAssessmentRequest request)
    {
        var conversation = request.Conversation.Count == 0
            ? "(no previous nudge questions yet)"
            : string.Join("\n", request.Conversation.Select((turn, index) => $"Turn {index + 1} question: {turn.Question}\nTurn {index + 1} answer: {turn.Answer}"));

        return $$"""
Project title: {{request.ProjectTitle}}
Project description: {{request.ProjectDescription}}
Topic title: {{request.TopicTitle}}
Topic prompt/question: {{request.TopicPrompt}}

Current idea draft:
{{request.IdeaText}}

Conversation so far:
{{conversation}}

Decide whether the draft is ready. If not, ask one follow-up question that is specific to this idea and helps deepen it using the project and topic context.
""";
    }

    internal static IReadOnlyDictionary<string, string> BuildNudgingVariables(IdeaNudgeAssessmentRequest request)
    {
        var conversation = request.Conversation.Count == 0
            ? "(no previous nudge questions yet)"
            : string.Join("\n", request.Conversation.Select((turn, index) => $"Turn {index + 1} question: {turn.Question}\nTurn {index + 1} answer: {turn.Answer}"));

        return new Dictionary<string, string>
        {
            ["ProjectTitle"] = request.ProjectTitle ?? string.Empty,
            ["ProjectDescription"] = request.ProjectDescription ?? string.Empty,
            ["TopicTitle"] = request.TopicTitle ?? string.Empty,
            ["TopicPrompt"] = request.TopicPrompt ?? string.Empty,
            ["IdeaText"] = request.IdeaText ?? string.Empty,
            ["Conversation"] = conversation
        };
    }

    internal static string BuildIdeaRankingPrompt(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        var relationGoal = preferDifferent
            ? "Return ideas that take a noticeably different angle, theme, or approach than the reference idea. Include ideas with a different focus or perspective, not just extreme opposites. Skip only ideas that are nearly identical in meaning to the reference."
            : "Return ideas that share a similar theme, goal, or approach with the reference idea. Skip ideas that are clearly unrelated or focused on a completely different topic.";

        var candidates = string.Join("\n", candidateIdeas.Select((idea, index) => $"[{index}] {idea}"));

        return $$"""
Reference idea:
{{referenceIdea}}

Candidate ideas (use only these indexes):
{{candidates}}

Task:
- {{relationGoal}}
- Return up to {{limit}} indexes, ordered from best to least fitting for this relation.
- Do not invent indexes.
- Return strict JSON only with this schema:
{"rankedIndexes":[0,1,2]}
""";
    }

    internal static IReadOnlyDictionary<string, string> BuildRankingVariables(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        var relationGoal = preferDifferent
            ? "Return ideas that take a noticeably different angle, theme, or approach than the reference idea. Include ideas with a different focus or perspective, not just extreme opposites. Skip only ideas that are nearly identical in meaning to the reference."
            : "Return ideas that share a similar theme, goal, or approach with the reference idea. Skip ideas that are clearly unrelated or focused on a completely different topic.";

        var candidates = string.Join("\n", candidateIdeas.Select((idea, index) => $"[{index}] {idea}"));

        return new Dictionary<string, string>
        {
            ["ReferenceIdea"] = referenceIdea ?? string.Empty,
            ["Candidates"] = candidates,
            ["RelationGoal"] = relationGoal,
            ["Limit"] = limit.ToString()
        };
    }

    internal static string BuildCategorizationPrompt(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var indexedIdeas = string.Join("\n", ideas.Select((idea, index) => $"[{index}] {idea}"));
        var existingCategoryList = existingCategories.Count == 0
            ? "(none yet)"
            : string.Join(", ", existingCategories.Distinct(StringComparer.OrdinalIgnoreCase));
        return $$"""
Categorize each idea semantically. One idea may belong to multiple categories.

These are the existing categories already used in this topic. Reuse these exact labels whenever possible and only invent a new label if nothing fits:
{{existingCategoryList}}

Ideas:
{{indexedIdeas}}

Rules:
- Use short, human-readable category names.
- Max {{maxCategoriesPerIdea}} categories per idea.
- Prefer reusing an existing category label when it is semantically close enough.
- Avoid near-duplicate labels when an existing category already covers the same meaning.
- Do not invent idea indexes.
- Avoid creating near-duplicate labels if an existing category already fits.
- Return strict JSON only in this shape:
{"items":[{"index":0,"categories":["Category A","Category B"]}]}
""";
    }

    internal static IReadOnlyDictionary<string, string> BuildCategorizationVariables(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var indexedIdeas = string.Join("\n", ideas.Select((idea, index) => $"[{index}] {idea}"));
        var existingCategoryList = existingCategories.Count == 0
            ? "(none yet)"
            : string.Join(", ", existingCategories.Distinct(StringComparer.OrdinalIgnoreCase));

        return new Dictionary<string, string>
        {
            ["Ideas"] = indexedIdeas,
            ["ExistingCategories"] = existingCategoryList,
            ["MaxCategoriesPerIdea"] = maxCategoriesPerIdea.ToString()
        };
    }

    internal static string BuildKeyPhrasesUserPrompt(
        string transcript,
        Language language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases,
        IReadOnlyList<string> rejectedPhrases)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"From the following {language} speech transcript, extract EXACTLY up to {maxPhrases} unique, meaningful key phrases.");
        prompt.AppendLine();
        prompt.AppendLine("### CONTEXT:");
        prompt.AppendLine($"Existing concepts (do NOT repeat or paraphrase): {JsonSerializer.Serialize(existingPhrases ?? new List<string>())}");
        prompt.AppendLine($"Rejected concepts (NEVER suggest, even as synonyms): {JsonSerializer.Serialize(rejectedPhrases ?? new List<string>())}");
        prompt.AppendLine();
        prompt.AppendLine("### STRICT RULES:");
        prompt.AppendLine("1. **Role**: Act as a meeting note-taker. Convert spoken words into brief, meaningful notes.");
        prompt.AppendLine("2. **Content**: ONLY extract substantive opinions, observations, or actionable points. Skip:");
        prompt.AppendLine(@"   - All greetings: ""hallo"", ""hi"", ""hey"", ""hoi""");
        prompt.AppendLine(@"   - All filler: ""oke"", ""dus"", ""eigenlijk"", ""ik vind dat"", ""ik bedoel"", ""kijk"", ""ja"", ""nee"", ""wel"", ""even""");
        prompt.AppendLine(@"   - All conversation starters: ""Wanneer ik..."", ""Als ik..."", ""Wat als...""");
        prompt.AppendLine(@"   - All acknowledgments: ""keigoed"", ""goed"", ""fijn"", ""mooi"", ""leuk""");
        prompt.AppendLine(@"   - All speech artifacts: ""eh"", ""hmm"", ""oh"", ""ah"", ""tja""");
        prompt.AppendLine("3. **Length**: Each phrase MUST be 2-5 words (inclusive). No exceptions.");
        prompt.AppendLine("4. **Uniqueness**: Remove ALL duplicate or near-duplicate phrases (case-insensitive).");
        prompt.AppendLine("5. **Semantic Check**: If a phrase means the same as an existing or rejected concept (even with different wording), SKIP it.");
        prompt.AppendLine($"6. **Language**: Maintain the original {language} language in all phrases.");
        prompt.AppendLine("7. **New Only**: Only extract concepts NOT already in existing or rejected lists.");
        prompt.AppendLine(@"8. **Format**: Return ONLY a JSON object with format: {""phrases"": [""phrase 1"", ""phrase 2""]}");
        prompt.AppendLine("   - No markdown, no explanations, no additional fields");
        prompt.AppendLine(@"   - Empty array if no valid new concepts: {""phrases"": []}");
        prompt.AppendLine();
        prompt.AppendLine("### EXAMPLES:");
        prompt.AppendLine("Example 1 (Dutch):");
        prompt.AppendLine(@"Transcript: ""Ik vind dat de toegang tot mental health zorg echt verbeterd moet worden. Ook de wachtlijsten zijn veel te lang.""");
        prompt.AppendLine(@"Output: {""phrases"": [""Improve mental health access"", ""Reduce waiting lists""]}");
        prompt.AppendLine();
        prompt.AppendLine("Example 2 (Dutch):");
        prompt.AppendLine(@"Transcript: ""Hallo, hoe gaat het? Ik ben het helemaal eens met het vorige punt.""");
        prompt.AppendLine(@"Output: {""phrases"": []}");
        prompt.AppendLine();
        prompt.AppendLine("Example 3 (English):");
        prompt.AppendLine(@"Transcript: ""The system should support real-time collaboration. Users need to see each other's cursors.""");
        prompt.AppendLine(@"Output: {""phrases"": [""Support real-time collaboration"", ""Show user cursors""]}");
        prompt.AppendLine();
        prompt.AppendLine("### TRANSCRIPT:");
        prompt.Append(transcript);
        return prompt.ToString();
    }

    internal static string BuildTextFromBubblesUserPrompt(
        string transcript,
        IReadOnlyList<string> bubbles,
        Language language,
        IReadOnlyList<string> rejectedPhrases)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("Write the transcript and key phrases from the FIRST-PERSON user perspective. Use first-person pronouns matching the language: Dutch='ik/mijn/wij/onze', English='I/my/we/our', French='je/mon/nous/notre'. Make it sound like the user is speaking directly.");
        prompt.AppendLine();
        prompt.AppendLine("### INSTRUCTIONS:");
        prompt.AppendLine("1. Write a summary in FIRST PERSON using appropriate pronouns");
        prompt.AppendLine("2. Ensure all key phrases are incorporated naturally into the response");
        prompt.AppendLine("3. Write in complete sentences from the user's perspective");
        prompt.AppendLine("4. Maintain the original language (" + language + ")");
        prompt.AppendLine("5. Do NOT include any rejected phrases or concepts similar to them");
        prompt.AppendLine("6. Do NOT add any phrases that are not in the transcript or key phrases");
        prompt.AppendLine("7. Do NOT invent information");
        prompt.AppendLine("8. Be concise but complete");
        prompt.AppendLine("9. ALWAYS use first-person pronouns (never third-person)");
        prompt.AppendLine();
        prompt.AppendLine("### KEY PHRASES:");
        foreach (var phrase in bubbles)
            prompt.AppendLine("- " + phrase);
        prompt.AppendLine();
        prompt.AppendLine("### REJECTED PHRASES (NEVER INCLUDE):");
        if (rejectedPhrases != null && rejectedPhrases.Count > 0)
        {
            foreach (var phrase in rejectedPhrases)
                prompt.AppendLine("- " + phrase);
        }
        else
        {
            prompt.AppendLine("(none)");
        }
        prompt.AppendLine();
        prompt.AppendLine("### TRANSCRIPT:");
        prompt.Append(transcript);
        return prompt.ToString();
    }

    internal static string BuildKeyPhrasesSystemPrompt()
    {
        return "You are a professional note-taking assistant. You ALWAYS return valid JSON. " +
               "Extract concise, meaningful key phrases from spoken language in {{Language}} as if taking meeting notes. " +
               "Be precise, remove all fluff, focus on actionable content, and never include filler words or greetings.";
    }

    internal static string BuildTextFromBubblesSystemPrompt()
    {
        return "You rewrite text from the user's first-person perspective. Always use first-person pronouns matching the language (Dutch: ik/mijn/wij/onze, English: I/my/we/our, French: je/mon/nous/notre). Always respond in {{Language}}.";
    }

    internal static IReadOnlyDictionary<string, string> BuildKeyPhrasesVariables(
        string transcript,
        Language language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases,
        IReadOnlyList<string> rejectedPhrases)
    {
        return new Dictionary<string, string>
        {
            ["Transcript"] = transcript ?? string.Empty,
            ["Language"] = language.ToString(),
            ["MaxPhrases"] = maxPhrases.ToString(),
            ["ExistingPhrases"] = JsonSerializer.Serialize(existingPhrases ?? new List<string>()),
            ["RejectedPhrases"] = JsonSerializer.Serialize(rejectedPhrases ?? new List<string>())
        };
    }

    internal static IReadOnlyDictionary<string, string> BuildTextFromBubblesVariables(
        string transcript,
        IReadOnlyList<string> bubbles,
        Language language,
        IReadOnlyList<string> rejectedPhrases)
    {
        return new Dictionary<string, string>
        {
            ["Transcript"] = transcript ?? string.Empty,
            ["Bubbles"] = bubbles != null ? string.Join("\n- ", bubbles) : string.Empty,
            ["Language"] = language.ToString(),
            ["RejectedPhrases"] = rejectedPhrases != null && rejectedPhrases.Count > 0
                ? string.Join("\n- ", rejectedPhrases)
                : "(none)"
        };
    }
}
