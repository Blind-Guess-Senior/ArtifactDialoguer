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

    /// <summary>
    /// Interface for dialogue runner.
    /// </summary>
    public interface IDialogueRunner
    {
        public void Init();
        public IDialogueRuntimeResult Next();
        public IDialogueRuntimeResult OptionChosen(int index);
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
            OnceStatement = new Dictionary<Node, bool>(),
            Variables = new Dictionary<string, ExpressionEvaluateResult>(),
            CallStack = new Stack<BlockNode>(),
        };

        /// <summary>
        /// Whether there is in cross text's context.
        /// </summary>
        private bool _isCrossTextContext;

        /// <summary>
        /// Whether there is in condition of waiting for option choose.
        /// </summary>
        private bool _isOptionContext;

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

        /// <summary>
        /// Init dialogue runner.
        /// </summary>
        public void Init()
        {
            if (!dialogueStorageObject)
            {
                ArtifactDialoguerDebug.PackageLog("Dialogue file not found!", DebugLogLevel.Fatal);
            }

            _isCrossTextContext = false;
            _isOptionContext = false;

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
                if (_isOptionContext)
                {
                    return new DialogueRuntimeResultNextDenied();
                }

                if (_dialogueState.CurrentBlock == null)
                {
                    ArtifactDialoguerDebug.RuntimeLog("No current dialogue block found.", DebugLogLevel.Warning);
                    return new DialogueRuntimeResultError();
                }

                if (_dialogueState.CurrentBlockStatementIndex >= _dialogueState.CurrentBlock.Statements.Count)
                {
                    _isOptionContext = false;
                    _isCrossTextContext = false;
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
                        if (!_dialogueState.OnceStatement.TryAdd(_dialogueState.CurrentBlock, true))
                        {
                            _dialogueState.CurrentBlock = _dialogueState.CurrentBlock.NaturalNext;
                            _dialogueState.CurrentBlockStatementIndex = 0;
                            goto Continue;
                        }
                    }
                }

                var statement = _dialogueState.CurrentBlock.Statements[_dialogueState.CurrentBlockStatementIndex];
                if (_isCrossTextContext)
                {
                    statement = _dialogueState.CurrentBlock.Statements[_dialogueState.CurrentCrossTextStatementIndex];
                    goto StatementActions;
                }

                _dialogueState.CurrentBlockStatementIndex++;

                // Check statement's attribute
                conditions = statement.Attributes.OfType<IfAttribute>().ToList();
                if (!CheckConditions(conditions))
                {
                    goto Continue;
                }

                if (statement.Attributes?.Any(attr => attr is OnceAttribute) ?? false)
                {
                    if (!_dialogueState.OnceStatement.TryAdd(statement, true)) // Found got once
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
                        if (!_isCrossTextContext)
                        {
                            _isCrossTextContext = true;
                            _dialogueState.CurrentCrossTextIndex = 0;
                            _dialogueState.CurrentCrossTextStatementIndex =
                                _dialogueState.CurrentBlockStatementIndex - 1;
                        }

                        if (_dialogueState.CurrentCrossTextIndex >= crossTextNode.Contents.Count)
                        {
                            _isCrossTextContext = false;
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
                        _dialogueState.CurrentBlock = _dialogueState.CallStackPop();
                        _dialogueState.CurrentBlockStatementIndex = 0;

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
                                if (_dialogueState.OnceStatement.ContainsKey(option))
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

                        _isOptionContext = true;
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

            _isOptionContext = false;
            var chosenOption = _currentOptionList[index];

            if (chosenOption.Once)
            {
                _dialogueState.OnceStatement.Add(chosenOption.Node, true);
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

                    _dialogueState.CurrentBlock = _dialogueState.CallStackPop();
                    _dialogueState.CurrentBlockStatementIndex = 0;
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
            // TODO: check if conditions
            return true;
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
                default:
                    ArtifactDialoguerDebug.RuntimeLog($"Evaluate expression {expression} failed.", DebugLogLevel.Fatal);
                    return new ExpressionEvaluateResult(null, null);
            }
        }

        #endregion
    }
}