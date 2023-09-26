using System;
using System.Collections.Generic;

namespace DoMoolJung
{
	public static class BuiltInMethods
	{
		public static Token print(Token[] tokens)
		{
			if (tokens[0].returnType == token_type.Float)
			{
				Console.Write(string.Format("{0:0.000000}", double.Parse(tokens[0].text)));
			}
			else
			{
				Console.Write(tokens[0].text);
			}
			return new Token("", -1, token_type.Void, token_type.None, null, null);
		}

		public static Token Goto(Token[] tokens)
		{
			MainWindow.TokenIndex = int.Parse(tokens[0].text);
			return new Token();
		}

		public static Token Calc(Token a, Token b, Token op)
		{
			if (a.type == token_type.Ident)
			{
				if (!MainWindow.Variables.TryGetValue(a.text, out a)) throw new Exception($"[{a.Line}]  {a.text}: 잘못된 식별자입니다.");
			}
			if (b.type == token_type.Ident)
			{
				if (!MainWindow.Variables.TryGetValue(b.text, out b)) throw new Exception($"[{b.Line}]  {b.text}: 잘못된 식별자입니다.");
			}
			if (op.type != token_type.Operator) throw new Exception($"[{op.Line}]  {op.text}: 잘못된 연산자입니다.");

			switch (op.returnType)
			{
				case token_type.Plus:
					{
						return a + b;
					}
				case token_type.Minus:
					{
						return a - b;
					}
				case token_type.Multi:
					{
						return a * b;
					}
				case token_type.Divi:
					{
						return a / b;
					}
				case token_type.Equal:
					{
						return a == b;
					}
				case token_type.Not_Equal:
					{
						return a != b;
					}
				case token_type.Greater:
					{
						return a > b;
					}
				case token_type.Less:
					{
						return a < b;
					}
				default:
					{
						return new Token("", -1, token_type.Null, token_type.Null);
					}
			}
		}
	}

	public struct Token
	{
		public string text;
		public int Line;
		public token_type type;
		public token_type returnType;
		public token_type[] argTypes;
		public List<Token> args;
		public Func<Token[], Token> func;

		public Token(string s, int line, token_type t, token_type r, token_type[] argType, Func<Token[], Token> fun)
		{
			text = s;
			Line = line;
			type = t;
			returnType = r;
			argTypes = argType;
			func = fun;
			args = new();
		}

		public Token(string s, int line, token_type t, token_type r)
		{
			text = s;
			Line = line;
			type = t;
			returnType = r;
			argTypes = null;
			func = null;
		}

		public static Token operator +(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(((int)(av + bv)).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token((av + bv).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.String:
					{
						if (b.type == token_type.Var)
						{
							return new Token(a.text + b.text, -1, a.type, a.returnType);
						}
						else
						{
							throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
						}
					}
				default:
					{
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
			}
		}

		public static Token operator -(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(((int)(av - bv)).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token((av - bv).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.String:
					{
						if (b.type == token_type.Var)
						{
							foreach (char item in b.text)
							{
								while (a.text.Contains(item))
								{
									a.text.Remove(a.text.IndexOf(item));
								}
							}
							return new Token(a.text, -1, a.type, a.returnType);
						}
						else
						{
							throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
						}
					}
				default:
					{
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
			}
		}

		public static Token operator *(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(((int)(av * bv)).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token((av * bv).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.String:
					{
						if (b.returnType == token_type.Int)
						{
							string tmp = "";
							for (int i = int.Parse(b.text); i > 0; i--)
							{
								tmp += a.text;
							}
							return new Token(tmp, -1, a.type, a.returnType);
						}
						else
						{
							throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
						}
					}
				default:
					{
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
			}
		}

		public static Token operator /(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(((int)(av / bv)).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token((av / bv).ToString(), -1, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				default:
					{
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
			}
		}

		public static Token operator ==(Token a, Token b)
		{
			if (a.returnType == b.returnType)
			{
				if (a.text == b.text)
				{
					return new Token("1", -1, a.type, a.returnType);
				}
				else
				{
					return new Token("0", -1, a.type, a.returnType);
				}
			}
			else
			{
				throw new Exception($"[{a.Line}] : {a.returnType} 와 {b.returnType} 끼리 서로 비교할 수 없습니다.");
			}
		}

		public static Token operator !=(Token a, Token b)
		{
			if (a.returnType == b.returnType)
			{
				if (a.text != b.text)
				{
					return new Token("1", -1, a.type, a.returnType);
				}
				else
				{
					return new Token("0", -1, a.type, a.returnType);
				}
			}
			else
			{
				throw new Exception($"[{a.Line}] : {a.returnType} 와 {b.returnType} 끼리 서로 비교할 수 없습니다.");
			}
		}

		public static Token operator >(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(av > bv ? "1" : "0", -1, token_type.Var, token_type.Int);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(av > bv ? "1" : "0", -1, token_type.Var, token_type.Int);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				default:
					{
						throw new Exception($"[{a.Line}] : {a.returnType} 와 {b.returnType} 끼리 서로 비교할 수 없습니다.");
					}
			}
		}

		public static Token operator <(Token a, Token b)
		{
			switch (a.returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(av < bv ? "1" : "0", -1, token_type.Var, token_type.Int);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.Float:
					{
						if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
						{
							if (b.returnType == token_type.Int || b.returnType == token_type.Float)
							{
								return new Token(av < bv ? "1" : "0", -1, token_type.Var, token_type.Int);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				default:
					{
						throw new Exception($"[{a.Line}] : {a.returnType} 와 {b.returnType} 끼리 서로 비교할 수 없습니다.");
					}
			}
		}

		public void Run()
		{
			func(args.ToArray());
		}
	}

	public enum token_type
	{
		None = 0,//아무것도 아님
		Null,//빈 줄

		Method, Flag, If, Else,//함수

		Void, Int, Float, String, //리터럴
		Ident, Declar, Var, Formula, Equation,//식별자

		Operator, Plus, Minus, Multi, Divi, //연산자
		Brack, Brack_end,//소괄호
		Block, Block_end,//블록
		Assign,//대입 =
		Comma,//콤마
		Equal, Not_Equal, Less, Greater,//== != < > (비교 연산자)
		And, Or//논리 연산자
	};
}
