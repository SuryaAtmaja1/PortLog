using PortLog.Helpers;
using Xunit;

namespace PortLog.Testing.Helpers
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_And_Verify_Should_Pass()
        {
            string password = "TestingPassword123!@";

            string hash = PasswordHasher.Hash(password);

            Assert.True(PasswordHasher.Verify(password, hash));
        }

        [Fact]
        public void Verify_Should_Fail_With_Wrong_Password()
        {
            string password = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";

            string hash = PasswordHasher.Hash(password);

            Assert.False(PasswordHasher.Verify(wrongPassword, hash));
        }

        [Fact]
        public void Hash_Should_Produce_Unique_Salts()
        {
            string password = "SamePassword123!";

            string hash1 = PasswordHasher.Hash(password);
            string hash2 = PasswordHasher.Hash(password);

            Assert.NotEqual(hash1, hash2);
        }
    }
}
