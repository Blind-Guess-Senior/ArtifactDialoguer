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

        private CompileException() : base()
        {
        }

        protected CompileException(string message) : base($"{ArtifactExceptionHead}<color=#FF4500>{message}</color>")
        {
        }

        protected CompileException(string message, Exception innerException) : base(
            $"{ArtifactExceptionHead}<color=#FF4500>{message}</color>", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that there is a problem occur when lexing.
    /// </summary>
    public class LexException : CompileException
    {
        protected LexException(string message) : base(message)
        {
        }

        protected LexException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that there is an unexpected token appear.
    /// </summary>
    public sealed class BadTokenException : LexException
    {
        public BadTokenException(string message) : base(message)
        {
        }

        public BadTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that there is a problem occur when parsing.
    /// </summary>
    public class ParseException : CompileException
    {
        protected ParseException(string message) : base(message)
        {
        }

        protected ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that missing a token which is explicit expected.
    /// </summary>
    public sealed class MismatchSyntaxException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that an unexpected token appeared.
    /// </summary>
    public sealed class UnexpectedTokenException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that there should not be an attribute token, but it appears.
    /// </summary>
    public sealed class UnexpectedAttributeException : ParseException
    {
        public UnexpectedAttributeException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected attribute.")
        {
        }

        public UnexpectedAttributeException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Unexpected attribute.", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that an attribute appeared with a token that cannot have this attribute.
    /// </summary>
    public sealed class UnmatchedAttributeException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that an isolated attribute is appeared without attached token.
    /// </summary>
    public sealed class IsolatedAttributeException : ParseException
    {
        public IsolatedAttributeException(IDialogueAttribute attr, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Isolated attribute {attr}.")
        {
        }

        public IsolatedAttributeException(IDialogueAttribute attr, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Isolated attribute {attr}.", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that some conflict attribute attached a same token.
    /// </summary>
    public sealed class ConflictAttributeException : ParseException
    {
        public ConflictAttributeException(IDialogueAttribute[] attr, Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Conflict attributes '{attr}'.")
        {
        }

        public ConflictAttributeException(IDialogueAttribute[] attr, Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Conflict attributes '{attr}'.", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that a block name is appeared twice.
    /// </summary>
    public sealed class MultipleBlockDefinitionException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that no expression found in if attribute's body.
    /// </summary>
    public sealed class NoExpressionException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that an unsupported operator appeared.
    /// </summary>
    public sealed class InvalidOperatorException : ParseException
    {
        public InvalidOperatorException(Token token) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Invalid operator {token.Type}.")
        {
        }

        public InvalidOperatorException(Token token, Exception innerException) : base(
            $"Syntax Error at Line {token.Line}, Column {token.Column}: Invalid operator {token.Type}.", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that the object given reference refers to is undefined.
    /// </summary>
    public class UndefinedReferenceException : ParseException
    {
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

    /// <summary>
    /// Exception which represent that the block given reference refers to is undefined.
    /// </summary>
    public sealed class UndefinedBlockReferenceException : UndefinedReferenceException
    {
        public UndefinedBlockReferenceException(Tuple<string, Token> info) : base(info.Item2,
            $"block '{info.Item2.Literal}' in namespace '{info.Item1}'")
        {
        }

        public UndefinedBlockReferenceException(Tuple<string, Token> info, Exception innerException) : base(info.Item2,
            $"block '{info.Item2.Literal}' in namespace '{info.Item1}'", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that the variable given reference refers to is undefined.
    /// </summary>
    public sealed class UndefinedVariableReferenceException : UndefinedReferenceException
    {
        public UndefinedVariableReferenceException(Token token, string var) : base(token, $"variable '{var}'.")
        {
        }

        public UndefinedVariableReferenceException(Token token, string var, Exception innerException) : base(token,
            $"'{var}'.", innerException)
        {
        }
    }

    /// <summary>
    /// Exception which represent that the body of option statement is not a command, which is unallowed.
    /// </summary>
    public sealed class WrongOptionBodyException : ParseException
    {
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