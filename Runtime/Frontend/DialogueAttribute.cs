using System.Collections.Generic;
using BlindGuessSenior.ArtifactDialoguer.Utilities.Exceptions;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
    public interface IDialogueAttribute
    {
        public void IsAllowed(Token token);
    }

    public sealed class OnceAttribute : IDialogueAttribute
    {
        private readonly List<TokenType> _allowedTokens = new()
        {
            TokenType.BlockDecl,
            TokenType.Text,
            TokenType.CrossText,
            TokenType.At,
            TokenType.Question,

            TokenType.Goto,
            TokenType.Return,
            TokenType.Null,
            TokenType.Set,
        };

        public void IsAllowed(Token token)
        {
            if (!_allowedTokens.Contains(token.Type))
            {
                throw new UnmatchedAttributeException(this, token);
            }
        }
    }

    public sealed class CycleAttribute : IDialogueAttribute
    {
        private readonly List<TokenType> _allowedTokens = new()
        {
            TokenType.BlockDecl,
        };

        public void IsAllowed(Token token)
        {
            if (!_allowedTokens.Contains(token.Type))
            {
                throw new UnmatchedAttributeException(this, token);
            }
        }
    }

    public sealed class UnreachAttribute : IDialogueAttribute
    {
        private readonly List<TokenType> _allowedTokens = new()
        {
            TokenType.BlockDecl,
        };

        public void IsAllowed(Token token)
        {
            if (!_allowedTokens.Contains(token.Type))
            {
                throw new UnmatchedAttributeException(this, token);
            }
        }
    }

    public sealed class IfAttribute : IDialogueAttribute
    {
        private readonly List<TokenType> _allowedTokens = new()
        {
            TokenType.BlockDecl,
            TokenType.Text,
            TokenType.CrossText,
            TokenType.At,
            TokenType.Question,

            TokenType.Goto,
            TokenType.Return,
            TokenType.Null,
            TokenType.Set,
        };

        public void IsAllowed(Token token)
        {
            if (!_allowedTokens.Contains(token.Type))
            {
                throw new UnmatchedAttributeException(this, token);
            }
        }

        public ExpressionNode LHS;
        public TokenType Operator;
        public ExpressionNode RHS;

        public IfAttribute(ExpressionNode lhs, TokenType @operator, ExpressionNode rhs)
        {
            LHS = lhs;
            Operator = @operator;
            RHS = rhs;
        }
    }
}