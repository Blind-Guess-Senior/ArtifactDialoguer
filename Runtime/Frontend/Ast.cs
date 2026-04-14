using System.Collections.Generic;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
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
        public string Namespace;

        /// <summary>
        /// All blocks of this story node.
        /// </summary>
        [SerializeReference] public List<BlockNode> Blocks = new();

        /// <summary>
        /// All scope variables definition.
        /// </summary>
        public List<string> ScopeVars = new();

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
        public string BlockName;

        [SerializeReference] public List<IDialogueAttribute> Attributes;

        #endregion

        /// <summary>
        /// Previous block node in natural flow.
        /// </summary>
        [SerializeReference] public BlockNode NaturalPrevious;

        /// <summary>
        /// Next block node in natural flow.
        /// </summary>
        [SerializeReference] public BlockNode NaturalNext;

        /// <summary>
        /// Statement nodes of this block node.
        /// </summary>
        [SerializeReference] public List<StatementNode> Statements = new();

        #endregion

        #region Constructor

        public BlockNode(string blockName)
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

        [SerializeReference] public List<IDialogueAttribute> Attributes;

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

    /// <summary>
    /// Goto command node for jumping to another dialogue block.
    /// </summary>
    [System.Serializable]
    public sealed class GotoCommandNode : CommandNode
    {
        #region Fields

        [SerializeReference] public BlockNode ToBlock;

        #endregion

        #region Constructor

        public GotoCommandNode(List<IDialogueAttribute> attributes) : base(attributes)
        {
        }

        #endregion
    }

    /// <summary>
    /// Return command node for jumping backward to block that call current block.
    /// </summary>
    [System.Serializable]
    public sealed class RetCommandNode : CommandNode
    {
        #region Fields

        [SerializeReference] public BlockNode BelongsToBlock;

        #endregion

        #region Constructor

        public RetCommandNode(BlockNode belongsTo, List<IDialogueAttribute> attributes) : base(attributes)
        {
            BelongsToBlock = belongsTo;
        }

        #endregion
    }

    /// <summary>
    /// Null command node for no operation needed.
    /// </summary>
    [System.Serializable]
    public sealed class NullCommandNode : CommandNode
    {
        public NullCommandNode() : base(null)
        {
        }
    }

    /// <summary>
    /// Set command node for set a value for a variable. 
    /// </summary>
    [System.Serializable]
    public sealed class SetCommandNode : CommandNode
    {
        #region Fields

        public string TargetVar;

        [SerializeReference] public ExpressionNode ValueExpr;

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

    /// <summary>
    /// Set command node for set a value for a global variable. 
    /// </summary>
    [System.Serializable]
    public sealed class GlobalSetCommandNode : CommandNode
    {
        #region Fields

        public string TargetVar;
        [SerializeReference] public ExpressionNode ValueExpr;

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

        public string SpeakerName;

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

        public string Speaker;

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

        public string Content;

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

        public List<string> Contents;

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

    /// <summary>
    /// Class for options group.
    /// </summary>
    [System.Serializable]
    public sealed class OptionsNode : StatementNode
    {
        #region Fields

        public List<OptionOfNode> Options = new();

        #endregion

        #region Constructors

        public OptionsNode() : base(null)
        {
        }

        #endregion
    }

    /// <summary>
    /// Struct for an option in an options group.
    /// </summary>
    [System.Serializable]
    public sealed class OptionOfNode : Node
    {
        #region Fields

        /// <summary>
        /// Attribute of this option.
        /// </summary>
        [SerializeReference] public List<IDialogueAttribute> Attributes;

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

    /// <summary>
    /// Abstract class for expression node that representing a value.
    /// </summary>
    [System.Serializable]
    public abstract class ExpressionNode : Node
    {
    }

    /// <summary>
    /// A value of type int.
    /// </summary>
    [System.Serializable]
    public sealed class IntValueNode : ExpressionNode
    {
        public int Value;

        public IntValueNode(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// A value of type float.
    /// </summary>
    [System.Serializable]
    public sealed class FloatValueNode : ExpressionNode
    {
        public float Value;

        public FloatValueNode(float value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// A value of type bool true.
    /// </summary>
    [System.Serializable]
    public sealed class TrueValueNode : ExpressionNode
    {
    }

    /// <summary>
    /// A value of type bool false.
    /// </summary>
    [System.Serializable]
    public sealed class FalseValueNode : ExpressionNode
    {
    }

    /// <summary>
    /// A value reference to a variable.
    /// </summary>
    [System.Serializable]
    public sealed class VarRefNode : ExpressionNode
    {
        #region Fields

        /// <summary>
        /// Name of variable that this node refer to.
        /// </summary>
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
