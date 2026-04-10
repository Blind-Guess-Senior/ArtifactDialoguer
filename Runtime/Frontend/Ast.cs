using System.Collections.Generic;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
    // TODO: internal commands: goto, if var cond, extcall

    [System.Serializable]
    public abstract class Node
    {
    }

    /// <summary>
    /// Definition of node of a namespaced story.
    /// </summary>
    [System.Serializable]
    public sealed class NamespaceStoryNode : Node
    {
        #region Fields

        /// <summary>
        /// Namespace of current story.
        /// <br/>
        /// Stories with same namespace will be process together.(Unfinished)
        /// </summary>
        public readonly string Namespace;

        /// <summary>
        /// All blocks of this story node.
        /// </summary>
        public readonly List<BlockNode> Blocks = new();

        /// <summary>
        /// All scope variables definition.
        /// </summary>
        public readonly List<string> ScopeVars = new();

        #endregion

        #region Constructor

        public NamespaceStoryNode(string namespaceName)
        {
            Namespace = namespaceName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Helper methods for get block node by identification name.
        /// </summary>
        /// <param name="name">The identification name of a block node.</param>
        /// <returns>The block node in current node with given name.</returns>
        public BlockNode GetBlock(string name)
            => Blocks.Find(x => x.BlockName == name);

        /// <summary>
        /// Add a block to current namespace story node.
        /// </summary>
        /// <param name="block">The block node to add.</param>
        public void AddBlock(BlockNode block)
            => Blocks.Add(block);

        #endregion
    }

    /// <summary>
    /// Definition of a block, which is the component of a story node.
    /// </summary>
    [System.Serializable]
    public sealed class BlockNode : Node
    {
        #region Fields

        #region Block Property Fields

        /// <summary>
        /// Identification name of this block node.
        /// </summary>
        public readonly string BlockName;

        [SerializeReference] public readonly List<IDialogueAttribute> Attributes;

        #endregion

        /// <summary>
        /// Previous block node in natural flow.
        /// </summary>
        public BlockNode NaturalPrevious;

        /// <summary>
        /// Next block node in natural flow.
        /// </summary>
        public BlockNode NaturalNext;

        /// <summary>
        /// Statement nodes of this block node.
        /// </summary>
        [SerializeReference] public readonly List<StatementNode> Statements = new();

        #endregion

        #region Constructor

        private BlockNode(string blockName)
        {
            BlockName = blockName;
        }

        public BlockNode(string blockName, List<IDialogueAttribute> attributes) : this(blockName)
        {
            Attributes = attributes;
        }

        #endregion
    }

    /// <summary>
    /// Abstract class for definition of statement.
    /// </summary>
    [System.Serializable]
    public abstract class StatementNode : Node
    {
        #region Fields

        [SerializeReference] public readonly List<IDialogueAttribute> Attributes;

        #endregion

        #region Constructor

        protected StatementNode(List<IDialogueAttribute> attributes)
        {
            Attributes = attributes;
        }

        #endregion
    }

    /// <summary>
    /// Abstract class for a command node.
    /// </summary>
    [System.Serializable]
    public abstract class CommandNode : StatementNode
    {
        #region Constructor

        protected CommandNode(List<IDialogueAttribute> attributes) : base(attributes)
        {
        }

        #endregion
    }

    [System.Serializable]
    public sealed class GotoCommandNode : CommandNode
    {
        #region Fields

        public BlockNode ToBlock;

        #endregion

        #region Constructor

        public GotoCommandNode(List<IDialogueAttribute> attributes) : base(attributes)
        {
        }

        #endregion
    }

    [System.Serializable]
    public sealed class RetCommandNode : CommandNode
    {
        #region Fields

        public readonly BlockNode BelongsToBlock;

        #endregion

        #region Constructor

        public RetCommandNode(BlockNode belongsTo, List<IDialogueAttribute> attributes) : base(attributes)
        {
            BelongsToBlock = belongsTo;
        }

        #endregion
    }

    [System.Serializable]
    public sealed class NullCommandNode : CommandNode
    {
        public NullCommandNode() : base(null)
        {
        }
    }

    [System.Serializable]
    public sealed class SetCommandNode : CommandNode
    {
        #region Fields

        public readonly string TargetVar;

        [SerializeReference] public readonly ExpressionNode ValueExpr;

        #endregion

        #region Constructors

        public SetCommandNode(string targetVar, ExpressionNode valueExpr, List<IDialogueAttribute> attributes) :
            base(attributes)
        {
            TargetVar = targetVar;
            ValueExpr = valueExpr;
        }

        #endregion
    }

    [System.Serializable]
    public sealed class GlobalSetCommandNode : CommandNode
    {
        #region Fields

        public readonly string TargetVar;
        [SerializeReference] public readonly ExpressionNode ValueExpr;

        #endregion

        #region Constructors

        public GlobalSetCommandNode(string targetVar, ExpressionNode valueExpr, List<IDialogueAttribute> attributes) :
            base(attributes)
        {
            TargetVar = targetVar;
            ValueExpr = valueExpr;
        }

        #endregion
    }

    /// <summary>
    /// Class for definition of current speaker
    /// </summary>
    [System.Serializable]
    public sealed class SpeakerNode : StatementNode
    {
        #region Fields

        public readonly string SpeakerName;

        #endregion

        #region Constructor

        public SpeakerNode(string speakerName, List<IDialogueAttribute> attributes) : base(attributes)
        {
            SpeakerName = speakerName;
        }

        #endregion
    }

    /// <summary>
    /// Abstract class for text node.
    /// </summary>
    [System.Serializable]
    public abstract class GenericTextNode : StatementNode
    {
        #region Fields

        public readonly string Speaker;

        #endregion

        #region Constructor

        protected GenericTextNode(string speaker) : base(null)
        {
            Speaker = speaker;
        }

        protected GenericTextNode(string speaker, List<IDialogueAttribute> attributes) : base(attributes)
        {
            Speaker = speaker;
        }

        #endregion
    }

    /// <summary>
    /// Class for normal text node.
    /// </summary>
    [System.Serializable]
    public sealed class TextNode : GenericTextNode
    {
        #region Fields

        public readonly string Content;

        #endregion

        #region Constructor

        public TextNode(string speaker, string content) : base(speaker)
        {
            Content = content;
        }

        public TextNode(string speaker, string content, List<IDialogueAttribute> attributes)
            : base(speaker, attributes)
        {
            Content = content;
        }

        #endregion
    }

    /// <summary>
    /// Class for cross-line text node.
    /// </summary>
    [System.Serializable]
    public sealed class CrossTextNode : GenericTextNode
    {
        #region Fields

        public readonly List<string> Contents;

        #endregion

        #region Constructor

        public CrossTextNode(string speaker, List<string> content) : base(speaker)
        {
            Contents = content;
        }

        public CrossTextNode(string speaker, List<string> content, List<IDialogueAttribute> attributes)
            : base(speaker, attributes)
        {
            Contents = content;
        }

        #endregion
    }

    [System.Serializable]
    public sealed class OptionsNode : StatementNode
    {
        #region Fields

        public readonly List<OptionOfNode> Options = new();

        #endregion

        #region Constructors

        public OptionsNode() : base(null)
        {
        }

        #endregion
    }

    [System.Serializable]
    public struct OptionOfNode
    {
        #region Fields

        /// <summary>
        /// Attribute of this option.
        /// </summary>
        [SerializeReference] public readonly List<IDialogueAttribute> Attributes;

        /// <summary>
        /// The content to display of this option.
        /// </summary>
        public string DisplayContent;

        /// <summary>
        /// The operation to do after chosen this option.
        /// </summary>
        [SerializeReference] public CommandNode Command;

        #endregion

        #region Constructors

        public OptionOfNode(string displayContent, CommandNode command, List<IDialogueAttribute> attributes)
        {
            DisplayContent = displayContent;
            Command = command;
            Attributes = attributes;
        }

        #endregion
    }

    [System.Serializable]
    public abstract class ExpressionNode : Node
    {
    }

    [System.Serializable]
    public sealed class IntValueNode : ExpressionNode
    {
        public int Value;

        public IntValueNode(int value)
        {
            Value = value;
        }
    }

    [System.Serializable]
    public sealed class FloatValueNode : ExpressionNode
    {
        public float Value;

        public FloatValueNode(float value)
        {
            Value = value;
        }
    }

    [System.Serializable]
    public sealed class TrueValueNode : ExpressionNode
    {
    }

    [System.Serializable]
    public sealed class FalseValueNode : ExpressionNode
    {
    }

    [System.Serializable]
    public sealed class VarRefNode : ExpressionNode
    {
        #region Fields

        public string Ref;

        #endregion

        #region Constructors

        public VarRefNode(string @ref)
        {
            Ref = @ref;
        }

        #endregion
    }
}