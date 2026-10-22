using EssenskasseAPI.Data;

namespace EssenskasseAPI.Test {

    public class DatabaseMigratorTest {

        private static readonly string[] All = [
            "update_v1-3-0_zu_1-4-0",
            "update_v1-1-0_zu_1-2-0",
            "update_v1-2-0_zu_1-3-0"
        ];

        [Fact]
        public void Order_SortsByVersion() {
            Assert.Equal(
                ["update_v1-1-0_zu_1-2-0", "update_v1-2-0_zu_1-3-0", "update_v1-3-0_zu_1-4-0"],
                DatabaseMigrator.Order(All));
        }

        [Fact]
        public void Order_ComparesNumbersNotText() {
            Assert.Equal(
                ["update_v1-9-0_zu_1-10-0", "update_v1-10-0_zu_2-0-0"],
                DatabaseMigrator.Order(["update_v1-10-0_zu_2-0-0", "update_v1-9-0_zu_1-10-0"]));
        }

        [Theory]
        [InlineData("update_1-2-0_zu_1-3-0")]
        [InlineData("update_v1-2_zu_1-3")]
        [InlineData("irgendwas")]
        public void Order_RejectsUnexpectedNames(string name) {
            Assert.Throws<InvalidOperationException>(() => DatabaseMigrator.Order([name]));
        }

        [Fact]
        public void Pending_ReturnsOnlyMissingInOrder() {
            Assert.Equal(
                ["update_v1-2-0_zu_1-3-0", "update_v1-3-0_zu_1-4-0"],
                DatabaseMigrator.Pending(All, ["update_v1-1-0_zu_1-2-0"]));
        }

        [Fact]
        public void Pending_NothingMissing_IsEmpty() {
            Assert.Empty(DatabaseMigrator.Pending(All, All));
        }

        [Fact]
        public void Pending_IgnoresUnknownAppliedEntries() {
            Assert.Equal(3, DatabaseMigrator.Pending(All, ["update_v0-9-0_zu_1-0-0"]).Count);
        }

        [Fact]
        public void EmbeddedScripts_AreNamedCorrectly() {
            // Die eingebetteten Skripte muessen alle dem Namensmuster entsprechen, sonst scheitert der Start der API.
            string[] ids = typeof(DatabaseMigrator).Assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("Migrations/"))
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .ToArray();
            Assert.NotEmpty(ids);
            Assert.Equal(ids.Length, DatabaseMigrator.Order(ids).Count);
        }
    }
}
