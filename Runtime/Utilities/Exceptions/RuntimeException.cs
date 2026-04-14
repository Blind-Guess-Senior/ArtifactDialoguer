using System;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Utilities.Exceptions
{
    /// <summary>
    /// Abstract exception for artifact compiler frontend.
    /// </summary>
    public abstract class RuntimeException : Exception
    {
        /// <summary>
        /// String header to indicate that log is come from Artifact Dialoguer.
        /// </summary>
        private const string ArtifactExceptionHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Runtime Error]</color> ";

        private RuntimeException() : base()
        {
        }

        protected RuntimeException(string message) : base($"{ArtifactExceptionHead}<color=#FF4500>{message}</color>")
        {
        }

        protected RuntimeException(string message, Exception innerException) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that the option user send back is not valid because there is no such index.
    /// </summary>
    public sealed class OptionOutOfRangeException : RuntimeException
    {
        public OptionOutOfRangeException(int index, int valid) : base(
            $"Option chosen index out of range. Got {index} but there is only {valid} available.")
        {
        }

        public OptionOutOfRangeException(int index, int valid, Exception innerException) : base(
            $"Option chosen index out of range. Got {index} but there is only {valid} available.", innerException)
        {
        }
    }
}