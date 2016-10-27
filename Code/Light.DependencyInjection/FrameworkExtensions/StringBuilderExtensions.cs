using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.FrameworkExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="StringBuilder" /> class.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        ///     Appends words in a comma-separated way, inserting an and between the last and second-to-last word.
        /// </summary>
        public static StringBuilder AppendWordEnumeration<T>(this StringBuilder stringBuilder, IEnumerable<T> words, string commaSeparator = ", ", string andSeparator = " and ", bool surroundWordsWithQuotationMarks = true)
        {
            // ReSharper disable PossibleMultipleEnumeration
            stringBuilder.MustNotBeNull(nameof(stringBuilder));
            words.MustNotBeNullOrEmpty(nameof(words));

            var wordList = words.AsList();

            for (var i = 0; i < wordList.Count; i++)
            {
                if (surroundWordsWithQuotationMarks)
                    stringBuilder.Append('"');
                stringBuilder.Append(wordList[i]);
                if (surroundWordsWithQuotationMarks)
                    stringBuilder.Append('"');
                if (i < wordList.Count - 2)
                    stringBuilder.Append(commaSeparator);
                else if (i == wordList.Count - 2)
                    stringBuilder.Append(andSeparator);
            }

            return stringBuilder;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}