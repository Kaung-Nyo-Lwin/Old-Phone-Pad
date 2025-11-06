using NUnit.Framework;

namespace OldPhonePadWeb.Tests;

[TestFixture]
public class UnitTests
{
    [Test]
    public void Test_E() => Assert.That(OldPhonePadDecoder.Decode("33#"), Is.EqualTo("E"));

    [Test]
    public void Test_B() => Assert.That(OldPhonePadDecoder.Decode("227*#"), Is.EqualTo("B"));

    [Test]
    public void Test_HELLO() => Assert.That(OldPhonePadDecoder.Decode("4433555 555666#"), Is.EqualTo("HELLO"));

    [Test]
    public void Test_TURING() => Assert.That(OldPhonePadDecoder.Decode("8 88777444666*664#"), Is.EqualTo("TURING"));


    [Test]
    public void Test_NullInput() => Assert.That(OldPhonePadDecoder.Decode(null), Is.EqualTo(string.Empty));

    [Test]
    public void Test_EmptyInput() => Assert.That(OldPhonePadDecoder.Decode(""), Is.EqualTo(string.Empty));

    [Test]
    public void Test_SendOnly() => Assert.That(OldPhonePadDecoder.Decode("#"), Is.EqualTo(string.Empty));

    [Test]
    public void Test_SpacesOnly() => Assert.That(OldPhonePadDecoder.Decode("   #"), Is.EqualTo(string.Empty));

    [Test]
    public void Test_BackspaceEmpty() => Assert.That(OldPhonePadDecoder.Decode("*****#"), Is.EqualTo(string.Empty));

    [Test]
    public void Test_BackspaceMidBuffer() => Assert.That(OldPhonePadDecoder.Decode("22*2#"), Is.EqualTo("A"));

    [Test]
    public void Test_WrapAround_2_Key() => Assert.That(OldPhonePadDecoder.Decode("2222#"), Is.EqualTo("A"));

    [Test]
    public void Test_WrapAround_7_Key() => Assert.That(OldPhonePadDecoder.Decode("77777#"), Is.EqualTo("P"));
}
