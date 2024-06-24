using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomatyczneZatwierdzanieKorektService;
using Xunit;

namespace AutomatyczneZatwierdzanieKorektService.Test
{
    public class CorrectionsTests
    {
        [Fact]
        public void GetCorrections_DataRowsShouldEqual()
        {
            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);

            // Arrange
            int expected = 0; // Expected dataRows on todays day

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
        public void ConfirmCorrections_ShouldUpdateDatabaseTable()
        {
            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);

            // Arrange
            DataTable correctionsToUpdate = corrections.GetCorrections();
            int correctionsToUpdateCount = correctionsToUpdate.Rows.Count;

            //Act
            int correctionsUpdated = corrections.ConfirmCorrections(correctionsToUpdate);

            // Assert
            Assert.Equal(correctionsToUpdateCount, correctionsUpdated);
        }
    }
}
