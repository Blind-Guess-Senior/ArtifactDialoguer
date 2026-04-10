using System;
using BlindGuessSenior.ArtifactDialoguer.Frontend;

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
        private const string ArtifactExceptionHead =
            "<color=purple>Artifact Dialoguer</color>: <color=yellow>[Compile Error]</color> ";

        protected CompileException() : base()
        {
        }

        protected CompileException(string message) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>")
        {
        }

        protected CompileException(string message, Exception innerException) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>",
            innerException)
        {
        }
    }

    public sealed class BadTokenException : CompileException
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

    public class ParseException : CompileException
    {
        protected ParseException() : base()
        {
        }

        protected ParseException(string message) : base(message)
        {
        }

        protected ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class MismatchSyntaxException : ParseException
    {
        public MismatchSyntaxException()
        {
        }

        public MismatchSyntaxException(TokenType expectedTokenType, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Expected {expectedTokenType}, but got {token.Type} instead.")
        {
        }

        public MismatchSyntaxException(TokenType expectedTokenType, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Expected {expectedTokenType}, but got {token.Type} instead.",
            innerException)
        {
        }
    }

    public sealed class UnexpectedTokenException : ParseException
    {
        public UnexpectedTokenException()
        {
        }

        public UnexpectedTokenException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected token \"{token.Type}\" \"{token.Literal}\".")
        {
        }

        public UnexpectedTokenException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected token \"{token.Type}\" \"{token.Literal}\".",
            innerException)
        {
        }
    }

    public sealed class UnexpectedAttributeException : ParseException
    {
        public UnexpectedAttributeException()
        {
        }

        public UnexpectedAttributeException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected attribute.")
        {
        }

        public UnexpectedAttributeException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected attribute.", innerException)
        {
        }
    }

    public sealed class UnmatchedAttributeException : ParseException
    {
        public UnmatchedAttributeException()
        {
        }

        public UnmatchedAttributeException(IDialogueAttribute attr, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unmatched attribute {attr} for token {token.Type}.")
        {
        }

        public UnmatchedAttributeException(IDialogueAttribute attr, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unmatched attribute {attr} for token {token.Type}.",
            innerException)
        {
        }
    }

    public sealed class IsolatedAttributeException : ParseException
    {
        public IsolatedAttributeException()
        {
        }

        public IsolatedAttributeException(IDialogueAttribute attr, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Isolated attribute {attr}.")
        {
        }

        public IsolatedAttributeException(IDialogueAttribute attr, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Isolated attribute {attr}.", innerException)
        {
        }
    }

    public sealed class MultipleBlockDefinitionException : ParseException
    {
        public MultipleBlockDefinitionException()
        {
        }

        public MultipleBlockDefinitionException(string @namespace, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Multiple definition for block {token.Literal} in {@namespace}.")
        {
        }

        public MultipleBlockDefinitionException(string @namespace, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Multiple definition for block {token.Literal} in {@namespace}.",
            innerException)
        {
        }
    }

    public sealed class NoExpressionException : ParseException
    {
        public NoExpressionException()
        {
        }

        public NoExpressionException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: 'If' attribute needs an expression.")
        {
        }

        public NoExpressionException(IDialogueAttribute attr, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: 'If' attribute needs an expression.",
            innerException)
        {
        }
    }

    public sealed class InvalidOperatorException : ParseException
    {
        public InvalidOperatorException()
        {
        }

        public InvalidOperatorException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Invalid operator {token.Type}.")
        {
        }

        public InvalidOperatorException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Invalid operator {token.Type}.", innerException)
        {
        }
    }

    public class UndefinedReferenceException : ParseException
    {
        protected UndefinedReferenceException()
        {
        }

        protected UndefinedReferenceException(Token token, string message) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Undefined reference to {message}.")
        {
        }

        protected UndefinedReferenceException(Token token, string message, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Undefined reference to {message}.",
            innerException)
        {
        }
    }

    public sealed class UndefinedBlockReferenceException : UndefinedReferenceException
    {
        public UndefinedBlockReferenceException()
        {
        }

        public UndefinedBlockReferenceException(Tuple<string, Token> info) : base(info.Item2,
            $"block '{info.Item2.Literal}' in namespace '{info.Item1}'")
        {
        }

        public UndefinedBlockReferenceException(Tuple<string, Token> info, Exception innerException) : base(info.Item2,
            $"block '{info.Item2.Literal}' in namespace '{info.Item1}'", innerException)
        {
        }
    }

    public sealed class UndefinedVariableReferenceException : UndefinedReferenceException
    {
        public UndefinedVariableReferenceException()
        {
        }

        public UndefinedVariableReferenceException(Token token, string var) : base(token, $"variable '{var}'.")
        {
        }

        public UndefinedVariableReferenceException(Token token, string var, Exception innerException) : base(token,
            $"'{var}'.", innerException)
        {
        }
    }

    public sealed class WrongOptionBodyException : ParseException
    {
        public WrongOptionBodyException()
        {
        }

        public WrongOptionBodyException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Option body must be a command, but got {token.Type} instead.")
        {
        }

        public WrongOptionBodyException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Option body must be a command, but got {token.Type} instead.",
            innerException)
        {
        }
    }
}