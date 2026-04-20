using System;
using System.Collections.Generic;
using System.Linq;
using BlindGuessSenior.ArtifactDialoguer.Frontend;
using BlindGuessSenior.ArtifactDialoguer.Utilities.DebugUtils;
using BlindGuessSenior.ArtifactDialoguer.Utilities.Exceptions;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Backend
{
    using ExpressionEvaluateResult = Tuple<Type, object>;
    // Item1 for block, Item2 for next statement index in block.
    using CallStackEntry = Tuple<BlockNode, int>;

    /// <summary>
    /// Interface for dialogue runner.
    /// </summary>
    public interface IDialogueRunner
    {
        public void Init();
        public IDialogueRuntimeResult Next();
        public IDialogueRuntimeResult OptionChosen(int index);
        public IDialogueRuntimeResult OptionChosen(DialogueRuntimeResultOption option);

        public DialogueState ExportSave(bool blockLevelOnly = false);

        public void LoadSave(DialogueState state);
    }

    /// <summary>
    /// Common dialogue runner.
    /// </summary>
    public class DialogueRunner : MonoBehaviour, IDialogueRunner
    {
        #region Fields

        /// <summary>
        /// Dialogue state storage.
        /// </summary>
        private readonly DialogueState _dialogueState = new()
        {
            OnceStatement = new HashSet<int>(),
            Variables = new Dictionary<string, ExpressionEvaluateResult>(),
            CallStack = new Stack<CallStackEntry>(),
        };

        private List<DialogueRuntimeResultOption> _currentOptionList = new();

        [Header("Dialogue start params")] public DialogueStorageObject dialogueStorageObject;
        public string startBlock;

        #endregion

        #region Methods

        #region Unity Event Methods

        private void Awake()
        {
            Init();
        }

        #endregion

        #region Runtime Methods

        /// <summary>
        /// Init dialogue runner.
        /// </summary>
        public void Init()
        {
            if (!dialogueStorageObject)
            {
                ArtifactDialoguerDebug.PackageLog("Dialogue file not found!", DebugLogLevel.Fatal);
            }

            _dialogueState.IsCrossTextContext = false;
            _dialogueState.IsOptionContext = false;

            if (string.IsNullOrEmpty(startBlock))
            {
                ArtifactDialoguerDebug.PackageLog("Dialogue start block not found!", DebugLogLevel.Warning);
            }

            var block = dialogueStorageObject.Blocks.Find(b => b.BlockName == startBlock);

            if (block != null)
            {
                _dialogueState.CurrentBlock = block;
            }
            else
            {
                ArtifactDialoguerDebug.PackageLog("Dialogue start block not found! Using first block as default.",
                    DebugLogLevel.Warning);
                _dialogueState.CurrentBlock = dialogueStorageObject.Blocks[0];
            }
        }

        /// <summary>
        /// Make dialogue go on for one step.
        /// </summary>
        /// <returns>The result after dialogue move forward.</returns>
        public IDialogueRuntimeResult Next()
        {
            while (true)
            {
                if (_dialogueState.IsOptionContext)
                {
                    return new DialogueRuntimeResultNextDenied();
                }

                if (_dialogueState.CurrentBlock == null)
                {
                    ArtifactDialoguerDebug.RuntimeLog("No current dialogue block found.", DebugLogLevel.Warning);
                    return new DialogueRuntimeResultError();
                }

                if (_dialogueState.CurrentBlockStatementIndex >= _dialogueState.CurrentBlock.Statements.Count &&
                    !_dialogueState.IsCrossTextContext)
                {
                    _dialogueState.IsCrossTextContext = false;
                    _dialogueState.IsOptionContext = false;
                    _dialogueState.CurrentCrossTextStatementIndex = 0;

                    if (_dialogueState.CurrentBlock.NaturalNext != null)
                    {
                        _dialogueState.CurrentBlock = _dialogueState.CurrentBlock.NaturalNext;
                        _dialogueState.CurrentBlockStatementIndex = 0;
                        return new DialogueRuntimeResultBlockEnd();
                    }
                    else
                    {
                        return new DialogueRuntimeResultDialogueEnd();
                    }
                }

                List<IfAttribute> conditions;

                // Check block's attribute
                if (_dialogueState.CurrentBlockStatementIndex == 0)
                {
                    conditions = _dialogueState.CurrentBlock.Attributes?.OfType<IfAttribute>().ToList();
                    if (!CheckConditions(conditions))
                    {
                        _dialogueState.CurrentBlock = _dialogueState.CurrentBlock.NaturalNext;
                        _dialogueState.CurrentBlockStatementIndex = 0;
                        goto Continue;
                    }

                    if (_dialogueState.CurrentBlock.Attributes?.Any(attr => attr is OnceAttribute) ?? false)
                    {
                        if (!_dialogueState.OnceStatement.Add(_dialogueState.CurrentBlock.NodeId))
                        {
                            _dialogueState.CurrentBlock = _dialogueState.CurrentBlock.NaturalNext;
                            _dialogueState.CurrentBlockStatementIndex = 0;
                            goto Continue;
                        }
                    }
                }

                StatementNode statement;
                if (_dialogueState.IsCrossTextContext)
                {
                    statement = _dialogueState.CurrentBlock.Statements[_dialogueState.CurrentCrossTextStatementIndex];
                    goto StatementActions;
                }

                statement = _dialogueState.CurrentBlock.Statements[_dialogueState.CurrentBlockStatementIndex];

                _dialogueState.CurrentBlockStatementIndex++;

                // Check statement's attribute
                conditions = statement.Attributes.OfType<IfAttribute>().ToList();
                if (!CheckConditions(conditions))
                {
                    goto Continue;
                }

                if (statement.Attributes?.Any(attr => attr is OnceAttribute) ?? false)
                {
                    if (!_dialogueState.OnceStatement.Add(statement.NodeId)) // Found got once
                    {
                        switch (statement)
                        {
                            case SpeakerNode: // Special check for skip whole speaker
                                while (true)
                                {
                                    if (_dialogueState.CurrentBlockStatementIndex >=
                                        _dialogueState.CurrentBlock.Statements.Count)
                                    {
                                        return new DialogueRuntimeResultBlockEnd();
                                    }

                                    if (_dialogueState.CurrentBlock.Statements[
                                            _dialogueState.CurrentBlockStatementIndex] is
                                        SpeakerNode)
                                    {
                                        break;
                                    }

                                    _dialogueState.CurrentBlockStatementIndex++;
                                }

                                break;
                        }

                        goto Continue;
                    }
                }

                StatementActions:
                switch (statement)
                {
                    case SpeakerNode:
                        goto Continue;
                    case TextNode textNode:
                        return new DialogueRuntimeResultTextGot(textNode.Speaker, textNode.Content);
                    case CrossTextNode crossTextNode:
                        // TODO: may need better way to pass cross text
                        if (!_dialogueState.IsCrossTextContext)
                        {
                            _dialogueState.IsCrossTextContext = true;
                            _dialogueState.CurrentCrossTextIndex = 0;
                            _dialogueState.CurrentCrossTextStatementIndex =
                                _dialogueState.CurrentBlockStatementIndex - 1;
                        }

                        if (_dialogueState.CurrentCrossTextIndex >= crossTextNode.Contents.Count)
                        {
                            _dialogueState.IsCrossTextContext = false;
                            goto Continue;
                        }

                        var content = string.Join('\n',
                            crossTextNode.Contents.Take(_dialogueState.CurrentCrossTextIndex + 1));
                        _dialogueState.CurrentCrossTextIndex++;
                        return new DialogueRuntimeResultTextGot(crossTextNode.Speaker, content);
                    case GotoCommandNode gotoCommandNode:
                        _dialogueState.CallStackPush();

                        _dialogueState.CurrentBlock = gotoCommandNode.ToBlock;
                        _dialogueState.CurrentBlockStatementIndex = 0;

                        goto Continue;
                    case RetCommandNode:
                        if (_dialogueState.CallStack.Count == 0)
                        {
                            break;
                        }

                        var ret = _dialogueState.CallStackPop();
                        _dialogueState.CurrentBlock = ret.Item1;
                        _dialogueState.CurrentBlockStatementIndex = ret.Item2;

                        goto Continue;
                    case NullCommandNode:
                        goto Continue;
                    case SetCommandNode setCommandNode:
                        // TODO: complex value expression
                        var targetVar = setCommandNode.TargetVar;
                        if (_dialogueState.Variables.ContainsKey(targetVar))
                        {
                            var result = EvaluateExpression(setCommandNode.ValueExpr);
                            _dialogueState.Variables[targetVar] = result;
                        }
                        else
                        {
                            var result = EvaluateExpression(setCommandNode.ValueExpr);
                            _dialogueState.Variables.Add(targetVar, result);
                        }

                        goto Continue;
                    case OptionsNode optionsNode:
                        List<DialogueRuntimeResultOption> options = new();

                        var index = 0;
                        foreach (var option in optionsNode.Options)
                        {
                            var attr = option.Attributes;
                            conditions = attr.OfType<IfAttribute>().ToList();
                            if (!CheckConditions(conditions))
                            {
                                continue;
                            }

                            if (attr.Any(att => att is OnceAttribute))
                            {
                                if (_dialogueState.OnceStatement.Contains(option.NodeId))
                                {
                                    continue;
                                }

                                options.Add(new DialogueRuntimeResultOption(index++, option.DisplayContent,
                                    option.Command, option, true));
                                continue;
                            }

                            options.Add(new DialogueRuntimeResultOption(index++, option.DisplayContent,
                                option.Command, option));
                        }

                        _dialogueState.IsOptionContext = true;
                        _currentOptionList = options;

                        return new DialogueRuntimeResultOptionsGot(options);
                }

                return new DialogueRuntimeResultDialogueEnd();

                Continue: ;
            }
        }

        /// <summary>
        /// Chose an option and went on.
        /// </summary>
        /// <param name="index">The index of chosen option.</param>
        /// <returns>The result after dialogue move forward.</returns>
        /// <exception cref="OptionOutOfRangeException">Occur when index cannot represent an option (out of range).</exception>
        public IDialogueRuntimeResult OptionChosen(int index)
        {
            if (index >= _currentOptionList.Count)
            {
                throw new OptionOutOfRangeException(index, _currentOptionList.Count);
            }

            _dialogueState.IsOptionContext = false;
            var chosenOption = _currentOptionList[index];

            if (chosenOption.Once)
            {
                _dialogueState.OnceStatement.Add(chosenOption.Node.NodeId);
            }

            var command = chosenOption.Command;

            switch (command)
            {
                case GotoCommandNode gotoCommandNode:
                    _dialogueState.CallStackPush();

                    _dialogueState.CurrentBlock = gotoCommandNode.ToBlock;
                    _dialogueState.CurrentBlockStatementIndex = 0;
                    break;
                case RetCommandNode:
                    if (_dialogueState.CallStack.Count == 0)
                    {
                        break;
                    }

                    var ret = _dialogueState.CallStackPop();
                    _dialogueState.CurrentBlock = ret.Item1;
                    _dialogueState.CurrentBlockStatementIndex = ret.Item2;
                    break;
                case NullCommandNode:
                    break;
                case SetCommandNode setCommandNode:
                    // TODO: complex value expression
                    var targetVar = setCommandNode.TargetVar;
                    if (_dialogueState.Variables.ContainsKey(targetVar))
                    {
                        var result = EvaluateExpression(setCommandNode.ValueExpr);
                        _dialogueState.Variables[targetVar] = result;
                    }
                    else
                    {
                        var result = EvaluateExpression(setCommandNode.ValueExpr);
                        _dialogueState.Variables.Add(targetVar, result);
                    }

                    break;
            }

            return Next();
        }

        /// <summary>
        /// Overload for passing an option result directly. Chose an option and went on.
        /// </summary>
        /// <param name="option">The chosen option.</param>
        /// <returns>The result after dialogue move forward.</returns>
        public IDialogueRuntimeResult OptionChosen(DialogueRuntimeResultOption option)
            => OptionChosen(option.Index);


        /// <summary>
        /// Check whether given if conditions are all met.
        /// </summary>
        /// <param name="conditions">The conditions to check.</param>
        /// <returns>True if all conditions met; otherwise, false.</returns>
        private bool CheckConditions(List<IfAttribute> conditions)
        {
            foreach (var cond in conditions)
            {
                var lhs = EvaluateExpression(cond.LHS);
                var op = cond.Operator;
                var rhs = EvaluateExpression(cond.RHS);
                if (!CompareResults(lhs, op, rhs))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method for compare two expression evaluate result.
        /// </summary>
        /// <param name="lhs">The left hand side result.</param>
        /// <param name="op">The compare operator.</param>
        /// <param name="rhs">The right hand side result.</param>
        /// <returns>True if two result meet given relationship; otherwise, false.</returns>
        private static bool CompareResults(ExpressionEvaluateResult lhs, TokenType op, ExpressionEvaluateResult rhs)
        {
            var leftType = lhs.Item1;
            var rightType = rhs.Item1;
            var leftObject = lhs.Item2;
            var rightObject = rhs.Item2;

            if (leftObject == null || rightObject == null)
            {
                return false;
            }

            if (leftType != rightType)
            {
                return false;
            }

            if (leftObject is IComparable compL && rightObject is IComparable compR)
            {
                var cmp = compL.CompareTo(compR);
                switch (op)
                {
                    case TokenType.DoubleEqual: return cmp == 0;
                    case TokenType.NotEqual: return cmp != 0;
                    case TokenType.LessThan: return cmp < 0;
                    case TokenType.LessThanEqual: return cmp <= 0;
                    case TokenType.GreaterThan: return cmp > 0;
                    case TokenType.GreaterThanEqual: return cmp >= 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluated given value expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The result after evaluating.</returns>
        private ExpressionEvaluateResult EvaluateExpression(ExpressionNode expression)
        {
            // TODO: complex value expression
            switch (expression)
            {
                case IntValueNode intValueNode:
                    return new ExpressionEvaluateResult(typeof(int), intValueNode.Value);
                case FloatValueNode floatValueNode:
                    return new ExpressionEvaluateResult(typeof(float), floatValueNode.Value);
                case TrueValueNode:
                    return new ExpressionEvaluateResult(typeof(bool), true);
                case FalseValueNode:
                    return new ExpressionEvaluateResult(typeof(bool), false);
                case VarRefNode varRefNode:
                    if (_dialogueState.Variables.TryGetValue(varRefNode.Ref, out var result))
                    {
                        return result;
                    }

                    ArtifactDialoguerDebug.RuntimeLog(
                        $"Evaluate expression {expression} failed. No such variable {varRefNode.Ref} existed.",
                        DebugLogLevel.Fatal);
                    return new ExpressionEvaluateResult(null, null);
                default:
                    ArtifactDialoguerDebug.RuntimeLog($"Evaluate expression {expression} failed.", DebugLogLevel.Fatal);
                    return new ExpressionEvaluateResult(null, null);
            }
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// Export dialogue state of this runner.
        /// </summary>
        /// <param name="blockLevelOnly">If true, this save will restart from the beginning of current block when loaded.</param>
        /// <returns>The dialogue state of this runner.</returns>
        public DialogueState ExportSave(bool blockLevelOnly = false)
        {
            if (_dialogueState == null)
            {
                ArtifactDialoguerDebug.RuntimeLog($"Dialogue state of {gameObject.name} is unexisted.",
                    DebugLogLevel.Error);
                return null;
            }

            if (!_dialogueState.Save(blockLevelOnly))
            {
                ArtifactDialoguerDebug.RuntimeLog($"{gameObject.name} saving failed. Logged above.",
                    DebugLogLevel.Error);
            }

            return _dialogueState;
        }

        /// <summary>
        /// Load dialogue state from saved state instance.
        /// </summary>
        /// <param name="state">The dialogue state to load.</param>
        public void LoadSave(DialogueState state)
        {
            if (state == null)
            {
                ArtifactDialoguerDebug.RuntimeLog($"Given save state for {gameObject.name} is unexisted.",
                    DebugLogLevel.Error);
            }

            if (!_dialogueState.Load(state, dialogueStorageObject))
            {
                ArtifactDialoguerDebug.RuntimeLog(
                    $"{gameObject.name} loading failed. Could not load save. Logged above.", DebugLogLevel.Error);
            }
        }

        #endregion

        #endregion
    }
}