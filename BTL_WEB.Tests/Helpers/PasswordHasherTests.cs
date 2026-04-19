using System.Security.Cryptography;
using System.Text;
using BTL_WEB.Services;

namespace BTL_WEB.Tests.Helpers;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsSha256Hex()
    {
        var hasher = new PasswordHasher();

        var actual = hasher.Hash("abc123");

        Assert.Equal("6ca13d52ca70c883e0f0bb101e425a89e8624de51db2d2392593af6a84118090", actual);
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenStoredPasswordIsRawText()
    {
        var hasher = new PasswordHasher();

        var result = hasher.Verify("plain-pass", "plain-pass");

        Assert.True(result);
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenStoredPasswordIsBase64Sha256()
    {
        var hasher = new PasswordHasher();
        var base64 = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("abc123")));

        var result = hasher.Verify("abc123", base64);

        Assert.True(result);
    }

    [Fact]
    public void Verify_ReturnsFalse_ForInvalidPassword()
    {
        var hasher = new PasswordHasher();

        var result = hasher.Verify("wrong", hasher.Hash("correct"));

        Assert.False(result);
    }
}
