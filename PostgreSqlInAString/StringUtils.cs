using System.Collections.Generic;
using System.Text;
using System;

namespace PostgreSqlInAString {
    // Current text editor colors can't be read due to access violation error while reading protected memory. It succeeds in some cases but some tokens spontaneously change to light themed colors (e.g. black operators/punctuation) while in dark mode.
    // System.AccessViolationException: 'Attempted to read or write protected memory. This is often an indication that other memory is corrupt.'
    internal static class StringUtils {
        internal static StringType CategorizeString(string text, out int openingLength, out int endingLength) {
            StringType stringType = StringType.Unknown;
            bool expectRawString = false;
            openingLength = 0;
            endingLength = 0;
            for (int i = 0; i < text.Length; i++) {
                char character = text[i];
                switch (character) {
                    case '"':
                        openingLength = i + 1;
                        if(!stringType.HasFlag(StringType.Verbatim)) {
                            for (; openingLength < text.Length && text[openingLength] == '"'; openingLength++)
                                ;
                            endingLength = openingLength - i;
                            if (endingLength >= 3) {
                                return stringType | StringType.Raw;
                            }
                        }
                        if (expectRawString) {
                            return StringType.Unknown;
                        }
                        endingLength = 1;
                        return stringType | StringType.Quoted;
                    case '@':
                        if (stringType.HasFlag(StringType.Verbatim)) {
                            // Code contains errors
                            return StringType.Unknown;
                        }
                        stringType |= StringType.Verbatim;
                        break;
                    case '$':
                        if (!stringType.HasFlag(StringType.Interpolated)) {
                            stringType |= StringType.Interpolated;
                            break;
                        } 
                        
                        if (stringType.HasFlag(StringType.Verbatim)) {
                            // Code contains errors
                            return StringType.Unknown;
                        }

                        expectRawString = true;
                        break;
                    default:
                        // Code contains errors
                        return StringType.Unknown;
                }
            }

            return StringType.Unknown;
        }

        internal static string Unescape(string literalText, StringType stringType, out List<(int Index, int Length, int SkipAmount)> escapes) {
            escapes = new List<(int Index, int Length, int SkipAmount)>();
            StringBuilder stringBuilder = new StringBuilder();
            int lastIndex = 0;

            // Collection of spans tagged as escape characters could be used to check and unescape these spans instead of checking individually which characters are escaped, but the surrogate pairs would still have to be scanned for.
            for (int index = 0; index < literalText.Length - 1; index++) {
                if (!stringType.HasFlag(StringType.Raw)) {
                    char character = literalText[index];
                    if ((
                        (stringType.HasFlag(StringType.Interpolated) && (character == '{' || character == '}')) ||
                        (stringType.HasFlag(StringType.Verbatim) && character == '"')
                    ) && literalText[index + 1] == character) {
                        escapes.Add((index, 2, 1));
                        index++;
                        stringBuilder.Append(literalText, lastIndex, index - lastIndex);
                        lastIndex = index + 1;
                        continue;
                    } else if (!stringType.HasFlag(StringType.Verbatim) && character == '\\') {
                        string unescapedCharacter = ScanEscapedCharacter(literalText, index + 1, out int escapeLength);
                        if (unescapedCharacter is null) {
                            // Error, malformed escape sequence
                            return null;
                        }

                        stringBuilder.Append(literalText, lastIndex, index - lastIndex);
                        stringBuilder.Append(unescapedCharacter);
                        escapes.Add((index, escapeLength + 1, escapeLength));   // unescapedCharacter can have length of 2 in C# strings but the lexer indexes characters as if they had length of 1
                        index += escapeLength;
                        lastIndex = index + 1;
                        continue;
                    }
                } 
                
                if (char.IsSurrogatePair(literalText, index)) {
                    escapes.Add((index, 0, 1)); // This isn't an escape but it should be treated as a single character. Escapes of length 0 should be removed during span normalization
                    index++;
                }
            }

            if (lastIndex < literalText.Length) {
                stringBuilder.Append(literalText, lastIndex, literalText.Length - lastIndex);
            }
            return stringBuilder.ToString();
        }

        // For \U00HHHHHH escape sequence, characters made of UTF16 surrogate pair make strings of length 2
        private static string ScanEscapedCharacter(string text, int index, out int escapeLength) {
            escapeLength = 1;
            if (text.Length <= index) {
                return null;
            }

            char character = text[index];

            switch (character) {
                case '\'': return "'";
                case '"': return "\"";
                case '\\': return "\\";
                case '0': return "\0";
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
                case 'u': {
                    escapeLength += 4;
                    int end = index + escapeLength;
                    if (end > text.Length) {
                        return null;
                    }

                    char value = (char)0;
                    for (index++; index < end; index++) {
                        int digit = HexDigit(text[index]);
                        if (digit == -1) {
                            return null;
                        }
                        value <<= 4;
                        value |= (char)digit;
                    }
                    return value.ToString();
                }
                case 'U': {
                    escapeLength += 8;
                    int end = index + escapeLength;
                    if (end > text.Length) {
                        return null;
                    }

                    int value = 0;
                    for (index++; index < end; index++) {
                        int digit = HexDigit(text[index]);
                        if (digit == -1) {
                            return null;
                        }
                        value <<= 4;
                        value |= digit;
                    }
                    try {
                        return char.ConvertFromUtf32(value);
                    } catch (ArgumentOutOfRangeException) {
                        return null;
                    }
                }
                case 'x': {
                    char value = (char)0;
                    index++;
                    if (index >= text.Length) {
                        return null;
                    }
                    do {
                        int digit = HexDigit(text[index]);
                        if (digit == -1) {
                            if (escapeLength == 0) {
                                return null;
                            } else {
                                return value.ToString();
                            }
                        }
                        escapeLength++;
                        value <<= 4;
                        value |= (char)digit;
                        index++;
                        if (index >= text.Length) {
                            return value.ToString();
                        }
                    } while (escapeLength < 5);
                    return value.ToString();
                }
            }

            return null;
        }

        // Implementation taken from System.Text.RegularExpressions.RegexParser
        private static int HexDigit(char character) {
            int digit;

            if ((uint)(digit = character - '0') <= 9)
                return digit;

            if ((uint)(digit = character - 'a') <= 5)
                return digit + 0xa;

            if ((uint)(digit = character - 'A') <= 5)
                return digit + 0xa;

            return -1;
        }
    }
}
