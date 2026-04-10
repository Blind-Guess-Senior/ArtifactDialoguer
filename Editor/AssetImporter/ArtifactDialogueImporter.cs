using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.AssetImporter
{
    [ScriptedImporter(0, "artidial")]
    public class ArtifactDialogueImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = File.ReadAllText(ctx.assetPath);

            var textAsset = new TextAsset(text);

            ctx.AddObjectToAsset("main", textAsset);
            ctx.SetMainObject(textAsset);
        }
    }
}