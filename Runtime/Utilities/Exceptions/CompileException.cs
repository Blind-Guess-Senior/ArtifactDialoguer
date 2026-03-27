using System;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Utilities.Exceptions
{
    /// <summary>
    /// Abstract exception for artifact compiler frontend.
    /// </summary>
    public abstract class CompileException : Exception
    {
        /// <summary>
        /// String header to indicate that log is come from Artifact Dialoguer.
        /// </summary>
        protected const string ArtifactExceptionHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Compile Error]</color> ";

        public CompileException() : base()
        {
        }

        public CompileException(string message) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>")
        {
        }

        public CompileException(string message, Exception innerException) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>",
            innerException)
        {
        }
    }

    public class BadTokenException : CompileException
    {
        public BadTokenException() : base()
        {
        }

        public BadTokenException(string message) : base(message)
        {
        }

        public BadTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}