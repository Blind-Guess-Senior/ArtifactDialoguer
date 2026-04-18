using System.IO;
using NUnit.Framework;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Tests.Editor
{
    [Category("Frontend")]
    public class LexerTests
    {
        [Test]
        public void TestLexerWithSampleFile()
        {
            // 通过Unity包路径读取
            string samplePath = Path.GetFullPath("Packages/com.blind-guess-senior.artifactdialoguer/Samples~/DialogueSample/dialoguesample.artidial");

            Assert.IsTrue(File.Exists(samplePath), $"找不到测试文件: {samplePath}");

            string source = File.ReadAllText(samplePath);

            var tokens = Lexer.Tokenize(source);

            Assert.IsNotNull(tokens);
            Assert.IsTrue(tokens.Count > 0);

            // 打印出所有 Token 以供检查，方便在控制台或是Test Runner中查看
            foreach (var token in tokens)
            {
                // 用 NUnit 的 TestContext 打印，这样在一些非 Unity 环境也能看到输出
                TestContext.WriteLine(
                    $"[{token.Line}:{token.Column}] {token.Type} : '{token.Literal.Replace("\n", "\\n").Replace("\r", "\\r")}'");
            }
        }
    }
}