using System;
using System.Collections.Generic;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Backend
{
    public class DialogueState
    {
        public BlockNode CurrentBlock;
        public int CurrentBlockStatementIndex;
        public int CurrentCrossTextIndex;
        public int CurrentCrossTextStatementIndex;

        public Dictionary<Node, bool> OnceStatement;

        public Dictionary<string, Tuple<Type, object>> Variables;

        public Stack<BlockNode> CallStack;

        public void CallStackPush()
            => CallStack.Push(CurrentBlock);

        public BlockNode CallStackPop()
            => CallStack.Pop();
    }
}