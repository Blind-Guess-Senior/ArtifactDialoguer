using System.Linq;
using UnityEditor;
using UnityEngine;
using BlindGuessSenior.ArtifactDialoguer.Frontend;
using BlindGuessSenior.ArtifactDialoguer.Utilities.DebugUtils;

namespace Editor.Compiler
{
    public static class DialogueCompile
    {
        [MenuItem("Artifact Dialoguer/Compile All")]
        public static void Compile()
        {
            var assets = AssetDatabase.FindAssets("t:TextAsset").Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => x.EndsWith(".artidial"))
                .Select(AssetDatabase.LoadAssetAtPath<TextAsset>)
                .ToList();

            var tokens = assets.Select(x => Lexer.Tokenize(x.text)).ToList();

            var dialogues = Parser.Parse(tokens);

            if (!AssetDatabase.IsValidFolder("Assets/Resources/ArtiDialogue"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "ArtiDialogue");
            }

            foreach (var story in dialogues)
            {
                var so = ScriptableObject.CreateInstance<DialogueStorageObject>();
                so.Namespace = story.Namespace;
                so.ScopeVars = story.ScopeVars.ToList();
                so.Blocks = story.Blocks;

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

            ArtifactDialoguerDebug.PackageLog($"Compile {assets.Count} files.", DebugLogLevel.WorksWell);
        }
    }
}