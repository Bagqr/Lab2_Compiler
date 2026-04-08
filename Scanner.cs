using System.Collections.Generic;
using System.Text;

namespace WpfApp1
{
    public class Scanner
    {
        private static readonly Dictionary<string, int> Keywords = new Dictionary<string, int>
        {
            {"for", 1},
            {"in", 2},
            {"range", 3}
        };

        private static readonly Dictionary<int, string> TypeNames = new Dictionary<int, string>
        {
            {1, "ключевое слово (for)"},
            {2, "ключевое слово (in)"},
            {3, "ключевое слово (range)"},
            {10, "идентификатор"},
            {20, "целое число"},
            {30, "оператор присваивания (=)"},
            {31, "оператор сравнения (==)"},
            {32, "оператор меньше (<)"},
            {33, "оператор больше (>)"},
            {34, "оператор меньше или равно (<=)"},
            {35, "оператор больше или равно (>=)"},
            {36, "оператор неравно (!=)"},
            {40, "оператор сложения (+)"},
            {41, "оператор вычитания (-)"},
            {42, "оператор умножения (*)"},
            {43, "оператор деления (/)"},
            {44, "оператор остатка (%)"},
            {50, "разделитель (пробел/табуляция)"},
            {51, "разделитель (новая строка)"},
            {60, "двоеточие (:)"},
            {61, "запятая (,)"},
            {62, "открывающая скобка (()"},
            {63, "закрывающая скобка ())"},
            {90, "недопустимый символ"}
        };

        public List<Lexem> Analyze(string text)
        {
            var result = new List<Lexem>();
            if (string.IsNullOrEmpty(text)) return result;

            int line = 1;         
            int pos = 1;           
            int idx = 0;           
            int len = text.Length;

            while (idx < len)
            {
                char ch = text[idx];

                if (ch == '\n')
                {
                    result.Add(CreateLexem(51, "\n", line, pos, pos));
                    line++;
                    pos = 1;
                    idx++;
                    continue;
                }
                if (ch == '\r')
                {
                    idx++;
                    continue;
                }

                if (ch == ' ' || ch == '\t')
                {
                    int startPos = pos;
                    var sb = new StringBuilder();
                    while (idx < len && (text[idx] == ' ' || text[idx] == '\t'))
                    {
                        sb.Append(text[idx]);
                        idx++;
                        pos++;
                    }
                    result.Add(CreateLexem(50, sb.ToString(), line, startPos, pos - 1));
                    continue;
                }

                if (char.IsDigit(ch))
                {
                    int startPos = pos;
                    var sb = new StringBuilder();
                    while (idx < len && char.IsDigit(text[idx]))
                    {
                        sb.Append(text[idx]);
                        idx++;
                        pos++;
                    }
                    result.Add(CreateLexem(20, sb.ToString(), line, startPos, pos - 1));
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    int startPos = pos;
                    var sb = new StringBuilder();
                    while (idx < len && (char.IsLetterOrDigit(text[idx]) || text[idx] == '_'))
                    {
                        sb.Append(text[idx]);
                        idx++;
                        pos++;
                    }
                    string word = sb.ToString();
                    if (Keywords.TryGetValue(word, out int code))
                    {
                        result.Add(CreateLexem(code, word, line, startPos, pos - 1));
                    }
                    else
                    {
                        result.Add(CreateLexem(10, word, line, startPos, pos - 1));
                    }
                    continue;
                }

                if (idx + 1 < len)
                {
                    string twoChars = text.Substring(idx, 2);
                    int codeTwo = GetTwoCharOperatorCode(twoChars);
                    if (codeTwo != -1)
                    {
                        result.Add(CreateLexem(codeTwo, twoChars, line, pos, pos + 1));
                        idx += 2;
                        pos += 2;
                        continue;
                    }
                }

                int codeSingle = GetSingleCharOperatorCode(ch);
                if (codeSingle != -1)
                {
                    result.Add(CreateLexem(codeSingle, ch.ToString(), line, pos, pos));
                    idx++;
                    pos++;
                    continue;
                }

                result.Add(CreateLexem(90, ch.ToString(), line, pos, pos, isError: true));
                idx++;
                pos++;
            }

            return result;
        }

        private int GetTwoCharOperatorCode(string op)
        {
            switch (op)
            {
                case "==": return 31;
                case "<=": return 34;
                case ">=": return 35;
                case "!=": return 36;
                default: return -1;
            }
        }

        private int GetSingleCharOperatorCode(char ch)
        {
            switch (ch)
            {
                case '=': return 30;
                case '<': return 32;
                case '>': return 33;
                case '+': return 40;
                case '-': return 41;
                case '*': return 42;
                case '/': return 43;
                case '%': return 44;
                case ':': return 60;
                case ',': return 61;
                case '(': return 62;
                case ')': return 63;
                default: return -1;
            }
        }

        private Lexem CreateLexem(int code, string value, int line, int start, int end, bool isError = false)
        {
            string typeName = TypeNames.ContainsKey(code) ? TypeNames[code] : "неизвестно";
            return new Lexem
            {
                Code = code,
                Type = typeName,
                Value = value,
                Line = line,
                StartPos = start,
                EndPos = end,
                IsError = isError
            };
        }
    }
}