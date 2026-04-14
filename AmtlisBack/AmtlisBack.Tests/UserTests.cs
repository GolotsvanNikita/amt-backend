using System;
using Xunit;
using AmtlisBack.Models;

namespace AmtlisBack.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_Initialization_SetsPropertiesCorrectly()
        {
            var user = new User
            {
                Id = 1,
                Name = "TestUser",
                Email = "test@example.com"
            };

            Assert.Equal(1, user.Id);
            Assert.Equal("TestUser", user.Name);
            Assert.Equal("test@example.com", user.Email);
            Assert.True((DateTime.UtcNow - user.CreatedAt).TotalSeconds < 1);
        }
    }
}