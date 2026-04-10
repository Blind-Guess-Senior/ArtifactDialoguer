using System.Collections.Generic;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
    public class DialogueStorageObject : ScriptableObject
    {
        public string Namespace;

        public List<string> ScopeVars = new();
        
        [SerializeReference]
        public List<BlockNode> Blocks = new();
    }
}