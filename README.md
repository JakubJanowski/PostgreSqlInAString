<p align="center">
  <img src="https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/7f494f51-7f97-4812-8d9c-9156a0a4d2c9" />
</p>

# PostgreSQL in a String for Visual Studio 2019

This is a separate PostgreSQL in a String extension that targets only Visual Studio 2019. Extension for Visual Studio 2022 can be found on the [main branch](https://github.com/JakubJanowski/PostgreSqlInAString).

---

PostgreSQL in a String is a Visual Studio extension that adds highlighting for PostgreSQL syntax in C# string literals.

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/3f331c52-556c-424f-8900-6a72d699c3bd)

Works with Visual Studio 2019 and Visual Studio 2022. Project targetting Visual Studio 2019 is located on a [separate branch](https://github.com/JakubJanowski/PostgreSqlInAString/tree/vs2019).

Applies only to C# `.cs` files.

Supports parametrized queries. Parameter placeholders prefixed with an `@` (named) or `$` (positional) are colorized in golden.

Supports all string literal types!
- regular quoted `" "`,
- verbatim `@" "`,
- interpolated `$" "`,
- interpolated verbatim `$@" "`,
- raw `""" """`
- interpolated raw `$""" """`.

Correctly handles string escape sequences.

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/71ce4ccd-257f-4bff-aac0-beb717e2a0bb)

PostgreSQL in a String uses an ANTLR 4 lexer to tokenize PostgreSQL syntax.


# Installation

## Visual Studio 2022

1. In Visual Studio, navigate to `Extensions > Manage Extensions > Online`.
1. Search for `PostgreSQL in a String`.
1. Click Download. The extension is then scheduled for install.

Alternatively, you can download the extension from [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=jakub-janowski.strpsql).

## Visual Studio 2019

1. In Visual Studio, navigate to `Extensions > Manage Extensions > Online`.
1. Search for `PostgreSQL in a String for VS2019`.
1. Click Download. The extension is then scheduled for install.

Alternatively, you can download the extension from [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=jakub-janowski.strpsql-vs2019).


# How to enable and disable highlighting

The extension does not automatically detect string literals with PostgreSQL syntax. The syntax highlighting must be explicitly enabled for a single string literal, a code region or whole project.

> [!NOTE]
> Rule configuration inside XML documentation comments (starting with `///` or `/**`) are ignored.

## Inline rule configuration

Highlighting can be enabled inline by prefixing a string literal with a comment containing `strpsql` text or `PostgreSqlInAString` (if you prefer to be more explicit). Note that only block comments can be used as the comment must be directly adjacent to the beginning of string literal.
```C#
string sql = /*strpsql*/@"SELECT COUNT(*) FROM order;";
```
When the string literal is inside an enabled region, its highlighting can be disabled inline by prefixing it with a comment containing `strpsql-ignore` text or `PostgreSqlInAString-ignore` for a longer version.
```C#
string text = /*strpsql-ignore*/"Just a regular string, no SQL here";
```

## Region rule configuration

Enable or disable highlighting in code regions with rule configuration comments. A region configuration will override highlighting of every string literal after it, except for those prefixed with inline rule configuration, until the next region configuration or end of file.

Short version:
- `// strpsql-on `
- `// strpsql-off `

Longer version:
- `/* PostgreSqlInAString-enable */`
- `/* PostgreSqlInAString-disable */`

The rule parts can be mixed, e.g. `/*strpsql-enable*/` will work too.

## Project configuration

To have the highlighting enabled by default for an entire project, add the following section in the `.csproj` file (or merge it with existing `ProjectExtensions` element).
```XML
<ProjectExtensions>
  <PostgreSqlInAString>
    <Enabled>true</Enabled>
  </PostgreSqlInAString>
</ProjectExtensions>
```

This may be useful when you have a separate data access project where most of the strings are database queries.

Adding/changing nodes in `.csproj` might require reopening all already open files from that project for the configuration to take effect.

## Explanations

Rule configuration comments can contain explanations similar to what ESLint rule configuration comments allow.

To add an explanation to the configuration, add two dashes after the rule name (separated by some whitespace) and place your explanation after the dashes.

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
              *    that spans multiple lines.
              */@"TRUNCATE address, order_product, order, product RESTART IDENTITY;";
```


# Highlighting configuration

The default highlight colors were selected for VS dark theme but they can be customized in `Tools > Options > Environment > Fonts and Colors > String SQL * [ - escape character]`

![image](https://github.com/JakubJanowski/PostgreSqlInAString/assets/19607303/7b5ed2e4-240e-4348-afb8-8b3ed53e0631)

The default colors except for parameter placeholder color are a mix of 50% default string color and 50% default SQL token color. This means that the highlighted text will have an orange tint, which is by design, to not overwhelm when combined with other C# syntax as it is still just a string literal.


# Possible further improvements
- Highlighting of PL/pgSQL in a string in a string
- Autocompletion
- Light-mode-friendly color palette and auto-switching between palettes when mode is changed
- Basic error recognition (typos, unsupported keyword order, etc.) by using an ANTLR parser
- Support for more language syntaxes in string literals: T-SQL, HTML, etc.
