﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using PostgreSqlInAString.Lexer;

namespace PostgreSqlInAString {
    internal sealed class StringSqlTagger: ITagger<ClassificationTag> {
        private ITagAggregator<IClassificationTag> tagAggregator;
        private IClassificationType commentClassificationType;
        private IClassificationType identifierClassificationType;
        private IClassificationType keywordClassificationType;
        private IClassificationType numberClassificationType;
        private IClassificationType operatorClassificationType;
        private IClassificationType parameterClassificationType;
        private IClassificationType stringClassificationType;
        private IClassificationType systemFunctionClassificationType;
        private IClassificationType commentEscapeClassificationType;
        private IClassificationType identifierEscapeClassificationType;
        private IClassificationType keywordEscapeClassificationType;
        private IClassificationType numberEscapeClassificationType;
        private IClassificationType operatorEscapeClassificationType;
        private IClassificationType stringEscapeClassificationType;
        private IClassificationType systemFunctionEscapeClassificationType;
        private PostgreSqlInAStringConfiguration configuration;
        private readonly static Regex enableRuleRegex = new Regex(@"^(?:strpsql|PostgreSqlInAString)-(on|off|enable|disable)(?:$|\s+--)", RegexOptions.IgnoreCase);    // Used to match trimmed comment content
        private readonly static Regex inlineEnableRuleRegex = new Regex(@"^(?:strpsql|PostgreSqlInAString)(?:$|\s+--)", RegexOptions.IgnoreCase);    // Used to match trimmed comment content
        private readonly static Regex inlineDisableRuleRegex = new Regex(@"^(?:strpsql|PostgreSqlInAString)-ignore(?:$|\s+--)", RegexOptions.IgnoreCase);    // Used to match trimmed comment content

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        internal StringSqlTagger(IClassificationTypeRegistryService registry, ITagAggregator<IClassificationTag> tagAggregator, PostgreSqlInAStringConfiguration configuration) {
            this.commentClassificationType = registry.GetClassificationType(CommentClassificationFormatDefinition.Name);
            this.identifierClassificationType = registry.GetClassificationType(IdentifierClassificationFormatDefinition.Name);
            this.keywordClassificationType = registry.GetClassificationType(KeywordClassificationFormatDefinition.Name);
            this.numberClassificationType = registry.GetClassificationType(NumberClassificationFormatDefinition.Name);
            this.operatorClassificationType = registry.GetClassificationType(OperatorClassificationFormatDefinition.Name);
            this.parameterClassificationType = registry.GetClassificationType(ParameterClassificationFormatDefinition.Name);
            this.stringClassificationType = registry.GetClassificationType(StringClassificationFormatDefinition.Name);
            this.systemFunctionClassificationType = registry.GetClassificationType(SystemFunctionClassificationFormatDefinition.Name);
            this.commentEscapeClassificationType = registry.GetClassificationType(CommentEscapeClassificationFormatDefinition.Name);
            this.identifierEscapeClassificationType = registry.GetClassificationType(IdentifierEscapeClassificationFormatDefinition.Name);
            this.keywordEscapeClassificationType = registry.GetClassificationType(KeywordEscapeClassificationFormatDefinition.Name);
            this.numberEscapeClassificationType = registry.GetClassificationType(NumberEscapeClassificationFormatDefinition.Name);
            this.operatorEscapeClassificationType = registry.GetClassificationType(OperatorEscapeClassificationFormatDefinition.Name);
            this.stringEscapeClassificationType = registry.GetClassificationType(StringEscapeClassificationFormatDefinition.Name);
            this.systemFunctionEscapeClassificationType = registry.GetClassificationType(SystemFunctionEscapeClassificationFormatDefinition.Name);
            this.tagAggregator = tagAggregator;
            this.configuration = configuration;
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection snapshotSpanCollection) {
            if (snapshotSpanCollection.Count == 0) {
                yield break;
            }

            // snapshotSpanCollection.Count should probably never be > 1
            IEnumerable<(SnapshotSpan, StringType)> stringSpans = GetEnabledStringSpans(snapshotSpanCollection);
            foreach ((SnapshotSpan stringSpan, StringType stringType) in stringSpans) {
                string stringToTokenize = StringUtils.Unescape(stringSpan.GetText(), stringType, out List<(int Index, int Length, int SkipAmount)> escapes);
                if (stringToTokenize == null) {
                    continue;   // Found invalid escape sequence
                }

                PostgreSqlLexer lexer = new PostgreSqlLexer(CharStreams.fromString(stringToTokenize));
                IList<IToken> tokens = lexer.GetAllTokens();

                int skippedIndices = 0;
                int escapesIndex = 0;
                NormalizedSnapshotSpanCollection stringEscapeSpans = new NormalizedSnapshotSpanCollection(escapes.Where(e => e.Length > 0).Select(e => new SnapshotSpan(stringSpan.Snapshot, stringSpan.Start.Position + e.Index, e.Length)));
                foreach (IToken token in tokens) {
                    if ((token.Text?.Length ?? 0) == 0) {
                        continue;   // Lexer shouldn't produce empty string tokens, but skip those just in case
                    }

                    SqlTokenCategory tokenCategory = TokenCategorizer.Categorize(token);
                    if (tokenCategory == SqlTokenCategory.None) {
                        continue;
                    }

                    for (; escapesIndex < escapes.Count && escapes[escapesIndex].Index < token.StartIndex + skippedIndices; escapesIndex++) {
                        skippedIndices += escapes[escapesIndex].SkipAmount;
                    }
                    int tokenStart = token.StartIndex + skippedIndices;

                    int tokenLength = token.Text.Length;
                    for (; escapesIndex < escapes.Count && escapes[escapesIndex].Index < tokenStart + tokenLength; escapesIndex++) {
                        tokenLength += escapes[escapesIndex].SkipAmount;
                    }
                    skippedIndices += tokenLength - token.Text.Length;

                    NormalizedSnapshotSpanCollection stringTokenSpan = new NormalizedSnapshotSpanCollection(new SnapshotSpan(stringSpan.Snapshot, stringSpan.Start.Position + tokenStart, tokenLength));
                    IEnumerable<SnapshotSpan> stringWithoutEscapesTokenSpans = NormalizedSnapshotSpanCollection.Difference(stringTokenSpan, stringEscapeSpans).Where(s => s.Length > 0);    // Difference can return empty spans
                    NormalizedSnapshotSpanCollection stringEscapeSpansWithinTokenSpan = NormalizedSnapshotSpanCollection.Overlap(stringTokenSpan, stringEscapeSpans);   // Overlap will not return empty spans

                    if (stringEscapeSpansWithinTokenSpan.Count > 0) {
                        IClassificationType classificationType = GetStringEscapeClassificationType(tokenCategory);
                        foreach (SnapshotSpan span in stringEscapeSpansWithinTokenSpan) {
                            yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(classificationType));
                        }
                    }

                    if (stringWithoutEscapesTokenSpans.Any()) {
                        IClassificationType classificationType = GetStringClassificationType(tokenCategory);
                        foreach (SnapshotSpan span in stringWithoutEscapesTokenSpans) {
                            yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(classificationType));
                        }
                    }
                }
            }
        }

        // Yields normalized snapshot spans tagged as string or string - verbatim, that are in enabled regions (strpsql-enable) and within the passed span collection
        private IEnumerable<(SnapshotSpan, StringType)> GetEnabledStringSpans(NormalizedSnapshotSpanCollection snapshotSpanCollection) {
            IEnumerable<IMappingTagSpan<IClassificationTag>> stringTagSpans = GetStringTagSpans(snapshotSpanCollection);
            if (!stringTagSpans.Any()) {
                yield break;
            }

            IEnumerable<SnapshotSpan> enabledSpans = GetEnabledSpans(snapshotSpanCollection);
            NormalizedSnapshotSpanCollection normalizedEnabledSpans = new NormalizedSnapshotSpanCollection(enabledSpans);
            NormalizedSnapshotSpanCollection normalizedStringSpans = new NormalizedSnapshotSpanCollection();
            foreach (IMappingTagSpan<IClassificationTag> mappingTagSpan in stringTagSpans) {
                NormalizedSnapshotSpanCollection stringSpans = mappingTagSpan.Span.GetSpans(mappingTagSpan.Span.AnchorBuffer); // spans.Count should probably never be > 1
                normalizedStringSpans = NormalizedSnapshotSpanCollection.Union(normalizedStringSpans, stringSpans);
            }

            normalizedStringSpans = EnsureStringSpansAreComplete(normalizedStringSpans);

            foreach (SnapshotSpan stringSpan in normalizedStringSpans) {
                bool isInEnabledRegion = normalizedEnabledSpans.OverlapsWith(stringSpan);
                StringType type = CategorizeAndTrimStringSpans(stringSpan, isInEnabledRegion, out SnapshotSpan trimmedSpan);
                if (type == StringType.Unknown) {
                    continue;
                }

                NormalizedSnapshotSpanCollection trimmedSpanCollection = new NormalizedSnapshotSpanCollection(trimmedSpan);
                trimmedSpanCollection = NormalizedSnapshotSpanCollection.Overlap(trimmedSpanCollection, snapshotSpanCollection);
                foreach (SnapshotSpan span in trimmedSpanCollection) {
                    yield return (span, type);
                }
            }
        }

        // Often string literals like @$"stuff" get tagged as 3 separate 'string - verbatim' tag-spans i.e. @$", stuff, "
        private NormalizedSnapshotSpanCollection EnsureStringSpansAreComplete(NormalizedSnapshotSpanCollection normalizedStringSpans) {
            NormalizedSnapshotSpanCollection completeStringSpans = new NormalizedSnapshotSpanCollection(normalizedStringSpans);
            foreach (SnapshotSpan stringSpan in normalizedStringSpans) {
                ITextSnapshot snapshot = stringSpan.Snapshot;
                int position = stringSpan.Start.Position - 1;
                if (snapshot[position] == '"') {
                    NormalizedSnapshotSpanCollection span = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                    if (!(span is null)) {
                        completeStringSpans = NormalizedSnapshotSpanCollection.Union(completeStringSpans, span);
                    }
                }

                position = stringSpan.End.Position;
                if (snapshot[position] == '"') {
                    NormalizedSnapshotSpanCollection span = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                    if (!(span is null)) {
                        completeStringSpans = NormalizedSnapshotSpanCollection.Union(completeStringSpans, span);
                    }
                }
            }

            return completeStringSpans;
        }

        private StringType CategorizeAndTrimStringSpans(SnapshotSpan span, bool isInEnabledRegion, out SnapshotSpan trimmedSpan) {
            // Cut out $@" at the beginning and " at the end of the string. Some instances may not have quotation marks at start nor end if the string is interpolated and there are parameters.
            // TODO: handle C# 11 raw string literals
            StringType stringType;
            trimmedSpan = default;
            SnapshotSpan stringBeginningSpan = span;
            int start = span.Start.Position;
            int end = span.End.Position;

            if (start > 0) {
                SnapshotSpan? potentialStringBeginningSpan = FindStringBeginningSpan(span);
                if (potentialStringBeginningSpan == null) {
                    // Code contains errors, skip this span
                    return StringType.Unknown;
                }
                stringBeginningSpan = potentialStringBeginningSpan.Value;
            }

            bool? isEnabledInline = IsEnabledInline(stringBeginningSpan);
            if (isEnabledInline == false || (isEnabledInline == null && !isInEnabledRegion)) {
                // String span is not in enabled region
                return StringType.Unknown;
            }

            string spanText = stringBeginningSpan.GetText();
            stringType = StringUtils.CategorizeString(spanText, out int openingLength);

            if (stringType == StringType.Unknown) {
                // Code contains errors, skip this span
                return StringType.Unknown;
            }

            if (stringBeginningSpan == span) {
                start += openingLength;
            }

            SnapshotSpan stringEndingSpan = span;
            if (end < span.Snapshot.Length - 1) {
                SnapshotSpan? potentialStringEndingSpan = FindStringEndingSpan(span);
                if (potentialStringEndingSpan == null) {
                    // Code contains errors, skip this span
                    return StringType.Unknown;
                }
                stringEndingSpan = potentialStringEndingSpan.Value;
            }

            if (stringEndingSpan == span && stringEndingSpan.GetText()[span.Length - 1] == '"') {   // The second check is probably unnecessary, string literal wil always end with a '"'
                end -= 1;
            }

            if (end > start) {
                trimmedSpan = new SnapshotSpan(span.Snapshot, start, end - start);
                return stringType;
            }

            return StringType.Unknown;
        }

        // true if enabled inline, false if disabled inline, null if none
        private bool? IsEnabledInline(SnapshotSpan stringSpan) {
            int position = stringSpan.Start.Position - 1;
            SnapshotSpan span = new SnapshotSpan(stringSpan.Snapshot, position, 1);
            IEnumerable<IMappingSpan> commentSpans = GetCommentTagSpans(span).Select(t => t.Span);
            if (!commentSpans.Any()) {
                return null;
            }

            IMappingSpan commentMappingSpan = commentSpans.First();
            SnapshotSpan commentSpan = commentMappingSpan.GetSpans(commentMappingSpan.AnchorBuffer).First();
            SnapshotSpan? commentContentSpan = GetCommentContentSpan(commentSpan);
            if (commentContentSpan is null) {
                return null;
            }

            string ruleText = commentContentSpan.Value.GetText().Trim();
            if (inlineEnableRuleRegex.IsMatch(ruleText)) {
                return true;
            }
            if (inlineDisableRuleRegex.IsMatch(ruleText)) {
                return false;
            }
            return null;
        }

        private SnapshotSpan? FindStringBeginningSpan(SnapshotSpan span) {
            while (true) {
                SnapshotSpan? resultSpan = FindPreviousStringSpan(span);
                if (resultSpan == span) {
                    return resultSpan;
                }
                if (resultSpan == null) {
                    return null;
                }
                span = resultSpan.Value;
            }
        }

        private SnapshotSpan? FindStringEndingSpan(SnapshotSpan span) {
            while (true) {
                SnapshotSpan? resultSpan = FindNextStringSpan(span);
                if (resultSpan == span) {
                    return resultSpan;
                }
                if (resultSpan == null) {
                    return null;
                }
                span = resultSpan.Value;
            }
        }

        // Returns passed in span if given span is the beginning of a string. otherwise previous span of the same string definition
        // Given code: string x = $"aaa{b}ccc";
        // when passed the 'ccc"' span, the function will return the '$"aaa' span
        // when passed the '$"aaa' span, the function will return the same '$"aaa' span
        private SnapshotSpan? FindPreviousStringSpan(SnapshotSpan snapshotSpan) {
            // Only interpolated string literals can be spread across multiple spans(don't know about C# 11 raw string literals)
            int position = snapshotSpan.Start.Position - 1;
            ITextSnapshot snapshot = snapshotSpan.Snapshot;
            if (snapshot[position] == '}') {
                // while there are more parameters adjacent in the string i.e. @"a{1}{2}[3}b"
                while (IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                    // Find the matching start brace
                    int nesting = 1;
                    while (true) {
                        if (position <= 1) { // could be even <= 2 because interpolated string requires 2 chars at the beginning $"
                            // Code contains errors, braces don't match or there is no enough space for previous string span
                            return null;
                        }
                        position--;
                        char character = snapshot[position];
                        if (character == '{' && IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                            nesting--;
                            if (nesting == 0) {
                                break;
                            }
                        } else if (character == '}' && IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                            nesting++;
                        }
                    }

                    position--;
                    // Check if the previous character is part of a string span
                    // Also handles case with escaped end brace, e.g. string x = $"}}{1}";
                    NormalizedSnapshotSpanCollection previousSpan = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                    if (!(previousSpan is null)) {
                        // Join together neighboring string literal spans
                        while (true) {
                            position = previousSpan[0].Start - 1;
                            if (position < 0) {
                                return previousSpan[0];
                            }
                            NormalizedSnapshotSpanCollection previousPreviousSpan = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                            if (previousPreviousSpan is null || previousPreviousSpan.Count == 0) {
                                return previousSpan[0];
                            }
                            previousSpan = NormalizedSnapshotSpanCollection.Union(previousSpan, previousPreviousSpan);
                        };
                    }

                    if (snapshot[position] != '}') {
                        // String just follows some code block, e.g. {}"str".ToString();
                        // It is not possible for different string literals to immediately lead and follow a code block, i.e. "aaa"{}"bbb" is not possible without errors, so previous checks are correct
                        return snapshotSpan;
                    }

                    // Continue loop, look for another end brace, e.g. string x = $"{1}{2}";
                }
            }

            return snapshotSpan;
        }

        // Returns passed in span if given span is the ending of a string. otherwise next span of the same string definition
        // Given code: string x = $"aaa{b}ccc";
        // when passed the '$"aaa' span, the function will return the 'ccc"' span
        // when passed the 'ccc"' span, the function will return the same 'ccc"' span
        private SnapshotSpan? FindNextStringSpan(SnapshotSpan snapshotSpan) {
            // Only interpolated string literals can be spread across multiple spans(don't know about C# 11 raw string literals)
            int position = snapshotSpan.End.Position;
            ITextSnapshot snapshot = snapshotSpan.Snapshot;
            if (snapshot[position] == '{') {
                // while there are more parameters adjacent in the string i.e. @"a{1}{2}[3}b"
                while (IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                    // Find the matching end brace
                    int nesting = 1;
                    while (true) {
                        if (position >= snapshot.Length - 2) { // Text needs to have place for at least '}"'
                            // Code contains errors, braces don't match or there is no enough space for next string span
                        }
                        position++;
                        char character = snapshot[position];
                        if (character == '}' && IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                            nesting--;
                            if (nesting == 0) {
                                break;
                            }
                        } else if (character == '{' && IsCharAtPositionPartOfSpanType(snapshot, position, "punctuation")) {
                            nesting++;
                        }
                    }

                    position++;
                    // Check if the next character is part of a string span
                    // Also handles case with escaped start brace, e.g. string x = $"{1}{{";
                    NormalizedSnapshotSpanCollection nextSpan = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                    if (!(nextSpan is null)) {
                        // Join together neighboring string literal spans
                        while (true) {
                            position = nextSpan[0].End;
                            if (position >= snapshot.Length) {
                                // Code probably contains errors because code should end with a closing brace or a comma or sth, but the string could still be colored and it shouldn't be this functions' responsibility, so return the last string's span
                                return nextSpan[0];
                            }
                            NormalizedSnapshotSpanCollection nextNextSpan = GetSpanIfCharAtPositionIsPartOfString(snapshot, position);
                            if (nextNextSpan is null || nextNextSpan.Count == 0) {
                                return nextSpan[0];
                            }
                            nextSpan = NormalizedSnapshotSpanCollection.Union(nextSpan, nextNextSpan);
                        };
                    }


                    if (snapshot[position] != '{') {
                        // String literal leads before some code block, e.g. "str"{} - this is a syntax error
                        return null;
                    }

                    // Continue loop, look for another start brace, e.g. string x = $"{1}{2}";
                }
            }

            return snapshotSpan;
        }

        // Returns enabled spans within relevant region of the snapshot span collection
        private IEnumerable<SnapshotSpan> GetEnabledSpans(NormalizedSnapshotSpanCollection snapshotSpanCollection) {
            ITextSnapshot snapshot = snapshotSpanCollection.First().Snapshot;
            int minStart = snapshotSpanCollection.Min(s => s.Start.Position);
            int maxEnd = snapshotSpanCollection.Max(s => s.End.Position);
            SnapshotSpan relevantCommentsSpan = new SnapshotSpan(snapshot, 0, maxEnd);
            IEnumerable<IMappingTagSpan<IClassificationTag>> commentTagSpans = GetCommentTagSpans(relevantCommentsSpan);
            NormalizedSnapshotSpanCollection normalizedCommentSpans = new NormalizedSnapshotSpanCollection();
            foreach (IMappingTagSpan<IClassificationTag> mappingTagSpan in commentTagSpans) {
                NormalizedSnapshotSpanCollection commentSpanCollection = mappingTagSpan.Span.GetSpans(mappingTagSpan.Span.AnchorBuffer); // commentSpanCollection.Count should probably never be > 1
                normalizedCommentSpans = NormalizedSnapshotSpanCollection.Union(normalizedCommentSpans, commentSpanCollection);
            }

            int count = normalizedCommentSpans.Where(s => s.End.Position < minStart).Count();
            int? start = FindEnabledSpanStart(count, minStart, normalizedCommentSpans);

            foreach (SnapshotSpan snapshotSpan in normalizedCommentSpans.Skip(count)) {
                SnapshotSpan? commentContentSpan = GetCommentContentSpan(snapshotSpan);
                if (commentContentSpan.HasValue) {
                    Match match = enableRuleRegex.Match(commentContentSpan.Value.GetText().Trim());
                    Group group = match.Groups[1];
                    switch (group.Value) {
                        case "enable":
                        case "on":
                            if (!start.HasValue) {
                                start = snapshotSpan.End.Position;
                            }
                            break;
                        case "disable":
                        case "off":
                            if (start.HasValue) {
                                yield return new SnapshotSpan(snapshot, start.Value, snapshotSpan.End.Position - start.Value);
                                start = null;
                            }
                            break;
                    }
                }
            }

            if (start.HasValue) {
                yield return new SnapshotSpan(snapshot, start.Value, snapshotSpanCollection.Last().End.Position - start.Value);
            }
        }

        private int? FindEnabledSpanStart(int count, int minStart, NormalizedSnapshotSpanCollection commentSpans) {
            for (int index = count - 1; index >= 0; index--) {
                SnapshotSpan snapshotSpan = commentSpans[index];
                SnapshotSpan? commentContentSpan = GetCommentContentSpan(snapshotSpan);
                if (commentContentSpan.HasValue) {
                    Match match = enableRuleRegex.Match(commentContentSpan.Value.GetText().Trim());
                    Group group = match.Groups[1];
                    switch (group.Value) {
                        case "enable":
                        case "on":
                            return Math.Max(minStart, snapshotSpan.End.Position);
                        case "disable":
                        case "off":
                            return null;
                    }
                }
            }

            return configuration.Enabled ? (int?)minStart : null;
        }

        private static SnapshotSpan? GetCommentContentSpan(SnapshotSpan snapshotSpan) {
            string comment = snapshotSpan.GetText();
            if (comment.StartsWith("//")) {
                for (int i = 2; i < snapshotSpan.Length; i++) { // loop currently redundant because XML doc comments are ignored
                    if (comment[i] != '/') {
                        return new SnapshotSpan(snapshotSpan.Snapshot, snapshotSpan.Start.Position + i, snapshotSpan.Length - i);
                    }
                }
                return null;
            } else if (comment.StartsWith("/*")) {
                int? start = null;
                for (int i = 2; i < snapshotSpan.Length; i++) { // loop currently redundant because XML doc comments are ignored
                    if (comment[i] != '*') {
                        start = i;
                        break;
                    }
                }
                if (!start.HasValue) {
                    return null;
                }
                for (int i = snapshotSpan.Length - 3; i >= start; i--) {
                    if (comment[i] != '*') {
                        return new SnapshotSpan(snapshotSpan.Snapshot, snapshotSpan.Start.Position + start.Value, i - start.Value + 1);
                    }
                }
                return null;
            } else {
                // Unknown comment format
                return null;
            }
        }

        private IEnumerable<IMappingTagSpan<IClassificationTag>> GetCommentTagSpans(SnapshotSpan span) {
            return tagAggregator.GetTags(span).Where(t => t.Tag.ClassificationType.IsOfType("comment"));    // This includes single-line comments and block comments. XML doc comments are not included intentionally.
        }

        private IEnumerable<IMappingTagSpan<IClassificationTag>> GetStringTagSpans(NormalizedSnapshotSpanCollection span) {
            return tagAggregator.GetTags(span).Where(t => t.Tag.ClassificationType.IsOfType("string") || t.Tag.ClassificationType.IsOfType("string - verbatim"));
        }

        private IEnumerable<IMappingTagSpan<IClassificationTag>> GetStringTagSpans(SnapshotSpan span) {
            return tagAggregator.GetTags(span).Where(t => t.Tag.ClassificationType.IsOfType("string") || t.Tag.ClassificationType.IsOfType("string - verbatim"));
        }

        private NormalizedSnapshotSpanCollection GetSpanIfCharAtPositionIsPartOfString(ITextSnapshot snapshot, int position) {
            SnapshotSpan span = new SnapshotSpan(snapshot, position, 1);
            IMappingSpan mappingSpan = GetStringTagSpans(span).Select(t => t.Span).FirstOrDefault();
            if (mappingSpan is null) {
                return null;
            }
            return mappingSpan.GetSpans(mappingSpan.AnchorBuffer);
        }

        private bool IsCharAtPositionPartOfSpanType(ITextSnapshot snapshot, int position, string type) {
            SnapshotSpan span = new SnapshotSpan(snapshot, position, 1);
            return tagAggregator.GetTags(span).Any(t => t.Tag.ClassificationType.IsOfType(type));
        }



        private IClassificationType GetStringClassificationType(SqlTokenCategory category) {
            switch (category) {
                case SqlTokenCategory.None:
                default:
                    return null;
                case SqlTokenCategory.Comment:
                    return commentClassificationType;
                case SqlTokenCategory.Identifier:
                    return identifierClassificationType;
                case SqlTokenCategory.Keyword:
                    return keywordClassificationType;
                case SqlTokenCategory.Number:
                    return numberClassificationType;
                case SqlTokenCategory.Operator:
                    return operatorClassificationType;
                case SqlTokenCategory.Parameter:
                    return parameterClassificationType;
                case SqlTokenCategory.String:
                    return stringClassificationType;
                case SqlTokenCategory.SystemFunction:
                    return systemFunctionClassificationType;
            }
        }

        private IClassificationType GetStringEscapeClassificationType(SqlTokenCategory category) {
            switch (category) {
                case SqlTokenCategory.None:
                default:
                    return null;
                case SqlTokenCategory.Comment:
                    return commentEscapeClassificationType;
                case SqlTokenCategory.Identifier:
                    return identifierEscapeClassificationType;
                case SqlTokenCategory.Keyword:
                    return keywordEscapeClassificationType;
                case SqlTokenCategory.Number:
                    return numberEscapeClassificationType;
                case SqlTokenCategory.Operator:
                    return operatorEscapeClassificationType;
                case SqlTokenCategory.String:
                    return stringEscapeClassificationType;
                case SqlTokenCategory.SystemFunction:
                    return systemFunctionEscapeClassificationType;
            }
        }
    }
}
