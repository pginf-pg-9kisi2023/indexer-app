using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using Indexer.Model;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Indexer.Tests.Model;

[TestClass]
public class SessionTests
{
    [TestMethod]
    [DeploymentItem(@"test_data\session_valid.xml")]
    public void FromFile_WithValidFile_IsInValidState()
    {
        var session = Session.FromFile("session_valid.xml");

        // session.Config
        Assert.IsNotNull(session.Config);
        var hints = session.Config.Hints;
        Assert.AreEqual(2, hints.Count);
        Assert.AreEqual("TOP_LT", hints[0].Name);
        Assert.AreEqual("Górne pole - lewy górny narożnik", hints[0].Description);
        Assert.AreEqual("V20_TOP_LT.jpg", hints[0].ImagePath);
        Assert.AreEqual("TOP_RT", hints[1].Name);
        Assert.AreEqual("Górne pole - prawy górny narożnik", hints[1].Description);
        Assert.AreEqual("V20_TOP_RT.jpg", hints[1].ImagePath);

        // session.CurrentHint.Name
        Assert.AreEqual("TOP_RT", session.CurrentHint?.Name);
        // session.CurrentImageIndex
        Assert.AreEqual(0, session.CurrentImageIndex);

        // session.IndexedImages
        var images = session.IndexedImages;
        ReadOnlyCollection<Label> labels;
        Assert.AreEqual(3, images.Count);

        // session.IndexedImages[0]
        Assert.AreEqual(@"D:\absolute\path\to\image1.jpg", images[0].ImagePath);
        labels = images[0].Labels;
        Assert.AreEqual(2, labels.Count);
        Assert.AreEqual("TOP_LT", labels[0].Name);
        Assert.AreEqual(123, labels[0].X);
        Assert.AreEqual(456, labels[0].Y);
        Assert.AreEqual("TOP_RT", labels[1].Name);
        Assert.AreEqual(321, labels[1].X);
        Assert.AreEqual(654, labels[1].Y);

        // session.IndexedImages[1]
        Assert.AreEqual(@"D:\absolute\path\to\image2.jpg", images[1].ImagePath);
        labels = images[1].Labels;
        Assert.AreEqual(1, labels.Count);
        Assert.AreEqual("TOP_LT", labels[0].Name);
        Assert.AreEqual(404, labels[0].X);
        Assert.AreEqual(103, labels[0].Y);

        // session.IndexedImages[2]
        Assert.AreEqual(@"D:\absolute\path\to\image3.jpg", images[2].ImagePath);
        labels = images[2].Labels;
        Assert.AreEqual(0, labels.Count);
    }

    [TestMethod]
    [DeploymentItem(@"test_data\invalid_xml.xml")]
    public void FromFile_WithInvalidXml_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("invalid_xml.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_bad_root_name.xml")]
    public void FromFile_WithBadRootName_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("session_bad_root_name.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_missing_element.xml")]
    public void FromFile_WithMissingElement_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("session_missing_element.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_wrong_order.xml")]
    public void FromFile_WithWrongOrder_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("session_wrong_order.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_duplicate_config_point_name.xml")]
    public void FromFile_WithDuplicateConfigPointName_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("session_duplicate_config_point_name.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_duplicate_image_point_name.xml")]
    public void FromFile_WithDuplicateImagePointName_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Session.FromFile("session_duplicate_image_point_name.xml")
        );
    }

    [TestMethod]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(
            () => Session.FromFile("non_existent_file.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_valid.xml")]
    public void Save_ToSamePath_KeepsIdenticalContent()
    {
        var before = File.ReadAllText("session_valid.xml", Encoding.UTF8);
        before = before.Replace("\r\n", "\n");
        var session = Session.FromFile("session_valid.xml");

        session.Save();
        var after = File.ReadAllText("session_valid.xml", Encoding.UTF8);

        Assert.AreEqual(before, after);
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_valid.xml")]
    public void Save_ToDifferentPath_CheckOutput()
    {
        var expected = File.ReadAllText("session_valid.xml", Encoding.UTF8);
        expected = expected.Replace("\r\n", "\n");
        var session = Session.FromFile("session_valid.xml");
        session.FilePath = Path.GetTempFileName();

        try
        {
            session.Save();
            var actual = File.ReadAllText(session.FilePath, Encoding.UTF8);

            Assert.AreEqual(expected, actual);
        }
        finally
        {
            File.Delete(session.FilePath);
        }
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_valid.xml")]
    [DeploymentItem(@"test_data\session_exported_points.csv")]
    public void ExportPointsToCSV_CheckOutput()
    {
        var expected = File.ReadAllText("session_exported_points.csv", Encoding.UTF8);
        expected = expected.Replace("\r\n", "\n");
        var session = Session.FromFile("session_valid.xml");
        var filePath = Path.GetTempFileName();

        try
        {
            session.ExportPointsToCSV(filePath);
            var actual = File.ReadAllText(filePath, Encoding.UTF8);

            Assert.AreEqual(expected, actual);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_valid.xml")]
    [DeploymentItem(@"test_data\session_exported_points.xml")]
    public void ExportPointsToXML_CheckOutput()
    {
        var expected = File.ReadAllText("session_exported_points.xml", Encoding.UTF8);
        expected = expected.Replace("\r\n", "\n");
        var session = Session.FromFile("session_valid.xml");
        var filePath = Path.GetTempFileName();

        try
        {
            session.ExportPointsToXML(filePath);
            var actual = File.ReadAllText(filePath, Encoding.UTF8);

            Assert.AreEqual(expected, actual);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [TestMethod]
    [DeploymentItem(@"test_data\session_valid_without_labels.xml")]
    [DeploymentItem(@"test_data\PointProposer.exe")]
    [DeploymentItem(@"test_data\PointProposer.dll")]
    public void AnalyzeImages_TestLabelCount()
    {
        var expected = 2;
        var session = Session.FromFile("session_valid.xml");
        session.AnalyzeImages("PointProposer.exe");
        session.IndexedImages.TryGetValue(
            "D:\\absolute\\path\\to\\image1.jpg", out var image
        );
        Assert.IsNotNull(image);
        Assert.AreEqual(expected, image.Labels.Count);
    }
}
