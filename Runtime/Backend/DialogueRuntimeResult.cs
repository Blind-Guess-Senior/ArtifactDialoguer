using System;
using System.Collections.Generic;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Backend
{
    public interface IDialogueRuntimeResult
    {
        public Type ResultType { get; }
    }

    public struct DialogueRuntimeResultTextGot : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();

        public string Speaker;
        public string Content;

        public DialogueRuntimeResultTextGot(string speaker, string content)
        {
            Speaker = speaker;
            Content = content;
        }
    }

    public struct DialogueRuntimeResultOptionsGot : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();

        public List<DialogueRuntimeResultOption> Option;

        public DialogueRuntimeResultOptionsGot(List<DialogueRuntimeResultOption> option)
        {
            Option = option;
        }
    }

    public struct DialogueRuntimeResultOption
    {
        public readonly int Index;
        public string Text;
        public readonly CommandNode Command;
        public readonly bool Once;
        public readonly OptionOfNode Node;

        public DialogueRuntimeResultOption(int index, string text, CommandNode command, OptionOfNode node,
            bool once = false)
        {
            Index = index;
            Text = text;
            Command = command;
            Node = node;
            Once = once;
        }
    }

    public struct DialogueRuntimeResultBlockEnd : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();
    }

    public class DialogueRuntimeResultDialogueEnd : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();
    }

    public class DialogueRuntimeResultError : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();
    }

    public class DialogueRuntimeResultNextDenied : IDialogueRuntimeResult
    {
        public Type ResultType => GetType();
    }
}