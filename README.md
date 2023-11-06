<p align="center">
  <img src="https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/7f494f51-7f97-4812-8d9c-9156a0a4d2c9" />
</p>

# PostgreSQL in a String

This is a Visual Studio extension that adds highlighting for PostgreSQL syntax in C# string literals.

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/b09afa46-1529-4f22-b8c4-9ddfec9d6181)

Works with Visual Studio 2019 and Visual Studio 2022. Applies only to C# `.cs` files.

Supports regular quoted strings `""`, verbatim strings `@""`, interpolated strings `$""` and interpolated verbatim strings `$@""`. Does not support C#11 raw string literals yet (I plan on tackling this after .NET 8 comes out).

Supports parametrized queries. Parameters prefixed with an `@` are colorized differently.

Correctly handles string escape sequences.

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/71ce4ccd-257f-4bff-aac0-beb717e2a0bb)

PostgreSQL in a String uses an ANTLR 4 lexer to tokenize PostgreSQL syntax.


# How to enable and disable highlighting

The extension does not automatically detect string literals with PostgreSQL syntax. The syntax highlighting must be explicitly enabled for a single string literal, a code region or whole project.

> [!NOTE]
> Rules inside XML documentation comments (starting with `///` or `/**`) are ignored.

## Inline rules

Highlighting can be enabled inline by prefixing a string literal with a comment containing `strpsql` text or `PostgreSqlInAString` (if you prefer to be more explicit). Note that only block comments can be used as the comment must be directly adjacent to the beginning of string literal.
```C#
string sql = /*strpsql*/@"SELECT COUNT(*) FROM order;";
```
When the string is inside enabled region, highlighting can be disabled inline by prefixing the string literal with a comment containing `strpsql-ignore` text or `PostgreSqlInAString-ignore` for a longer version.
```C#
string text = /*strpsql-ignore*/"Just a regular string, no SQL here";
```

## Region rules

Enable or disable highlighting in code regions with comment rules. A region rule will override highlighting of every string literal after it until the next rule or end of file.

Short version:
- `// strpsql-on `
- `// strpsql-off `

Longer version:
- `/* PostgreSqlInAString-enable */`
- `/* PostgreSqlInAString-disable */`

The rule parts can be mixed, e.g. `strpsql-enable` will work too.

## Project configuration

To have the highlighting enabled by default for an entire project, add the following section in the `.csproj` file (or merge it with existing `ProjectExtensions` element).
```
<ProjectExtensions>
  <PostgreSqlInAString>
    <Enabled>true</Enabled>
  </PostgreSqlInAString>
</ProjectExtensions>
```

This may be useful when you have a separate data access project where most of the strings are database queries.

## Explanations

Rules can contain explanations similar to what ESLint comment rules allow.

To add an explanation to the rule, add two dashes after the rule name (separated by some whitespace) and place your explanation after the dashes.

``` C#
/* strpsql-off -- This is HTML */
string str = "<select>" +
             "   <option>Apple</option>" +
             "   <option>Orange</option>" +
             "</select>";
/* strpsql-on -- That was HTML */
```

```C#
string sql = /* PostgreSqlInAString -- hey, 
              *    this is an explanation
              *    that spans multiple lines
              */@"TRUNCATE address, order_product, order, product RESTART IDENTITY;";
```


# Highlighting configuration

The default highlight colors were selected for VS dark theme but they can be customized in `Tools > Options > Environment > Fonts and Colors > String SQL * [ - escape character]`

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/7b5ed2e4-240e-4348-afb8-8b3ed53e0631)

The default colors except for parameter color are a mix of 50% default string color and 50% default SQL token color. This means that the highlighted text will have an orange tint, which is by design, to not overwhelm you when combined with other C# syntax as it is still just a string.


# Possible further improvements
- support raw string literals
- handle PL/pgSQL in a string in a string
- autocompletion
- light-mode-friendly color palette and auto-switching between palettes when mode is changed
- basic error recognition (typos, unsupported keyword order, etc.) by using an ANTLR parser
- extend to support more language syntaxes in string literals: T-SQL, HTML, etc.
