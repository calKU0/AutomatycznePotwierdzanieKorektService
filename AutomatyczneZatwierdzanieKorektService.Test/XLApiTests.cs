using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutomatyczneZatwierdzanieKorektService.Test
{
    public class XLApiTests
    {
        [Fact]
        public void Login_ShouldLoginAndLogout()
        {
            // Arrange
            XLApi xlApi = new XLApi();
            int expected = 0;

            // Act
            int actual = xlApi.Login();
            actual += xlApi.Logout();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
