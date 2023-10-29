namespace PostgreSqlInAString {
    internal enum SqlTokenCategory {
        None = 0,   // Also for whitespace
        Comment,
        Identifier,
        Keyword,
        Number,
        Operator,   // Also for punctuation
        Parameter,
        String,
        SystemFunction  // Also for special/rare tokens
    }
}
