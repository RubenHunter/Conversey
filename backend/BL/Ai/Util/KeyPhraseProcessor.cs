using System.Text.Json;
using Conversey.BL.Ai.Dto;

namespace Conversey.BL.Ai;

internal static class KeyPhraseProcessor
{
    internal static IReadOnlyList<string> ParseAndClean(
        JsonElement phrasesArray,
        IReadOnlyList<string> existingPhrases,
        IReadOnlyList<string> rejectedPhrases,
        int maxPhrases,
        out IReadOnlyList<RejectedPhrase> rejectedPhrasesWithReasons)
    {
        var phrases = new List<string>();
        var rejectedList = new List<RejectedPhrase>();

        foreach (var element in phrasesArray.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var phrase = element.GetString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(phrase))
                    phrases.Add(phrase);
            }
        }

        var cleaned = new List<string>();
        var existingPhrasesSet = new HashSet<string>(existingPhrases ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
        var rejectedPhrasesSet = new HashSet<string>(rejectedPhrases ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

        foreach (var phrase in phrases)
        {
            var wordCount = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount < 2)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.WordCountTooLow));
                continue;
            }
            if (wordCount > 5)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.WordCountExceeded));
                continue;
            }

            if (existingPhrasesSet.Contains(phrase) || rejectedPhrasesSet.Contains(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateExact));
                continue;
            }

            if (existingPhrasesSet.Any(existing => IsSubset(phrase, existing) || IsSubset(existing, phrase)))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.SubsetOfExisting));
                continue;
            }

            if (existingPhrasesSet.Any(existing => JaccardSimilarity(phrase, existing) > 0.6))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateSemantic));
                continue;
            }

            if (ContainsFillerWords(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.FillerContent));
                continue;
            }

            if (IsTooGeneric(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.TooGeneric));
                continue;
            }

            cleaned.Add(phrase);
        }

        cleaned = ApplyStemmingDeduplication(cleaned, rejectedList);

        var finalPhrases = cleaned.Take(maxPhrases).ToList();

        foreach (var phrase in cleaned.Skip(maxPhrases))
            rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.TooGeneric));

        rejectedPhrasesWithReasons = rejectedList.AsReadOnly();
        return finalPhrases.AsReadOnly();
    }

    private static bool IsSubset(string phraseA, string phraseB)
    {
        var wordsA = phraseA.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var wordsB = phraseB.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (wordsA.Length >= wordsB.Length) return false;

        var setB = new HashSet<string>(wordsB, StringComparer.OrdinalIgnoreCase);
        return wordsA.All(w => setB.Contains(w));
    }

    private static double JaccardSimilarity(string phraseA, string phraseB)
    {
        var wordsA = phraseA.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var wordsB = phraseB.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var setA = new HashSet<string>(wordsA, StringComparer.OrdinalIgnoreCase);
        var setB = new HashSet<string>(wordsB, StringComparer.OrdinalIgnoreCase);

        var intersection = setA.Count(w => setB.Contains(w));
        var union = setA.Count + setB.Count - intersection;

        return union == 0 ? 0 : (double)intersection / union;
    }

    private static bool ContainsFillerWords(string phrase)
    {
        var fillerWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "enkele", "enkelei", "enige", "sommige", "verschillende",
            "hier", "daar", "dit", "dat", "deze", "die", "het", "een",
            "van", "in", "op", "te", "voor", "met", "door", "bij", "uit",
            "als", "dat", "wat", "die", "welke", "waar",
            "is", "zijn", "was", "waren", "wordt", "worden",
            "heeft", "hebben", "had", "hadden",
            "zeer", "erg", "heel", "veel", "meeste"
        };

        var words = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return false;
        var fillerCount = words.Count(w => fillerWords.Contains(w));
        return (double)fillerCount / words.Length > 0.3;
    }

    private static bool IsTooGeneric(string phrase)
    {
        var genericPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "enige voorbeelden",
            "verschillende dingen",
            "diverse zaken",
            "veel dingen",
            "dergelijke",
            "en dergelijke",
            "etc",
            "enzovoort",
            "enzovoorts"
        };

        return genericPhrases.Contains(phrase.ToLower());
    }

    private static List<string> ApplyStemmingDeduplication(List<string> phrases, List<RejectedPhrase> rejectedList)
    {
        var result = new List<string>();
        var seenStems = new HashSet<string>();

        foreach (var phrase in phrases)
        {
            var stem = StemPhrase(phrase);
            if (seenStems.Contains(stem))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateSemantic));
            }
            else
            {
                seenStems.Add(stem);
                result.Add(phrase);
            }
        }

        return result;
    }

    private static string StemPhrase(string phrase)
    {
        var words = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var stemmedWords = words.Select(StemWord).ToArray();
        return string.Join(" ", stemmedWords);
    }

    private static string StemWord(string word)
    {
        var lowerWord = word.ToLower();

        if (lowerWord.EndsWith("ing") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 3);
        if (lowerWord.EndsWith("en") && lowerWord.Length > 3)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("s") && lowerWord.Length > 3)
            return lowerWord.Substring(0, lowerWord.Length - 1);
        if (lowerWord.EndsWith("te") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("de") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("heid") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 4);
        if (lowerWord.EndsWith("atie") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 4);
        if (lowerWord.EndsWith("tion") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 4);

        return lowerWord;
    }
}
