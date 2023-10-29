namespace PostgreSqlInAString {
    internal class PostgreSqlInAStringConfiguration {
        internal static PostgreSqlInAStringConfiguration GetDefault() => new PostgreSqlInAStringConfiguration() { Enabled = false };

        internal bool Enabled { get; set; }
    }
}
