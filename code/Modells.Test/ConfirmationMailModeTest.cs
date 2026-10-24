using Modells.User;

namespace Modells.Test {

    public class ConfirmationMailModeTest {

        [Theory]
        [InlineData("0", ConfirmationMailMode.Never)]
        [InlineData("1", ConfirmationMailMode.Optional)]
        [InlineData("2", ConfirmationMailMode.Always)]
        [InlineData("True", ConfirmationMailMode.Optional)]
        [InlineData("False", ConfirmationMailMode.Never)]
        [InlineData("true", ConfirmationMailMode.Optional)]
        [InlineData(" 2 ", ConfirmationMailMode.Always)]
        public void KnownValues_AreConverted(string input, ConfirmationMailMode expected) {
            Assert.Equal(expected, ConfirmationMailModes.Parse(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("3")]
        [InlineData("-1")]
        public void UnknownValues_GiveFallback(string? input) {
            Assert.Equal(ConfirmationMailMode.Optional, ConfirmationMailModes.Parse(input));
            Assert.Equal(ConfirmationMailMode.Never, ConfirmationMailModes.Parse(input, ConfirmationMailMode.Never));
        }
    }
}
