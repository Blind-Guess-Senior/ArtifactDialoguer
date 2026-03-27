// ReSharper disable CheckNamespace

using System.Collections.Generic;

namespace BlindGuessSenior.ArtifactDialoguer.Frontend
{
    public enum TokenType
    {
        // Command Keywords
        Goto,
        Return,
        Extcall,

        // Attribute Keywords
        Once,
        Unreach,
        Cycle,

        // Expr Keywords
        Null,
        Set,
        If,


        // Symbols
        // Pound, // #
        // LBrace, // {
        // RBrace, // }
        // LParen, // (
        // RParen, // )
        // Slash, // /
        At, // @
        LBracket, // [
        RBracket, // ]
        Colon, // :
        DoubleColon, // ::
        Question, // ?
        Arrow, // ->


        // Other
        EOF,
        Illegal,
        Text,
        CrossText,
        CommandText,
    }

    public struct Token
    {
        public readonly TokenType Type;
        public string Literal;
        public int Line;
        public int Column;

        public Token(TokenType type, string literal, int line, int column)
        {
            Type = type;
            Literal = literal;
            Line = line;
            Column = column;
        }
    }

    /// <summary>
    /// Class for lex input.
    /// </summary>
    public class LexerInstance
    {
        #region Fields

        /// <summary>
        /// String source that will be lex.
        /// </summary>
        private readonly string _source;

        /// <summary>
        /// Current character's position in source.
        /// </summary>
        private int _position;

        /// <summary>
        /// Next character's position in source.
        /// </summary>
        private int _readPosition;

        /// <summary>
        /// Current character.
        /// </summary>
        private char _ch;

        /// <summary>
        /// Current character's corresponding line number in file.
        /// </summary>
        private int _line = 1;

        /// <summary>
        /// Current character's corresponding column number in file.
        /// </summary>
        private int _column = 0;

        /// <summary>
        /// Whether it is in command context.
        /// </summary>
        private bool _isCommandContext;

        /// <summary>
        /// Whether current context is start in very head of a line.
        /// </summary>
        private bool _isLineStart = true;

        /// <summary>
        /// Keywords map.
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "goto", TokenType.Goto },
            { "return", TokenType.Return },
            { "extcall", TokenType.Extcall },

            { "once", TokenType.Once },
            { "unreach", TokenType.Unreach },
            { "cycle", TokenType.Cycle },

            { "null", TokenType.Null },
            { "set", TokenType.Set },
            { "if", TokenType.If },
        };

        #endregion

        #region Constructors

        public LexerInstance(string source)
        {
            _source = source;
            ReadChar();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Move current position to next char and update current char.
        /// </summary>
        private void ReadChar()
        {
            if (_readPosition >= _source.Length)
            {
                _ch = '\0';
            }
            else
            {
                _ch = _source[_readPosition];
            }

            _position = _readPosition;
            _readPosition++;
            _column++;
        }

        /// <summary>
        /// Peek next char without change current position.
        /// </summary>
        /// <returns>Next char in source.</returns>
        private char PeekChar()
        {
            if (_readPosition >= _source.Length)
                return '\0';
            return _source[_readPosition];
        }

        /// <summary>
        /// Generate next token.
        /// </summary>
        /// <returns>Next token in source.</returns>
        public Token NextToken()
        {
            SkipWhitespace();

            Token tok;
            int startColumn = _column;
            bool wasLineStart = _isLineStart;

            switch (_ch)
            {
                // Unused
                // case '(':
                //     tok = new Token(TokenType.LParen, _ch.ToString(), _line, startColumn);
                //     break;
                // case ')':
                //     tok = new Token(TokenType.RParen, _ch.ToString(), _line, startColumn);
                //     break;
                // case '/':
                //     tok = new Token(TokenType.Slash, _ch.ToString(), _line, startColumn);
                //     break;
                // case '#':
                //     tok = new Token(TokenType.Pound, _ch.ToString(), _line, startColumn);
                //     break;
                // Unused


                // Check key symbols. Only happen when the first character is not text.
                case '@':
                    // SPECIAL: @ will maintain the property of treating the current character as the start of a line.
                    // so that a speaker can have space.
                    ReadChar();
                    return new Token(TokenType.At, "@", _line, startColumn);
                case ':':
                    if (PeekChar() == ':')
                    {
                        ReadChar(); // Consume first ':'
                        tok = new Token(TokenType.DoubleColon, "::", _line, startColumn);
                    }
                    else
                    {
                        tok = new Token(TokenType.Colon, ":", _line, startColumn);
                    }

                    break;
                case '?':
                    tok = new Token(TokenType.Question, "?", _line, startColumn);
                    break;
                case '-':
                    if (PeekChar() == '>')
                    {
                        ReadChar(); // consume '-'
                        tok = new Token(TokenType.Arrow, "->", _line, startColumn);
                    }
                    else
                    {
                        tok = new Token(TokenType.Illegal, "Unexpected hyphen '-'. Do you missing '>'?", _line,
                            startColumn);
                    }

                    break;

                // Check if there is a cross-line text
                case '{':
                    ReadChar(); // consume '{'
                    var textPos = _position;
                    var textLine = _line;
                    // Read until file end or cross-line text end
                    while (_ch != '}' && _ch != '\0')
                    {
                        if (_ch == '\n')
                        {
                            _line++;
                            _column = 0;
                        }

                        ReadChar();
                    }

                    string textContent = _source.Substring(textPos, _position - textPos);

                    _isLineStart = false;

                    if (_ch == '}')
                    {
                        ReadChar(); // consume '}'
                        return new Token(TokenType.CrossText, textContent, textLine, startColumn);
                    }
                    else
                    {
                        return new Token(TokenType.Illegal, "Unterminated brace '{'.", textLine, startColumn);
                    }
                case '}':
                    tok = new Token(TokenType.Illegal, "Unexpected right brace '}'.", _line, startColumn);
                    _isLineStart = false;
                    break;

                // Check if there is a command context
                case '[':
                    _isLineStart = false;
                    if (_isCommandContext)
                    {
                        tok = new Token(TokenType.Illegal, "Invalid nested bracket '['.", _line, startColumn);
                        break;
                    }

                    _isCommandContext = true;
                    tok = new Token(TokenType.LBracket, "[", _line, startColumn);
                    break;
                case ']':
                    _isLineStart = false;
                    if (!_isCommandContext)
                    {
                        tok = new Token(TokenType.Illegal, "Unexpected right bracket ']'.", _line, startColumn);
                        break;
                    }

                    _isCommandContext = false;
                    tok = new Token(TokenType.RBracket, "]", _line, startColumn);
                    break;

                // Check if file ends.
                case '\0':
                    tok = new Token(TokenType.EOF, "", _line, startColumn);
                    break;

                // Evaluate normal text or command context.
                default:
                    // If we are at the start of a line and NOT in a command context, read the whole line as text.
                    // Full line text will include whitespace and some key symbols.
                    if (wasLineStart && !_isCommandContext && _ch != '\0' && _ch != '\n' && _ch != '\r')
                    {
                        var text = ReadText(true);
                        _isLineStart = false;
                        // Avoid calling ReadChar at the end, as ReadText stops exactly at the char that broke the loop
                        return new Token(TokenType.Text, text, _line, startColumn);
                    }

                    // Common text. Split by whitespace.
                    // Common text will include some key symbols that do not appear in the first char.
                    if (IsTextFirstChar(_ch))
                    {
                        var text = ReadText(false);
                        var type = LookupKeywordOrText(text);
                        _isLineStart = false;
                        // Avoid calling ReadChar at the end, as ReadText stops exactly at the char that broke the loop
                        return new Token(type, text, _line, startColumn);
                    }

                    tok = new Token(TokenType.Illegal, _ch.ToString(), _line, startColumn);
                    break;
            }

            _isLineStart = false;
            ReadChar();
            return tok;
        }

        /// <summary>
        /// Read text with two mode:
        /// <br/>
        /// 1. Read a text until whitespace.
        /// <br/>
        /// 2. Read a full-line text.
        /// </summary>
        /// <param name="isLineStartText">If it is a full-line text.</param>
        /// <returns>Content that be read.</returns>
        private string ReadText(bool isLineStartText)
        {
            int position = _position;

            // Adjust to the right offset depending on how the loop starts
            if (isLineStartText)
            {
                // We do NOT consume the first char beforehand, so we just read
                while (IsFullLineTextChar(_ch))
                {
                    ReadChar();
                }
            }
            else
            {
                // We consume the first char immediately, and we also consider the first char part of the text.
                // It was already validated by IsTextFirstChar.
                ReadChar();
                while (IsTextSubsequentChar(_ch))
                {
                    ReadChar();
                }
            }

            return _source.Substring(position, _position - position);
        }

        private string ReadNumber()
        {
            int position = _position;
            while (IsDigit(_ch))
            {
                ReadChar();
            }

            return _source.Substring(position, _position - position);
        }

        private void SkipWhitespace()
        {
            while (true)
            {
                if (_ch == ' ' || _ch == '\t' || _ch == '\n' || _ch == '\r')
                {
                    if (_ch == '\n')
                    {
                        _line++;
                        _column = 0;
                        _isLineStart = true;
                    }

                    ReadChar();
                }
                else if (_ch == '/' && PeekChar() == '/')
                {
                    // Skip comment block till end of line
                    while (_ch != '\n' && _ch != '\0')
                    {
                        ReadChar();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        #region Helper Methods

        /// <summary>
        /// Check if current char is text. Special check condition for very first char in a text.
        /// </summary>
        /// <param name="ch">The char to check.</param>
        /// <returns>True if given char is text; otherwise, false.</returns>
        private static bool IsTextFirstChar(char ch)
        {
            if (ch == '\0' || char.IsWhiteSpace(ch))
                return false;

            switch (ch)
            {
                case '#':
                case '@':
                case '{':
                case '}':
                case '[':
                case ']':
                case ':':
                case '?':
                case '/':
                case '-':
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Check if current char is text. Check condition for non-first char in a text.
        /// </summary>
        /// <param name="ch">The char to check.</param>
        /// <returns>True if given char is text; otherwise, false.</returns>
        private static bool IsTextSubsequentChar(char ch)
        {
            if (ch == '\0' || char.IsWhiteSpace(ch))
                return false;

            switch (ch)
            {
                case '[': // Command
                case ']': // Command
                case ':': // Block
                case '-': // Arrow
                    return false; // Only a few symbols interrupt normal text
                default:
                    return true;
            }
        }

        private static bool IsFullLineTextChar(char ch)
        {
            if (ch == '\n' || ch == '\r' || ch == '\0')
            {
                return false;
            }

            return ch switch
            {
                ':' => false, // Block
                _ => true
            };
        }

        private static bool IsDigit(char ch)
        {
            return ch is >= '0' and <= '9';
        }

        /// <summary>
        /// Get ident's type.
        /// </summary>
        /// <param name="ident">The ident string to check.</param>
        /// <returns>
        /// Keyword if it is in command context and ident is keyword;
        /// CommandText if it is in command context and ident is not keyword;
        /// otherwise, normal Text.
        /// </returns>
        private TokenType LookupKeywordOrText(string ident)
        {
            return _isCommandContext switch
            {
                true when Keywords.TryGetValue(ident, out TokenType type) => type,
                true => TokenType.CommandText,
                false => TokenType.Text
            };
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Lexer for dialoguer file.
    /// </summary>
    public static class Lexer
    {
        public static List<Token> Tokenize(string source)
        {
            var lexer = new LexerInstance(source);
            var tokens = new List<Token>();

            Token tok = lexer.NextToken();
            while (tok.Type != TokenType.EOF)
            {
                tokens.Add(tok);
                tok = lexer.NextToken();
            }
            // Optionally add EOF token to the list:
            // tokens.Add(tok);

            return tokens;
        }
    }
}