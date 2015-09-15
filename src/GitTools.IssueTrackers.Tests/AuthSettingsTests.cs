namespace GitTools.IssueTrackers.Tests
{
    using Shouldly;
    using Xunit;

    public class AuthSettingsTests
    {
        [Fact]
        public void TheIsEmptyMethod()
        {
            var authenticationContext = new AuthSettings();

            authenticationContext.IsEmpty.ShouldBeTrue();
            authenticationContext.IsTokenAuthentication.ShouldBeFalse();
            authenticationContext.IsUsernameAndPasswordAuthentication.ShouldBeFalse();
        }

        [Fact]
        public void TheIsUsernameAndPasswordAuthenticationMethod()
        {
            var authenticationContext = new AuthSettings("user", "password");

            authenticationContext.IsEmpty.ShouldBeFalse();
            authenticationContext.IsTokenAuthentication.ShouldBeFalse();
            authenticationContext.IsUsernameAndPasswordAuthentication.ShouldBeTrue();
        }

        [Theory]
        [InlineData("user", "password", null, false)]
        [InlineData(null, null, "token", true)]
        [InlineData(null, null, null, false)]
        public void TheIsTokenAuthenticationMethod(string username, string password, string token, bool expectedValue)
        {
            var authenticationContext = new AuthSettings("token");

            authenticationContext.IsEmpty.ShouldBeFalse();
            authenticationContext.IsTokenAuthentication.ShouldBeTrue();
            authenticationContext.IsUsernameAndPasswordAuthentication.ShouldBeFalse();
        }
    }
}