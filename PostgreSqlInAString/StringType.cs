using System;

namespace PostgreSqlInAString {
    [Flags]
    internal enum StringType {
        Unknown = 0,
        Quoted = 1,
        Verbatim = 3,
        Interpolated = 5,
        Raw = 9
    }
}
