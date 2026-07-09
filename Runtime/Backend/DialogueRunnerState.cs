using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BlindGuessSenior.ArtifactDialoguer.Frontend;
using BlindGuessSenior.ArtifactDialoguer.Utilities.DebugUtils;
using Newtonsoft.Json;
using Unity.VisualScripting.YamlDotNet.Serialization;

namespace BlindGuessSenior.ArtifactDialoguer.Backend
{
    using ExpressionEvaluateResult = Tuple<Type, object>;
    using CallStackEntry = Tuple<BlockNode, int>;

    [Serializable]
    public class DialogueRunnerState
    {
        #region Definitions

        /// <summary>
        /// Serializable entry of call stack.
        /// </summary>
        [Serializable]
        public struct SerializableCallStackEntry
        {
            public string BlockName;
            public int StatementIndex;
        }

        public enum DialogueVarType
        {
            Int,
            Float,
            Bool
        }

        /// <summary>
        /// Serializable entry of variables.
        /// </summary>
        [Serializable]
        public struct SerializableVariableEntry
        {
            public string VarName;
            public DialogueVarType ValueType;
            public string Value;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Current block node. Runtime field.
        /// </summary>
        [NonSerialized] [JsonIgnore] [XmlIgnore] [YamlIgnore]
        public BlockNode CurrentBlock;

        /// <summary>
        /// Current statement's index in current block.
        /// <br/>
        /// The statement this index point to is the statement to process when next one Next() func called.
        /// </summary>
        public int CurrentBlockStatementIndex;

        /// <summary>
        /// Current content index in current cross text.
        /// </summary>
        public int CurrentCrossTextIndex;

        /// <summary>
        /// Statement index of current cross text statement. Used to avoid wrong statement advance.
        /// </summary>
        public int CurrentCrossTextStatementIndex;

        /// <summary>
        /// Whether there is in cross text's context.
        /// </summary>
        public bool IsCrossTextContext;

        /// <summary>
        /// Whether there is in condition of waiting for option choose.
        /// </summary>
        public bool IsOptionContext;

        /// <summary>
        /// Saved statements that has been 'ponce', which is once in every runner.
        /// <br/>
        /// Distinguish by node's id.
        /// </summary>
        [NonSerialized] [JsonIgnore] [XmlIgnore] [YamlIgnore]
        public HashSet<int> POnceStatement;

        /// <summary>
        /// Call stack to support goto and ret.
        /// </summary>
        [NonSerialized] [JsonIgnore] [XmlIgnore] [YamlIgnore]
        public Stack<CallStackEntry> CallStack;

        #endregion

        #region Call Stack Methods

        /// <summary>
        /// Push current block into call stack.
        /// </summary>
        public void CallStackPush()
            => CallStack.Push(new CallStackEntry(CurrentBlock, CurrentBlockStatementIndex));

        /// <summary>
        /// Pop call stack entry, which is last block that calling, and return it.
        /// </summary>
        /// <returns>The last call stack entry that calling.</returns>
        public CallStackEntry CallStackPop()
            => CallStack.Pop();

        #endregion

        #region Save Fields

        /// <summary>
        /// Serialize version for <see cref="CurrentBlock"/>.
        /// </summary>
        public string CurrentBlockName;

        /// <summary>
        /// Serialize version for <see cref="POnceStatement"/>.
        /// </summary>
        public List<int> SerializedOnceStatements = new();

        /// <summary>
        /// Serialize version for <see cref="Variables"/>.
        /// </summary>
        public List<SerializableVariableEntry> SerializedVariables = new();

        /// <summary>
        /// Serialize version for <see cref="CallStack"/>.
        /// </summary>
        public List<SerializableCallStackEntry> SerializedCallStack = new();

        /// <summary>
        /// Flag whether to restart the current block on load.
        /// </summary>
        public bool SavedAtBlockLevel;

        #endregion

        #region Save Methods

        /// <summary>
        /// Try save current state into serializable fields. Then can return this state for saving.
        /// </summary>
        /// <param name="blockLevelOnly">True to save exactly at the beginning of the block, otherwise false.</param>
        /// <returns>True if save successfully; otherwise, false.</returns>
        [Obsolete("Current Save Implementation is Bad. Do not Use It.")]
        public bool Save(bool blockLevelOnly = false)
        {
            SavedAtBlockLevel = blockLevelOnly;

            // Save block
            if (CurrentBlock == null)
            {
                ArtifactDialoguerDebug.RuntimeLog("Save failed. Current block unexisted.");
                return false;
            }

            CurrentBlockName = CurrentBlock.BlockName;

            // Save once statements
            SerializedOnceStatements.Clear();
            foreach (var statement in POnceStatement)
            {
                SerializedOnceStatements.Add(statement);
            }

            // Save variables
            // SerializedVariables.Clear();
            // foreach (var variable in Variables)
            // {
            //     DialogueVarType saveType = DialogueVarType.Int;
            //     if (variable.Value.Item1 == typeof(float)) saveType = DialogueVarType.Float;
            //     else if (variable.Value.Item1 == typeof(bool)) saveType = DialogueVarType.Bool;
            //
            //     SerializedVariables.Add(new SerializableVariableEntry
            //     {
            //         VarName = variable.Key,
            //         ValueType = saveType,
            //         Value = variable.Value.Item2.ToString()
            //     });
            // }

            // Save call stack
            SerializedCallStack.Clear();
            var quickCopy = new Stack<CallStackEntry>(CallStack); // quickCopy is reversed.
            while (quickCopy.Count > 0)
            {
                var entry = quickCopy.Pop();

                SerializableCallStackEntry serializedEntry;
                serializedEntry.BlockName = entry.Item1.BlockName;
                serializedEntry.StatementIndex = entry.Item2;

                SerializedCallStack.Add(serializedEntry);
            }

            return true;
        }

        /// <summary>
        /// Try load state from given saved state.
        /// </summary>
        /// <returns>True if load successfully; otherwise, false.</returns>
        [Obsolete("Current Save Implementation is Bad. Do not Use It.")]
        public bool Load(DialogueRunnerState runnerState, DialogueStorageObject dialogueStorageObject)
        {
            // Load serializable
            if (runnerState.SavedAtBlockLevel)
            {
                CurrentBlockStatementIndex = 0;
                CurrentCrossTextIndex = 0;
                CurrentCrossTextStatementIndex = 0;
                IsCrossTextContext = false;
                IsOptionContext = false;
            }
            else
            {
                CurrentBlockStatementIndex = runnerState.CurrentBlockStatementIndex;
                CurrentCrossTextIndex = runnerState.CurrentCrossTextIndex;
                CurrentCrossTextStatementIndex = runnerState.CurrentCrossTextStatementIndex;
                IsCrossTextContext = runnerState.IsCrossTextContext;
                IsOptionContext = runnerState.IsOptionContext;
            }

            // Load block
            if (string.IsNullOrEmpty(runnerState.CurrentBlockName))
            {
                ArtifactDialoguerDebug.RuntimeLog("Load failed. Current block unexisted.");
                return false;
            }

            CurrentBlock = dialogueStorageObject.Blocks.Find(b => b.BlockName == runnerState.CurrentBlockName);
            if (CurrentBlock == null)
            {
                ArtifactDialoguerDebug.RuntimeLog("Load failed. Saved block not found.");
                return false;
            }

            // Load once statements
            POnceStatement.Clear();
            foreach (var statement in runnerState.SerializedOnceStatements)
            {
                POnceStatement.Add(statement);
            }

            // Load variables
            // Variables.Clear();
            // foreach (var variable in runnerState.SerializedVariables)
            // {
            //     Type runtimeType = typeof(int);
            //     object runtimeValue = 0;
            //
            //     switch (variable.ValueType)
            //     {
            //         case DialogueVarType.Int:
            //             runtimeType = typeof(int);
            //             runtimeValue = int.Parse(variable.Value);
            //             break;
            //         case DialogueVarType.Float:
            //             runtimeType = typeof(float);
            //             runtimeValue = float.Parse(variable.Value);
            //             break;
            //         case DialogueVarType.Bool:
            //             runtimeType = typeof(bool);
            //             runtimeValue = bool.Parse(variable.Value);
            //             break;
            //     }
            //
            //     Variables.Add(variable.VarName, new ExpressionEvaluateResult(runtimeType, runtimeValue));
            // }

            // Load call stack
            CallStack.Clear();
            foreach (var serializedEntry in runnerState.SerializedCallStack)
            {
                var block = dialogueStorageObject.Blocks.Find(b => b.BlockName == serializedEntry.BlockName);
                if (block == null)
                {
                    ArtifactDialoguerDebug.RuntimeLog("Load failed. Saved block in call stack not found.");
                    return false;
                }

                var entry = new CallStackEntry(block, serializedEntry.StatementIndex);

                CallStack.Push(entry);
            }

            return true;
        }

        #endregion
    }

    public static class DialogueState
    {
        #region Definitions

        public class PerNamespaceState
        {
            #region Fields

            /// <summary>
            /// Saved statements that has been 'once'. Distinguish by node's id.
            /// </summary>
            [NonSerialized] [JsonIgnore] [XmlIgnore] [YamlIgnore]
            private readonly HashSet<int> OnceStatement = new();

            /// <summary>
            /// Saved variables.
            /// </summary>
            [NonSerialized] [JsonIgnore] [XmlIgnore] [YamlIgnore]
            private readonly Dictionary<string, ExpressionEvaluateResult> Variables = new();

            #endregion

            #region OnceStatement Methods

            public bool HadOnced(int nodeId)
                => OnceStatement.Contains(nodeId);

            #endregion

            #region Variables Methods

            public bool TryGetVariable(string variable, out ExpressionEvaluateResult result)
                => Variables.TryGetValue(variable, out result);

            public void SetVariable(string variable, ExpressionEvaluateResult result)
                => Variables[variable] = result;

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Stores states that should be shared through runners in same namespace.
        /// <br/>
        /// Distinguish by namespace.
        /// </summary>
        public static Dictionary<string, PerNamespaceState> PerNamespaceStates { get; } = new();

        #endregion

        #region Methods

        #region Save Methods

        public static void Save()
        {
            throw new NotImplementedException();
        }

        public static void Load()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}