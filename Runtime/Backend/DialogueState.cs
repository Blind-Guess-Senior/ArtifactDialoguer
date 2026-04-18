using System;
using System.Collections.Generic;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Backend
{
    using CallStackEntry = Tuple<BlockNode, int>;
    
    public class DialogueState
    {
        public BlockNode CurrentBlock;
        public int CurrentBlockStatementIndex;
        public int CurrentCrossTextIndex;
        public int CurrentCrossTextStatementIndex;

        public Dictionary<Node, bool> OnceStatement;

        public Dictionary<string, Tuple<Type, object>> Variables;

        public Stack<CallStackEntry> CallStack;

        public void CallStackPush()
            => CallStack.Push(new CallStackEntry(CurrentBlock, CurrentBlockStatementIndex));

        public CallStackEntry CallStackPop()
            => CallStack.Pop();
    }
}