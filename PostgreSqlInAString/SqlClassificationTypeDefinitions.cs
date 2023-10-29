using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PostgreSqlInAString {
    internal static class SqlClassificationTypeDefinition {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(CommentClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Comment;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(IdentifierClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Identifier;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(KeywordClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Keyword;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(NumberClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Number;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(OperatorClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Operator;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ParameterClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition Parameter;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(StringClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition String;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SystemFunctionClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition SystemFunction;
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(CommentEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition CommentEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(IdentifierEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition IdentifierEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(KeywordEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition KeywordEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(NumberEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition NumberEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(OperatorEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition OperatorEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(StringEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition StringEscape;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SystemFunctionEscapeClassificationFormatDefinition.Name)]
        [BaseDefinition("string")]
        internal static ClassificationTypeDefinition SystemFunctionEscape;
    }
}
