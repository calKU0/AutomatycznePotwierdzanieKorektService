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
        [Fact]
        public void CloseDocument_ShouldOpenAndClose()
        {
            // Arrange
            XLApi xlApi = new XLApi();
            int expected = 1;

            // Act
            xlApi.Login();
            int gidNumber = 1619793;
            int gidTyp = 2041;

            xlApi.OpenDocument(gidNumber, gidTyp);
            int actual = xlApi.CloseDocument();

            xlApi.Logout();

            // Assert
            Assert.Equal(expected, actual);
        }

    }
}
