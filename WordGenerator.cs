using System;
using System.Text;

/*
 * This code is based on WordBox, which is described below
 * .NET port, Copyright (C) 2011 by Andy Sherwood
 */ 

//========================================================================================
//
// WORDBOX - Generate random pronounceable words based on trigram probabilities
//
// WordBox is Copyright (c) 1999 by Klaus Rindtorff
//
// Permission to use, copy, and distribute this software and its documentation
// for any purpose, without fee, and without a written agreement is hereby
// granted, provided that the above copyright notice and this paragraph and
// the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE AUTHOR BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT,
// SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS,
// ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF THE
// AUTHOR HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// THE AUTHOR SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS ON AN "AS IS"
// BASIS, AND THE AUTHOR HAS NO OBLIGATIONS TO PROVIDE MAINTENANCE, SUPPORT,
// UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
//
// WORDBOX generates random pronounceable words based on trigram probabilities.
// Each word is generated randomly based on the probability of individual trigrams in
// a given language. The probabilities for each language are stored in a database.
//
// Each database consists of 26 tables for the starting character 'A'..'Z'. Each table
// contains 26 rows for the middle character, and each row contains 28 entries for the
// third character 'A'..'Z' and two extra values. Each entry gives the probability for
// the occurence of the respective trigram. The first extra value gives the probability
// that the relevant character pair is a word prefix. The second extra value gives the
// probability that the character pair is a word suffix. All probabilities are relative
// and all values are normalized to be in the range 0 to 255.
//
// The trigram statistics were generated from word lists for the following languages:
//
//   ENGLISH          153418 words
//   LATIN             43530 words
//   NAMES              6333 male and female given names
//   JAPANESE NAMES    52643 names of people and places
//

namespace Wordbox
{
    public class WordGenerator
    {
        private readonly byte[] _table;
        private readonly int _minLength;
        private readonly int _maxLength;
        private readonly int _prefixSum;
        private readonly Random _rnd = new Random();

        public WordGenerator(IWordboxDictionary dictionary, int minLength, int maxLength)
        {
            _table = dictionary.ProbabilityTable;
            _minLength = minLength;
            _maxLength = maxLength;

            _prefixSum = 0;

            for (int i = 0; i < 26; i++)
                for (int j = 0; j < 26; j++)
                    _prefixSum += Probability(i, j, 26);
        }

        public string Generate()
        {
            int i, j, k, length, sum, sigma;
            var word = new StringBuilder();

            int wordLength = _rnd.Next(_maxLength - _minLength) + _minLength;

            length = 0;

            while (length < wordLength)
            {
                while (true)
                {
                    sigma = _rnd.Next(_prefixSum);
                    sum = 0;
                    i = 0;
                    while (i < 26)
                    {
                        j = 0;
                        while (j < 26)
                        {
                            sum += Probability(i, j, 26);
                            if (sum > sigma)
                            {
                                if (length++ < wordLength) word.Append((char)('A' + i));
                                if (length++ < wordLength) word.Append((char)('a' + j));
                                goto select_prefix;
                            }
                            j++;
                        }
                        i++;
                    }
                }
            select_prefix:
                while (length < wordLength)
                {
                    if (length == wordLength - 1 && length > 1) // select a suffix
                    {
                        sigma = SuffixSum(i, j);
                        if (sigma != 0) // is there a possible continuation?
                        {
                            sigma = _rnd.Next(sigma);
                            sum = 0;

                            for (k = 0; k < 26; k++)
                            {
                                // does this make an existing trigram?
                                if (Probability(i, j, k) != 0) sum += Probability(j, k, 27);
                                if (sum > sigma)
                                {
                                    word.Append((char)('a' + k));
                                    length++;
                                    goto select_next;
                                }
                            }
                        }
                    }

                    sigma = InfixSum(i, j);
                    if (sigma == 0)
                        goto select_next; // is this a dead-end?

                    sigma = _rnd.Next(sigma);
                    sum = 0;

                    for (k = 0; k < 26; k++)
                    {
                        sum += Probability(i, j, k);

                        if (sum > sigma)
                        {
                            word.Append((char)('a' + k));
                            length++;
                            i = j; j = k;
                            goto select_trigram;
                        }
                    }

                select_trigram: ;
                }
            select_next: ;
            }

            return word.ToString();
        }

        private int Probability(int i, int j, int k)
        {
            return _table[(i * 28 * 26) + (j * 28) + k];
        }

        private int InfixSum(int i, int j)
        {
            int k, sum = 0;

            for (k = 0; k < 26; k++)
                sum += Probability(i, j, k);

            return sum;
        }

        private int SuffixSum(int i, int j)
        {
            int k, sum = 0;

            for (k = 0; k < 26; k++)
            {
                if (Probability(i, j, k) != 0)
                    sum += Probability(j, k, 27);
            }

            return sum;
        }
    }
}
