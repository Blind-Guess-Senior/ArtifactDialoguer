using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Tests.Editor
{
    [Category("Frontend")]
    public class CompileTest
    {
        [Test]
        public void CompileSampleFile()
        {
            string samplePath = Application.dataPath + "/Resources/dialoguesample.artidial";
            Assert.IsTrue(File.Exists(samplePath), $"找不到测试文件: {samplePath}");

            string source = File.ReadAllText(samplePath);
            var tokens = Lexer.Tokenize(source);
            
            Assert.IsNotNull(tokens);
            
            var tokensList = new System.Collections.Generic.List<System.Collections.Generic.List<Token>> { tokens };
            var dialogues = Parser.Parse(tokensList);
            
            Assert.IsNotNull(dialogues);

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/ArtiDialogue"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "ArtiDialogue");
            }

            foreach (var story in dialogues)
            {
                TestContext.WriteLine($"Namespace: {story.Namespace}");
                TestContext.WriteLine($"ScopeVars Count: {story.ScopeVars?.Count ?? 0}");
                TestContext.WriteLine($"Blocks Count: {story.Blocks?.Count ?? 0}");

                if (story.Blocks != null)
                {
                    foreach (var block in story.Blocks)
                    {
                        TestContext.WriteLine($"  Block Name: {block.BlockName}");
                        TestContext.WriteLine($"    Statements Count: {block.Statements?.Count ?? 0}");
                    }
                }

                var so = ScriptableObject.CreateInstance<DialogueStorageObject>();
                so.Namespace = story.Namespace;
                so.ScopeVars = story.ScopeVars?.ToList() ?? new System.Collections.Generic.List<string>();
                so.Blocks = story.Blocks;
                so.name = story.Namespace;

                string path = $"Assets/Resources/ArtiDialogue/{story.Namespace}.asset";

                var existing = AssetDatabase.LoadAssetAtPath<DialogueStorageObject>(path);
                if (existing)
                {
                    EditorUtility.CopySerialized(so, existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(so, path);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}