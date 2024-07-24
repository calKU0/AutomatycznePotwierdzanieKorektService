using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatyczneZatwierdzanieKorektService;
using Xunit;
using cdn_api;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System.Diagnostics;

namespace AutomatyczneZatwierdzanieKorektService.Test
{
    public class CorrectionsTests
    {
        public static int IDSesjiXL = 0;
        public static readonly Int32 APIVersion = 20231;

        [Fact]
        public void GetCorrections_DataRowsShouldEqual()
        {
            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);

            // Arrange
            int expected = 26; // Expected dataRows on todays day

            // Act
            int actual = corrections.GetCorrections().Rows.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCorrections_ShouldNotBeNull()
        {
            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);
            
            // Arrange

            // Act
            DataTable actual = corrections.GetCorrections();

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void ConfirmCorrections_ShouldConfirm()
        {
            XLApi xlApi = new XLApi();
            xlApi.Login();

            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);

            // Arrange
            DataTable correctionsToUpdate = corrections.GetCorrections();
            int correctionsToUpdateCount = correctionsToUpdate.Rows.Count;

            //Act
            int correctionsUpdated = corrections.ConfirmCorrections(correctionsToUpdate);
            // Assert
            Assert.Equal(correctionsToUpdateCount, correctionsUpdated);

            xlApi.Logout();
        }
    }
}
