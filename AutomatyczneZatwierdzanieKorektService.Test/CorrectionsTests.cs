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
            int expected = 12; // Expected dataRows on todays day

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
        public void ConfirmCorrections_ShouldConfirmAndGeneratePM()
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

        [Fact]
        public void GeneratePM_ShouldGenerate()
        {
            XLApi xlApi = new XLApi();
            xlApi.Login();

            Corrections corrections = new Corrections(ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString);

            //Arrange
            int expected = 0;
            DataTable correctionsToUpdate = new DataTable();
            correctionsToUpdate.Columns.Add("TrN_GIDNumer");
            correctionsToUpdate.Columns.Add("TrN_GIDTyp");
            correctionsToUpdate.Columns.Add("TrN_DokumentObcy");
            correctionsToUpdate.Columns.Add("Czy generowac dok. magazynowe", expected.GetType());

            correctionsToUpdate.Rows.Add(1775365, 2042, "PAK-230/24/DETK", 1);
            correctionsToUpdate.Rows.Add(1769032, 2042, "PAK-207/24/DETK", 0);
            correctionsToUpdate.Rows.Add(1769132, 2041, "FSK-708/24/SPRK", 0);
            correctionsToUpdate.Rows.Add(1769513, 2041, "FSK-712/24/SPRK", 0);

            foreach (DataRow row in correctionsToUpdate.Rows)
            {
                if (Convert.ToBoolean(row["Czy generowac dok. magazynowe"]))
                {
                    expected += 1;
                }
            }

            //Act
            int actual = 0;
            foreach (DataRow row in correctionsToUpdate.Rows)
            {
                if (Convert.ToBoolean(row["Czy generowac dok. magazynowe"]))
                {
                    int result = corrections.GeneratePM(row);
                    actual += result == 0 ? 1 : result;
                }
            }

            // Assert
            Assert.Equal(expected, actual);
            xlApi.Logout();
        }
    }
}
