using System.IO;
using System.Runtime.Serialization;

using Indexer.Model;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Indexer.Tests.Model;

[TestClass]
public class ConfigTests
{
    [TestMethod]
    [DeploymentItem(@"test_data\config_valid.xml")]
    public void FromFile_WithValidFile_IsInValidState()
    {
        var config = Config.FromFile("config_valid.xml");

        Assert.AreEqual(2, config.Hints.Count);
        var hints = config.Hints;
        Assert.AreEqual("TOP_LT", hints[0].Name);
        Assert.AreEqual("Górne pole - lewy górny narożnik", hints[0].Description);
        Assert.AreEqual("V20_TOP_LT.jpg", hints[0].ImagePath);
        Assert.AreEqual("TOP_RT", hints[1].Name);
        Assert.AreEqual("Górne pole - prawy górny narożnik", hints[1].Description);
        Assert.AreEqual("V20_TOP_RT.jpg", hints[1].ImagePath);
    }

    [TestMethod]
    [DeploymentItem(@"test_data\invalid_xml.xml")]
    public void FromFile_WithInvalidXml_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Config.FromFile("invalid_xml.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\config_bad_root_name.xml")]
    public void FromFile_WithBadRootName_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Config.FromFile("config_bad_root_name.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\config_missing_element.xml")]
    public void FromFile_WithMissingElement_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Config.FromFile("config_missing_element.xml")
        );
    }

    [TestMethod]
    [DeploymentItem(@"test_data\config_wrong_order.xml")]
    public void FromFile_WithWrongOrder_ThrowsSerializationException()
    {
        Assert.ThrowsException<SerializationException>(
            () => Config.FromFile("config_wrong_order.xml")
        );
    }

    [TestMethod]
    public void FromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(
            () => Config.FromFile("non_existent_file.xml")
        );
    }
}
