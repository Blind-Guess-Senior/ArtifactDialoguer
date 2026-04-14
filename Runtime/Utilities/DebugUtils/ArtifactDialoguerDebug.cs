using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Utilities.DebugUtils
{
    /// <summary>
    /// Enum for switching debug level to set color and log level.
    /// </summary>
    public enum DebugLogLevel
    {
        Fatal,
        Error,
        Warning,
        Info,
        Hint,
        WorksWell,
    }

    /// <summary>
    /// Debug wrapper of Unity Debug. With package header and debug-level-based color.
    /// <br/>
    /// It will be automatically disabled in the built release version.
    /// </summary>
    public static class ArtifactDialoguerDebug
    {
        #region Fields

        /// <summary>
        /// String header to indicate that log is come from Artifact Dialoguer.
        /// </summary>
        private const string ArtifactHead = "<color=purple>Artifact Dialoguer</color>: ";

        /// <summary>
        /// String header to indicate that log is come from Artifact Dialoguer compile time.
        /// </summary>
        private const string ArtifactCompileHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Compile]</color> ";

        private const string ArtifactCompileErrorHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Compile Error]</color> ";

        private const string ArtifactCompileWarningHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Compile Warning]</color> ";

        /// <summary>
        /// String header to indicate that log is come from Artifact Dialoguer runtime.
        /// </summary>
        private const string ArtifactRuntimeHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Runtime]</color> ";

        private const string ArtifactRuntimeErrorHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Runtime Error]</color> ";

        private const string ArtifactRuntimeWarningHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Runtime Warning]</color> ";

        #endregion

        #region Static Methods

        /// <summary>
        /// Wrapped debug log function for package internal usage.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void PackageLog(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
        {
            switch (logLevel)
            {
                case DebugLogLevel.Fatal:
                    Debug.LogError($"{ArtifactHead}<color=#DC143C>{message}</color>");
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError($"{ArtifactHead}<color=#FF4500>{message}</color>");
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning($"{ArtifactHead}<color=#FFA500>{message}</color>");
                    break;
                case DebugLogLevel.Info:
                    Debug.Log($"{ArtifactHead}<color=#6495ED>{message}</color>");
                    break;
                case DebugLogLevel.Hint:
                    Debug.Log($"{ArtifactHead}<color=#48D1CC>{message}</color>");
                    break;
                case DebugLogLevel.WorksWell:
                    Debug.Log($"{ArtifactHead}<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }

        /// <summary>
        /// Wrapped debug log function for package internal usage.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void CompileLog(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
        {
            switch (logLevel)
            {
                case DebugLogLevel.Fatal:
                    Debug.LogError($"{ArtifactCompileErrorHead}<color=#DC143C>{message}</color>");
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError($"{ArtifactCompileErrorHead}<color=#FF4500>{message}</color>");
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning($"{ArtifactCompileWarningHead}<color=#FFA500>{message}</color>");
                    break;
                case DebugLogLevel.Info:
                    Debug.Log($"{ArtifactCompileHead}<color=#6495ED>{message}</color>");
                    break;
                case DebugLogLevel.Hint:
                    Debug.Log($"{ArtifactCompileHead}<color=#48D1CC>{message}</color>");
                    break;
                case DebugLogLevel.WorksWell:
                    Debug.Log($"{ArtifactCompileHead}<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }


        /// <summary>
        /// Wrapped debug log function for package internal usage.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void RuntimeLog(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
        {
            switch (logLevel)
            {
                case DebugLogLevel.Fatal:
                    Debug.LogError($"{ArtifactRuntimeErrorHead}<color=#DC143C>{message}</color>");
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError($"{ArtifactRuntimeErrorHead}<color=#FF4500>{message}</color>");
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning($"{ArtifactRuntimeWarningHead}<color=#FFA500>{message}</color>");
                    break;
                case DebugLogLevel.Info:
                    Debug.Log($"{ArtifactRuntimeHead}<color=#6495ED>{message}</color>");
                    break;
                case DebugLogLevel.Hint:
                    Debug.Log($"{ArtifactRuntimeHead}<color=#48D1CC>{message}</color>");
                    break;
                case DebugLogLevel.WorksWell:
                    Debug.Log($"{ArtifactRuntimeHead}<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }

        #endregion
    }
}