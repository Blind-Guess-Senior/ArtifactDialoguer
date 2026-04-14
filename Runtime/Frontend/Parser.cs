using System;
using System.Collections.Generic;
using System.Linq;
using BlindGuessSenior.ArtifactDialoguer.Utilities.DebugUtils;
using BlindGuessSenior.ArtifactDialoguer.Utilities.Exceptions;
using NUnit.Framework;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
    // TODO: Var usage check -> cfg LiveOut
    // TODO: Explicit command ident, no number start
    // TODO: () for Expr in common if expr. Like `if (cntA + cntB) == cntC`

    /*
     * Story -> NamespaceAssignment Blocks Story
     *        | ϵ
     *
     * Blocks -> Block Blocks
     *         | ϵ
     *
     * Block -> AttributeStatement _BlockDecl_ Expr
     *
     * NamespaceAssignment -> _::_ AssignText _::_
     *
     * Expr -> ExprText Expr
     *       | Speaker Expr
     *       | Command Expr
     *       | OptionStatement Expr
     *       | AttributeStatementLess Expr
     *       | ϵ
     *
     * Speaker -> _@_ AssignText
     *
     * Command -> _Goto_ _CommandIdent_
     *          | _Ret_
     *          | _Null_
     *          | _Set_ _CommandIdent_ _=_ CommandExpr
     *
     * OptionStatement -> _?_ OptionText _->_ OptionBody
     *
     * OptionText -> _Text_
     *
     * OptionBody -> Command
     *
     * AttributeStatement -> _Once_
     *                     | _Cycle_
     *                     | _Unreach_
     *                     | ϵ
     *
     * AttributeStatementLess -> _Once_
     *                         | _If_ CommandExpr RelevantOperator CommandExpr
     *                         | ϵ
     *
     * ExprText -> _Text_
     *           | _CrossText_
     *
     * AssignText -> _Text_
     *
     * RelevantOperator -> _==_
     *                   | _!=_
     *                   | _<_
     *                   | _<=_
     *                   | _>_
     *                   | _>=_
     *
     * ArithmeticOperatorL1 -> _+_
     *                       | _-_
     *
     * ArithmeticOperatorL2 -> _*_
     *                       | _/_
     *                       | _%_
     *
     * CommandExpr -> CommandTerm CommandExpr'
     *
     * CommandExpr' -> ArithmeticOperatorL1 CommandTerm CommandExpr'
     *               | ϵ
     *
     * CommandTerm -> CommandFactor CommandTerm'
     *
     * CommandTerm' -> ArithmeticOperatorL2 CommandFactor CommandTerm'
     *               | ϵ
     *
     * CommandFactor -> _(_ CommandExpr _)_
     *                | CommandValue
     *
     * CommandValue -> _CommandInt_
     *               | _CommandFloat_
     *               | _CommandBool_
     *               | _CommandIdent_
     *
     */
    /*          | Story | Blocks                             | Block                            | NamespaceAssignment
     * First    | ::, ϵ | BlockDecl, Cycle, Once, Unreach, ϵ | BlockDecl, Cycle, Once, Unreach  | ::
     *
     *          | Expr
     * First    | ?, @, Text, CrossText, Goto, Ret, Null, Set, Once, If, ϵ
     *
     *          | Speaker | OptionStatement | OptionText  | OptionBody
     * First    | @       | ?               | Text        | Goto, Ret, Null, Set
     *
     *          | Command              | ExprText        | AssignText
     * First    | Goto, Ret, Null, Set | Text, CrossText | Text
     *
     *          | AttributeStatement      | AttributeStatementLess    | RelevantOperator     | ArithmeticOperatorL1 | ArithmeticOperatorL2
     * First    | Cycle, Once, Unreach, ϵ | Once, If, ϵ               | !=, <, <=, ==, >, >= | +, -                 | *, /, %
     *
     *          | CommandExpr                                            | CommandExpr'
     * First    | (, CommandBool, CommandFloat, CommandInt, CommandIdent | +, -, ϵ
     *
     *          | CommandTerm                                            | CommandTerm'
     * First    | (, CommandBool, CommandFloat, CommandInt, CommandIdent | *, /, %, ϵ
     *
     *          | CommandFactor                                          | CommandValue
     * First    | (, CommandBool, CommandFloat, CommandInt, CommandIdent | CommandBool, CommandFloat, CommandInt, CommandIdent
     */
    /* $ for eof
     *          | Story | Blocks | Block                                  | NamespaceAssignment
     * Follow   | $     | $, ::  | $, ::, BlockDecl, Cycle, Once, Unreach | $, ::, BlockDecl, Cycle, Once, Unreach
     *
     *          | Expr
     * Follow   | $, ::, BlockDecl, Cycle, Once, Unreach
     *
     *          | Speaker
     * Follow   | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | OptionStatement
     * Follow   | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | OptionText | OptionBody
     * Follow   | ->         | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | Command
     * Follow   | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | ExprText
     * Follow   | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | AssignText
     * Follow   | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | AttributeStatement | AttributeStatementLess
     * Follow   | BlockDecl          | $, ::, ?, @, Text, CrossText, BlockDecl, Goto, Ret, Null, Set, Cycle, Once, Unreach, If
     *
     *          | RelevantOperator
     * Follow   | (, CommandBool, CommandFloat, CommandInt, CommandIdent
     *
     *          | ArithmeticOperatorL1                                   | ArithmeticOperatorL2
     * Follow   | (, CommandBool, CommandFloat, CommandInt, CommandIdent | (, CommandBool, CommandFloat, CommandInt, CommandIdent
     *
     *          | CommandExpr
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, !=, <, <=, ==, >, >=
     *
     *          | CommandExpr'
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, !=, <, <=, ==, >, >=
     *
     *          | CommandTerm
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, +, -, !=, <, <=, ==, >, >=
     *
     *          | CommandTerm'
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, +, -, !=, <, <=, ==, >, >=
     *
     *          | CommandFactor
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, +, -, *, /, %, !=, <, <=, ==, >, >=
     *
     *          | CommandValue
     * Follow   | $, ::, ?, @, ), Text, CrossText, BlockDecl, Goto, Null, Ret, Set, Cycle, Once, Unreach, If, +, -, *, /, %, !=, <, <=, ==, >, >=
     */

    public class ParserInstance
    {
        #region Fields

        /// <summary>
        /// Tokens to parse.
        /// </summary>
        private List<Token> _tokens;

        /// <summary>
        /// Index of current token.
        /// </summary>
        private int _position;

        /// <summary>
        /// Current token (will be consume when Advance())
        /// </summary>
        private Token _currentToken;

        private readonly Dictionary<string, Type> _attributesMap = new()
        {
            { "once", typeof(OnceAttribute) },
            { "cycle", typeof(CycleAttribute) },
            { "unreach", typeof(UnreachAttribute) },
        };

        /// <summary>
        /// Current namespace literal.
        /// </summary>
        private NamespaceStoryNode _currentNamespaceNode;

        /// <summary>
        /// Storage of all namespace stories.
        /// </summary>
        private readonly Dictionary<string, NamespaceStoryNode> _allNamespaceStories = new();

        /// <summary>
        /// Current block node.
        /// </summary>
        private BlockNode _currentBlockNode;

        /// <summary>
        /// Map from block's identification to block node reference.
        /// </summary>
        private readonly Dictionary<string, BlockNode> _blockMap = new();

        /// <summary>
        /// Previous block in natural flow.
        /// </summary>
        private BlockNode _naturalPreviousBlock;

        /// <summary>
        /// Whether there is a namespace switch (often cause by file switch) to break natural flow.
        /// </summary>
        private bool _isNaturalFlowBreak;

        /// <summary>
        /// Speaker in current context, used to inject text node.
        /// </summary>
        private SpeakerNode _currentSpeakerNode;

        /// <summary>
        /// Temp storage for 'goto' command, used to inject right block node after parse.
        /// </summary>
        private readonly Dictionary<GotoCommandNode, Tuple<string, Token>> _commandGotoMap = new();

        /// <summary>
        /// Current options group node.
        /// </summary>
        private OptionsNode _currentOptions;

        /// <summary>
        /// Temp storage for current attributes that will apply to next valid token.
        /// </summary>
        private List<IDialogueAttribute> _currentAttributes = new();

        // /// <summary>
        // /// All global variables.
        // /// </summary>
        // private readonly HashSet<string> _globalVars = new();

        /// <summary>
        /// All valid relation operator token types.
        /// </summary>
        private static readonly List<TokenType> ValidRelationOperators = new()
        {
            TokenType.DoubleEqual,
            TokenType.NotEqual,
            TokenType.GreaterThan,
            TokenType.LessThan,
            TokenType.GreaterThanEqual,
            TokenType.LessThanEqual,
        };

        #endregion

        #region Methods

        /// <summary>
        /// Reset current token list.
        /// </summary>
        /// <param name="inputTokens">The tokens to parse.</param>
        public void SetInput(List<Token> inputTokens)
        {
            _tokens = inputTokens;
            _position = 0;
            if (_tokens.Count > 0)
            {
                _currentToken = _tokens[0];
            }
        }

        /// <summary>
        /// Collect result after parse.
        /// </summary>
        /// <returns>All dialogue node after parse.</returns>
        public List<NamespaceStoryNode> CollectResult()
        {
            // Post process
            InjectGotoBlock();

            return _allNamespaceStories.Values.ToList();
        }

        #region Process Methods

        /// <summary>
        /// Peek next token without move index.
        /// </summary>
        /// <returns>Next token if it is not in the end; otherwise, last token in list.</returns>
        private Token PeekToken()
            => _position >= _tokens.Count ? _tokens[^1] : _tokens[_position + 1];

        /// <summary>
        /// Move index to next token and return the token that be consumed.
        /// </summary>
        /// <returns>Current token before advance.</returns>
        private Token Advance()
        {
            var token = _currentToken;
            if (_position < _tokens.Count - 1)
            {
                _currentToken = _tokens[++_position];
            }

            return token;
        }

        /// <summary>
        /// Match a token and advance it.
        /// </summary>
        /// <param name="expectedType">The type of token to match.</param>
        /// <returns>Current token before match.</returns>
        /// <exception cref="UnexpectedAttributeException">Occur when unexpected attribute token appear.</exception>
        /// <exception cref="MismatchSyntaxException">Occur when next token is not expected token.</exception>
        private Token Match(TokenType expectedType)
        {
            if (_currentToken.Type == expectedType)
                return Advance();

            if (_currentToken.Type == TokenType.Attribute)
            {
                throw new UnexpectedAttributeException(_currentToken);
            }

            throw new MismatchSyntaxException(expectedType, _currentToken);
        }

        #endregion

        #region Unterminator Parser

        /// <summary>
        /// Entry function for this LL(1) parser. It will parse all token.
        /// </summary>
        /// <returns></returns>
        public void ParseNamespaceStory()
        {
            while (_currentToken.Type == TokenType.DoubleColon)
            {
                Match(TokenType.DoubleColon);
                var namespaceToken = Match(TokenType.Text);
                Match(TokenType.DoubleColon);

                NamespaceStoryNode story;
                if (_allNamespaceStories.TryGetValue(namespaceToken.Literal, out var existedStory))
                {
                    story = existedStory;
                }
                else
                {
                    story = new NamespaceStoryNode(namespaceToken.Literal);
                    _allNamespaceStories.Add(namespaceToken.Literal, story);
                }

                _currentNamespaceNode = story;
                _currentBlockNode = null;
                _naturalPreviousBlock = null;
                _currentSpeakerNode = null;
                _currentOptions = null;

                ParseBlocks(story);
            }
        }

        /// <summary>
        /// Parser for Blocks rule.
        /// </summary>
        /// <param name="namespaceStory">The node that blocks mount to.</param>
        private void ParseBlocks(NamespaceStoryNode namespaceStory)
        {
            while (true)
            {
                ParseAttributes();
                if (_currentToken.Type != TokenType.BlockDecl)
                {
                    break;
                }

                var blockAttribute = _currentAttributes.ToList();
                _currentAttributes.Clear();
                foreach (var attr in blockAttribute)
                {
                    attr.IsAllowed(_currentToken);
                }

                var block = ParseBlock(blockAttribute);

                namespaceStory.AddBlock(block);
            }
        }

        /// <summary>
        /// Parser for Block rule.
        /// </summary>
        /// <param name="attributes">The attributes of this block.</param>
        /// <returns>The parsed block node.</returns>
        /// <exception cref="MultipleBlockDefinitionException"></exception>
        /// <exception cref="ConflictAttributeException"></exception>
        private BlockNode ParseBlock(List<IDialogueAttribute> attributes)
        {
            var declToken = Match(TokenType.BlockDecl);
            var block = new BlockNode(declToken.Literal, attributes);

            try
            {
                _blockMap.Add(
                    BlockIdentificationName(new Tuple<string, Token>(_currentNamespaceNode.Namespace, declToken)),
                    block);
            }
            catch (ArgumentException)
            {
                throw new MultipleBlockDefinitionException(_currentNamespaceNode.Namespace, declToken);
            }

            _currentBlockNode = block;

            // Reset current speaker
            _currentSpeakerNode = null;
            // Reset current option group
            _currentOptions = null;

            if (attributes.Any(attr => attr is UnreachAttribute) &&
                attributes.Any(attr => attr is CycleAttribute))
            {
                throw new ConflictAttributeException(attributes.ToArray(), _currentToken);
            }

            if (!attributes.Any(attr => attr is UnreachAttribute))
            {
                if (_naturalPreviousBlock != null)
                {
                    _naturalPreviousBlock.NaturalNext = block;
                    block.NaturalPrevious = _naturalPreviousBlock;
                }

                if (attributes.Any(attr => attr is CycleAttribute))
                {
                    block.NaturalNext = block;
                    _naturalPreviousBlock = null;
                }
                else
                {
                    _naturalPreviousBlock = block;
                }
            }

            ParseExpr(block.Statements);

            return block;
        }

        /// <summary>
        /// Parser for Expr rule. Parse statements and append them into block node's statements.
        /// </summary>
        /// <param name="statements">The statements to parse</param>
        /// <exception cref="UnmatchedAttributeException">Occur when attribute unmatched.</exception>
        /// <exception cref="IsolatedAttributeException">Occur when isolated attribute found.</exception>
        /// <exception cref="UnexpectedTokenException">Occur when token is not in process range.</exception>
        private void ParseExpr(List<StatementNode> statements)
        {
            while (_currentToken.Type != TokenType.EOF &&
                   _currentToken.Type != TokenType.BlockDecl &&
                   _currentToken.Type != TokenType.DoubleColon) // Tail recursion -> loop
            {
                ParseAttributes();

                // Back to block parser
                if (_currentToken.Type == TokenType.BlockDecl)
                {
                    break;
                }

                // Namespace assignment shall not have attribute
                if (_currentToken.Type == TokenType.DoubleColon)
                {
                    throw new UnmatchedAttributeException(_currentAttributes[0], PeekToken());
                }

                // Isolated attribute
                if (_currentToken.Type == TokenType.EOF)
                {
                    throw new IsolatedAttributeException(_currentAttributes[0], _currentToken);
                }

                var attributes = _currentAttributes.ToList();
                _currentAttributes.Clear();

                // Why don't C# provide a way to fallthrough?!?
                if (_currentToken.Type != TokenType.Question)
                {
                    if (_currentOptions != null)
                    {
                        statements.Add(_currentOptions);
                    }

                    _currentOptions = null;
                }

                switch (_currentToken.Type)
                {
                    case TokenType.At: // Speaker
                        var curToken = Match(TokenType.At);

                        foreach (var attr in attributes)
                        {
                            attr.IsAllowed(curToken);
                        }

                        var speakerToken = Match(TokenType.Text);
                        var speakerNode = new SpeakerNode(speakerToken.Literal, attributes);

                        _currentSpeakerNode = speakerNode;

                        statements.Add(speakerNode);
                        break;
                    case TokenType.Text:
                        var textToken = Match(TokenType.Text);

                        foreach (var attr in attributes)
                        {
                            attr.IsAllowed(textToken);
                        }

                        var textNode = new TextNode(_currentSpeakerNode.SpeakerName, textToken.Literal, attributes);

                        statements.Add(textNode);
                        break;
                    case TokenType.CrossText:
                        var crossTextToken = Match(TokenType.CrossText);

                        foreach (var attr in attributes)
                        {
                            attr.IsAllowed(crossTextToken);
                        }

                        var crossTextNode = new CrossTextNode(_currentSpeakerNode.SpeakerName,
                            crossTextToken.Literal.Replace("\r\n", "\n").Split('\n').ToList(),
                            attributes);

                        statements.Add(crossTextNode);
                        break;
                    case TokenType.Goto:
                        Match(TokenType.Goto);

                        var gotoBlock = Match(TokenType.CommandIdent);

                        var gotoNode = new GotoCommandNode(attributes);

                        _commandGotoMap.Add(gotoNode,
                            new Tuple<string, Token>(_currentNamespaceNode.Namespace, gotoBlock));

                        statements.Add(gotoNode);
                        break;
                    case TokenType.Return:
                        Match(TokenType.Return);

                        var returnNode = new RetCommandNode(_currentBlockNode, attributes);

                        statements.Add(returnNode);
                        break;
                    case TokenType.Null:
                        Match(TokenType.Null);
                        var nullNode = new NullCommandNode();
                        statements.Add(nullNode);
                        break;
                    case TokenType.Set:
                        Match(TokenType.Set);

                        var varToken = Match(TokenType.CommandIdent);
                        Match(TokenType.Equal);
                        var valueExpr = ParseValueExpression();
                        if (varToken.Literal.StartsWith('%'))
                        {
                            var setNode = new GlobalSetCommandNode(varToken.Literal[1..], valueExpr, attributes);
                            // _globalVars.Add(setNode.TargetVar);
                            statements.Add(setNode);
                        }
                        else
                        {
                            var setNode = new SetCommandNode(varToken.Literal, valueExpr, attributes);
                            _currentNamespaceNode.ScopeVars.Add(setNode.TargetVar);
                            statements.Add(setNode);
                        }

                        break;
                    case TokenType.Question: // Option group
                        ParseOption(attributes);
                        break;
                    default:
                        throw new UnexpectedTokenException(_currentToken);
                }
            }

            if (_currentOptions != null)
            {
                statements.Add(_currentOptions);
                _currentOptions = null;
            }

            if (_currentToken.Type == TokenType.EOF)
            {
                Match(TokenType.EOF);
            }
        }

        /// <summary>
        /// Parser for Option rule. Parse option group statement.
        /// </summary>
        /// <param name="optionAttributes">The attributes of this option.</param>
        /// <exception cref="WrongOptionBodyException">Occur when option's body is not command.</exception>
        private void ParseOption(List<IDialogueAttribute> optionAttributes)
        {
            foreach (var attr in optionAttributes)
            {
                attr.IsAllowed(_currentToken);
            }

            Match(TokenType.Question);
            var displayContent = Match(TokenType.Text);
            Match(TokenType.Arrow);

            OptionOfNode optionOfNode;
            ParseAttributes();

            var bodyAttributes = _currentAttributes.ToList();
            _currentAttributes.Clear();

            foreach (var attr in bodyAttributes)
            {
                attr.IsAllowed(_currentToken);
            }

            switch (_currentToken.Type)
            {
                case TokenType.Goto:
                    Match(TokenType.Goto);

                    var gotoBlock = Match(TokenType.CommandIdent);
                    var gotoNode = new GotoCommandNode(bodyAttributes);

                    _commandGotoMap.Add(gotoNode, new Tuple<string, Token>(_currentNamespaceNode.Namespace, gotoBlock));

                    optionOfNode = new OptionOfNode(displayContent.Literal, gotoNode, optionAttributes);
                    break;
                case TokenType.Return:
                    Match(TokenType.Return);

                    var returnNode = new RetCommandNode(_currentBlockNode, bodyAttributes);

                    optionOfNode = new OptionOfNode(displayContent.Literal, returnNode, optionAttributes);
                    break;
                case TokenType.Null:
                    Match(TokenType.Null);

                    var nullNode = new NullCommandNode();

                    optionOfNode = new OptionOfNode(displayContent.Literal, nullNode, optionAttributes);
                    break;
                case TokenType.Set:
                    Match(TokenType.Set);

                    var varToken = Match(TokenType.CommandIdent);
                    Match(TokenType.Equal);
                    var valueExpr = ParseValueExpression();
                    if (varToken.Literal.StartsWith('%'))
                    {
                        var setNode = new GlobalSetCommandNode(varToken.Literal[1..], valueExpr, bodyAttributes);
                        // _globalVars.Add(setNode.TargetVar);
                        optionOfNode = new OptionOfNode(displayContent.Literal, setNode, optionAttributes);
                    }
                    else
                    {
                        var setNode = new SetCommandNode(varToken.Literal, valueExpr, bodyAttributes);
                        _currentNamespaceNode.ScopeVars.Add(setNode.TargetVar);
                        optionOfNode = new OptionOfNode(displayContent.Literal, setNode, optionAttributes);
                    }

                    break;
                default:
                    throw new WrongOptionBodyException(_currentToken);
            }

            if (_currentOptions == null)
            {
                _currentOptions = new OptionsNode();
            }

            _currentOptions.Options.Add(optionOfNode);
        }

        /// <summary>
        /// Parse attributes and append it into temp attribute storage.
        /// </summary>
        /// <exception cref="InvalidOperatorException">Occur when found invalid operation in 'If' attribute.</exception>
        private void ParseAttributes()
        {
            while (_currentToken.Type == TokenType.Attribute || _currentToken.Type == TokenType.If)
            {
                if (_currentToken.Type == TokenType.Attribute)
                {
                    var attrToken = Advance();

                    Assert.IsTrue(_attributesMap.ContainsKey(attrToken.Literal));

                    var attrType = _attributesMap[attrToken.Literal];

                    _currentAttributes.Add(Activator.CreateInstance(attrType) as IDialogueAttribute);
                }
                else
                {
                    Match(TokenType.If);
                    var left = ParseValueExpression();

                    var opToken = _currentToken;
                    if (!ValidRelationOperators.Contains(opToken.Type))
                    {
                        throw new InvalidOperatorException(opToken);
                    }

                    Match(opToken.Type);

                    var right = ParseValueExpression();

                    var ifAttr = new IfAttribute(left, opToken.Type, right);

                    _currentAttributes.Add(ifAttr);
                }
            }
        }

        /// <summary>
        /// Parse value expression and return parsed expression node.
        /// </summary>
        /// <returns>The parsed expression node.</returns>
        /// <exception cref="NoExpressionException">Occur when no expression found.</exception>
        private ExpressionNode ParseValueExpression()
        {
            // TODO: complex value expression
            switch (_currentToken.Type)
            {
                case TokenType.CommandInt:
                {
                    var token = Match(TokenType.CommandInt);
                    return new IntValueNode(int.Parse(token.Literal));
                }
                case TokenType.CommandFloat:
                {
                    var token = Match(TokenType.CommandFloat);
                    return new FloatValueNode(float.Parse(token.Literal));
                }
                case TokenType.CommandTrue:
                {
                    var token = Match(TokenType.CommandTrue);
                    return new TrueValueNode();
                }
                case TokenType.CommandFalse:
                {
                    var token = Match(TokenType.CommandFalse);
                    return new FalseValueNode();
                }
                case TokenType.CommandIdent:
                {
                    var token = Match(TokenType.CommandIdent);
                    return new VarRefNode(token.Literal);
                }
                default:
                    throw new NoExpressionException(_currentToken);
            }
        }

        #endregion

        #region Post Process Methods

        /// <summary>
        /// Inject block node to all goto node.
        /// </summary>
        /// <exception cref="UndefinedReferenceException">Occur when goto's target block is undefined.</exception>
        private void InjectGotoBlock()
        {
            foreach (var @goto in _commandGotoMap)
            {
                if (_blockMap.TryGetValue(BlockIdentificationName(@goto.Value), out var targetBlock))
                {
                    @goto.Key.ToBlock = targetBlock;
                }
                else
                {
                    throw new UndefinedBlockReferenceException(@goto.Value);
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get block's identification name that can uniquely determine that block.
        /// </summary>
        /// <param name="tuple">The block and additional info. Item1 for namespace, Item2 for block token.</param>
        /// <returns>The identification name of given block.</returns>
        private static string BlockIdentificationName(Tuple<string, Token> tuple)
            => $"_%{tuple.Item1}_%{tuple.Item2.Literal}";

        #endregion

        #endregion
    }

    public static class Parser
    {
        /// <summary>
        /// Parse a sequence of token lists, and return parsed nodes.
        /// </summary>
        /// <param name="allTokens">The sequence of token lists to parse.</param>
        /// <returns>The parsed nodes.</returns>
        public static List<NamespaceStoryNode> Parse(IEnumerable<List<Token>> allTokens)
        {
            var parser = new ParserInstance();
            var tokens = allTokens.ToList();
            foreach (var oneTokens in tokens)
            {
                parser.SetInput(oneTokens);
                parser.ParseNamespaceStory();
            }

            ArtifactDialoguerDebug.CompileLog($"Compile {tokens.Count} groups tokens.");

            return parser.CollectResult();
        }
    }
}