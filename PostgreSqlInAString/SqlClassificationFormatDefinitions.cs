using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PostgreSqlInAString {
    // TODO: Probably needs to react to changing VS theme, see https://stackoverflow.com/a/48993958/6058164

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class CommentClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-comment";

        private static readonly Color defaultCommentColor = Color.FromRgb(87, 166, 74);

        public CommentClassificationFormatDefinition() {
            DisplayName = "String SQL comment";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultCommentColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class IdentifierClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-identifier";

        private static readonly Color defaultIdentifierColor = Color.FromRgb(220, 220, 220);

        public IdentifierClassificationFormatDefinition() {
            DisplayName = "String SQL identifier";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultIdentifierColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class KeywordClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-keyword";

        private static readonly Color defaultKeywordColor = Color.FromRgb(86, 156, 214);

        public KeywordClassificationFormatDefinition() {
            DisplayName = "String SQL keyword";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultKeywordColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class NumberClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-number";

        private static readonly Color defaultNumberColor = Color.FromRgb(181, 206, 168);

        public NumberClassificationFormatDefinition() {
            DisplayName = "String SQL number";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultNumberColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class OperatorClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-operator";

        private static readonly Color defaultSqlOperatorColor = Color.FromRgb(129, 129, 129);

        public OperatorClassificationFormatDefinition() {
            DisplayName = "String SQL operator";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultSqlOperatorColor);
        }
    }

    // Parameter format does not exist for string escape sequences
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class ParameterClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-parameter";

        public ParameterClassificationFormatDefinition() {
            DisplayName = "String SQL parameter";
            ForegroundColor = Color.FromRgb(235, 199, 20);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class StringClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-string";

        private static readonly Color defaultSqlStringColor = Color.FromRgb(203, 65, 65);

        public StringClassificationFormatDefinition() {
            DisplayName = "String SQL string";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultSqlStringColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class SystemFunctionClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-sql-system-function";

        private static readonly Color defaultSqlSystemFunctionColor = Color.FromRgb(201, 117, 213);

        public SystemFunctionClassificationFormatDefinition() {
            DisplayName = "String SQL system function";
            ForegroundColor = TextEditorUtils.MixWithStringColor(defaultSqlSystemFunctionColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class CommentEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-comment";

        private static readonly Color defaultCommentColor = Color.FromRgb(87, 166, 74);

        public CommentEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL comment - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultCommentColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class IdentifierEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-identifier";

        private static readonly Color defaultIdentifierColor = Color.FromRgb(220, 220, 220);

        public IdentifierEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL identifier - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultIdentifierColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class KeywordEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-keyword";

        private static readonly Color defaultKeywordColor = Color.FromRgb(86, 156, 214);

        public KeywordEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL keyword - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultKeywordColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class NumberEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-number";

        private static readonly Color defaultNumberColor = Color.FromRgb(181, 206, 168);

        public NumberEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL number - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultNumberColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class OperatorEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-operator";

        private static readonly Color defaultSqlOperatorColor = Color.FromRgb(129, 129, 129);

        public OperatorEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL operator - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultSqlOperatorColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class StringEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-string";

        private static readonly Color defaultSqlStringColor = Color.FromRgb(203, 65, 65);

        public StringEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL string - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultSqlStringColor);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Name)]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class SystemFunctionEscapeClassificationFormatDefinition: ClassificationFormatDefinition {
        internal const string Name = "string-escape-sql-system-function";

        private static readonly Color defaultSqlSystemFunctionColor = Color.FromRgb(201, 117, 213);

        public SystemFunctionEscapeClassificationFormatDefinition() {
            DisplayName = "String SQL system function - escape character";
            ForegroundColor = TextEditorUtils.MixWithStringEscapeColor(defaultSqlSystemFunctionColor);
        }
    }
}
