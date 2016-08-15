using System;
using System.Collections.Generic;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Various string utility functions.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Computes the Levenshtein Distance between two strings.
        /// This is an altered version of a script which can be found here: http://www.dotnetperls.com/levenshtein
        /// <param name="firstString">First string to compare</param>
        /// <param name="secondString">Second string to compare</param>
        /// <param name="caseSensitive">Whether the algorithm should preserve the casing of the two strings</param>
        /// </summary>
        public static int LevenshteinDistance(string firstString, string secondString, bool caseSensitive = false)
        {
            SmartLogger.Log(DebugFlags.StringUtilities, "\"" + firstString + "\" : \"" + secondString + "\"");

            int firstStringLength = firstString.Length;
            int secondStringLength = secondString.Length;

            // Initialize a 2D array of distances, where distances[i, j] is the Levenshtein Distance between the substring of 
            // the first i characters of firstString and the substring of the first j characters of secondString.
            // Trivially then, distances[i, 0] = i and distances[0, j] = j.
            var distances = new int[firstStringLength + 1, secondStringLength + 1];
            for (int i = 0; i <= firstStringLength; ++i)
            {
                distances[i, 0] = i;
            }
            for (int j = 0; j <= secondStringLength; ++j)
            {
                distances[0, j] = j;
            }

            if (!caseSensitive)
            {
                firstString = firstString.ToLower();
                secondString = secondString.ToLower();
            }

            for (int i = 1; i <= firstStringLength; ++i)
            {
                for (int j = 1; j <= secondStringLength; ++j)
                {
                    // Either align the two current characters and take a cost of 0 if they are the same
                    // and 1 if they are different, or skip one of the characters and take a cost of 1.
                    int cost = (secondString[j - 1] == firstString[i - 1]) ? 0 : 1;
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost);
                }
            }

            return distances[firstStringLength, secondStringLength];
        }

        /// <summary>
        /// Takes an input string and removes from it any special formatting the caller specifies.
        /// </summary>
        /// <param name="input">String to modify</param>
        /// <param name="charsToRemove">Set of individual characters to remove</param>
        /// <param name="leadingCharsForWordsToRemove">Set of leading characters for words to remove</param>
        /// <param name="surroundingCharsForTextToRemove">Dictionary of surrounding characters for text to remove,
        /// where the leading character is the key and the ending character is the value</param>
        /// <param name="removeAllNonAlphanumericOrWhitespaceChars">Whether all characters that are not alphanumeric
        /// and not whitespace should be removed</param>
        /// <param name="removeExtraWhitespace">Whether whitespace should be trimmed to a single character
        /// of whitespace and surrounding whitespace should be removed</param>
        /// <returns>The input string stripped of any specified special formatting</returns>
        public static string TrimSpecialFormatting(string input, HashSet<char> charsToRemove,
            HashSet<char> leadingCharsForWordsToRemove, Dictionary<char, char> surroundingCharsForTextToRemove,
            bool removeAllNonAlphanumericOrWhitespaceChars = true, bool removeExtraWhitespace = true)
        {
            string output = "";

            if (input.Length > 0)
            {
                int startIndex = 0;
                int endIndex = input.Length - 1;
                if (removeExtraWhitespace)
                {
                    // Skip whitespace at the beginning and end.
                    while (startIndex < input.Length && char.IsWhiteSpace(input[startIndex]))
                    {
                        startIndex++;
                    }
                    while (endIndex >= 0 && char.IsWhiteSpace(input[endIndex]))
                    {
                        endIndex--;
                    }
                }

                bool currentlyIgnoringWhitespace = false;
                bool currentlyInWordToRemove = false;
                const char wordSeparatorChar = ' ';
                char lastAddedChar = wordSeparatorChar;
                bool currentlyInTextToRemove = false;
                char endCharForCurrentTextToRemove = '\0';

                for (int i = startIndex; i <= endIndex; ++i)
                {
                    // Ignore all characters as long as we are still within text to remove.
                    if (currentlyInTextToRemove)
                    {
                        if (input[i] == endCharForCurrentTextToRemove)
                        {
                            currentlyInTextToRemove = false;
                        }
                    }
                    // Otherwise we might still add this character.
                    else
                    {
                        // If we were within a word to remove and the current character is a word separator character,
                        // then we are no longer within a word to remove.
                        if (currentlyInWordToRemove && input[i] == wordSeparatorChar)
                        {
                            currentlyInWordToRemove = false;
                        }

                        // If we are not within a word to remove and either we are not ignoring whitespace 
                        // or this character is not whitespace, we might still add this character.
                        if (!currentlyInWordToRemove && (!currentlyIgnoringWhitespace || !char.IsWhiteSpace(input[i])))
                        {
                            // If this character is the leading character for text to remove, then we are now within text to remove.
                            if (surroundingCharsForTextToRemove.ContainsKey(input[i]))
                            {
                                currentlyInTextToRemove = true;
                                endCharForCurrentTextToRemove = surroundingCharsForTextToRemove[input[i]];
                            }
                            // If this character is both the first character in a word and the leading character
                            // for a word to remove, then we are now within a word to remove.
                            else if (leadingCharsForWordsToRemove.Contains(input[i]) && (i == 0 || lastAddedChar == wordSeparatorChar))
                            {
                                currentlyInWordToRemove = true;
                            }
                            // If this character isn't one of the given characters to remove, we might still add this character.
                            else if (!charsToRemove.Contains(input[i])
                                && (!removeAllNonAlphanumericOrWhitespaceChars || char.IsLetterOrDigit(input[i]) || char.IsWhiteSpace(input[i])))
                            {
                                // If we are not ignoring whitespace or this character is not whitespace, add this character.
                                if (!currentlyIgnoringWhitespace || !char.IsWhiteSpace(input[i]))
                                {
                                    output += input[i];
                                    lastAddedChar = input[i];
                                }

                                // Determine if we should start ignoring whitespace or stop ignoring whitespace.
                                if (!currentlyIgnoringWhitespace && char.IsWhiteSpace(input[i]) && removeExtraWhitespace)
                                {
                                    currentlyIgnoringWhitespace = true;
                                }
                                else if (currentlyIgnoringWhitespace && !char.IsWhiteSpace(input[i]))
                                {
                                    currentlyIgnoringWhitespace = false;
                                }
                            }
                        }
                    }
                }
            }

            SmartLogger.Log(DebugFlags.StringUtilities, "Original String: " + input);
            SmartLogger.Log(DebugFlags.StringUtilities, "Trimmed String: " + output);
            return output;
        }
    }
}
