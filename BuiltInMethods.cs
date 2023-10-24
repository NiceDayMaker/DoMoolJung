using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DoMoolJung
{
	public static class BuiltInMethods
	{
		public static Token MethodRunner(Token[] tokens)
		{
			return new Token();
		}

		public static Token Print(Token[] tokens)
		{
			if (tokens[0].type == token_type.Method)
			{
				if (MainWindow.Methods.TryGetValue(tokens[0].text, out Token tmp_Method))
				{
					if (tmp_Method.returnType == token_type.Int || tmp_Method.returnType == token_type.Float || tmp_Method.returnType == token_type.String)
					{
						Console.Write(tokens[0].Run().text);
					}
					else
					{
						throw new Exception($"[{tokens[0].Line}] {tokens[0].type} {tokens[0].returnType} {tokens[0].text} : 잘못된 자료형입니다.");
					}
				}
			}
			else if (tokens[0].returnType == token_type.Float)
			{
				Console.Write(string.Format("{0:0.000000}", double.Parse(tokens[0].text)));
			}
			else
			{
				Console.Write(tokens[0].text);
			}
			return new Token("", -1, token_type.Void, token_type.None);
		}

		public static Token Input(Token[] tokens)
		{
			return new Token(Console.ReadLine() ?? "", -1, token_type.Var, token_type.String);
		}

		public static Token Clear(Token[] tokens)
		{
			Console.Clear();
			Console.WriteLine($"[##################################################] = 100.000000%\n--------------------------------------------------\n\n");
			return new Token("", -1, token_type.Void, token_type.None);
		}

		public static Token Casting(Token[] tokens)
		{
			if (tokens[1].type != token_type.Declar) throw new Exception($"[{tokens[1].Line}] {tokens[1].type} {tokens[1].returnType} {tokens[1].text} : 잘못된 자료형입니다.");
			switch (tokens[1].returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(tokens[0].text, out double a))
						{
							return new Token(((int)a).ToString(), -1, token_type.Var, tokens[1].returnType);
						}
						else
						{
							throw new Exception($"[{tokens[0].Line}] {tokens[0].type} {tokens[0].returnType} {tokens[0].text} : 정수형으로 변환할 수 없습니다.");
						}
					}
				case token_type.Float:
					{
						if (double.TryParse(tokens[0].text, out double a))
						{
							return new Token(a.ToString(), -1, token_type.Var, tokens[1].returnType);
						}
						else
						{
							throw new Exception($"[{tokens[0].Line}] {tokens[0].type} {tokens[0].returnType} {tokens[0].text} : 실수형으로 변환할 수 없습니다.");
						}
					}
				case token_type.String:
					{
						return new Token(tokens[0].text, -1, token_type.Var, tokens[1].returnType);
					}
				default:
					{
						throw new Exception($"[{tokens[1].Line}] {tokens[1].type} {tokens[1].returnType} {tokens[1].text} : 잘못된 자료형입니다.");
					}
			}
		}

		public static Token Delay(Token[] tokens)
		{
			if (int.Parse(tokens[0].text) < 0) throw new Exception($"[{tokens[0].Line}] {tokens[0].text} : 0보다 작은 시간을 지연시킬 수 없습니다.");
			Thread.Sleep(int.Parse(tokens[0].text));
			return new Token("", -1, token_type.Void, token_type.None);
		}

		public static Token Goto(Token[] tokens)
		{
			MainWindow.TokenIndex = int.Parse(tokens[0].text);
			return new Token();
		}

		public static Token Calc(bool isFormula)
		{
			Token a;
			Token b;
			Token op;
			Token tmp;

			if (isFormula)
			{
				a = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (a.type == token_type.Ident)
				{
					if (MainWindow.Variables.TryGetValue(a.text, out tmp)) a = tmp;
					else throw new Exception($"[{a.Line}] {a.text}: 잘못된 식별자입니다.");
				}
				else if (a.type == token_type.Equation)
				{
					a = Calc(false);
				}
				else if (a.type == token_type.Formula)
				{
					a = Calc(true);
				}

				MainWindow.TokenIndex++;

				b = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (b.type == token_type.Ident)
				{
					if (MainWindow.Variables.TryGetValue(b.text, out tmp)) b = tmp;
					else throw new Exception($"[{b.Line}] {b.text}: 잘못된 식별자입니다.");
				}
				else if (b.type == token_type.Equation)
				{
					b = Calc(false);
				}
				else if (b.type == token_type.Formula)
				{
					b = Calc(true);
				}

				MainWindow.TokenIndex++;

				op = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (op.type != token_type.Operator) throw new Exception($"[{op.Line}]  {op.text}: 잘못된 연산자입니다.");
			}
			else
			{
				a = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (a.type == token_type.Ident)
				{
					if (MainWindow.Variables.TryGetValue(a.text, out tmp)) a = tmp;
					else throw new Exception($"[{a.Line}] {a.text}: 잘못된 식별자입니다.");
				}
				else if (a.type == token_type.Equation)
				{
					a = Calc(false);
				}
				else if (a.type == token_type.Formula)
				{
					a = Calc(true);
				}

				op = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (op.type != token_type.Operator) throw new Exception($"[{op.Line}]  {op.text}: 잘못된 연산자입니다.");

				b = MainWindow.Tokens[++MainWindow.TokenIndex];
				if (b.type == token_type.Ident)
				{
					if (MainWindow.Variables.TryGetValue(b.text, out tmp)) b = tmp;
					else throw new Exception($"[{b.Line}] {b.text}: 잘못된 식별자입니다.");
				}
				else if (b.type == token_type.Equation)
				{
					b = Calc(false);
				}
				else if (b.type == token_type.Formula)
				{
					b = Calc(true);
				}
			}

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
						return new Token("", a.Line, token_type.Null, token_type.Null);
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
		public List<Token> codes;
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
			codes = new();
		}

		public Token(string s, int line, token_type t, token_type r)
		{
			text = s;
			Line = line;
			type = t;
			returnType = r;
			argTypes = Array.Empty<token_type>();
			args = new();
			func = null;
			codes = new();
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
								return new Token(((int)(av + bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token((av + bv).ToString(), a.Line, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.String:
					{
						if (b.type == token_type.Var)
						{
							return new Token(a.text + b.text, a.Line, a.type, a.returnType);
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
								return new Token(((int)(av - bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token((av - bv).ToString(), a.Line, a.type, a.returnType);
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
									a.text = a.text.Remove(a.text.IndexOf(item), 1);
								}
							}
							return new Token(a.text, a.Line, a.type, a.returnType);
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
								return new Token(((int)(av * bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token((av * bv).ToString(), a.Line, a.type, a.returnType);
							}
						}
						throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
					}
				case token_type.String:
					{
						if (b.returnType == token_type.Int)
						{
							StringBuilder tmp = new();
							for (int i = int.Parse(b.text); i > 0; i--)
							{
								tmp.Append(a.text);
							}
							return new Token(tmp.ToString(), a.Line, a.type, a.returnType);
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
								return new Token(((int)(av / bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token((av / bv).ToString(), a.Line, a.type, a.returnType);
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
					return new Token("1", a.Line, a.type, a.returnType);
				}
				else
				{
					return new Token("0", a.Line, a.type, a.returnType);
				}
			}
			else
			{
				return new Token("0", a.Line, a.type, a.returnType);
			}
		}

		public static Token operator !=(Token a, Token b)
		{
			if (a.returnType == b.returnType)
			{
				if (a.text != b.text)
				{
					return new Token("1", a.Line, a.type, a.returnType);
				}
				else
				{
					return new Token("0", a.Line, a.type, a.returnType);
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
								return new Token(av > bv ? "1" : "0", a.Line, token_type.Var, token_type.Int);
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
								return new Token(av > bv ? "1" : "0", a.Line, token_type.Var, token_type.Int);
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
								return new Token(av < bv ? "1" : "0", a.Line, token_type.Var, token_type.Int);
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
								return new Token(av < bv ? "1" : "0", a.Line, token_type.Var, token_type.Int);
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

		public Token Run()
		{
			try
			{
				if (func == null)
				{
					return new Token();
				}
				else
				{
					return func(args.ToArray());
				}
			}
			catch
			{
				throw;
			}
		}
	}

	public enum token_type
	{
		None = 0,//아무것도 아님
		Null,//빈 줄

		Method, Flag, If, Else,//함수

		Void, Int, Float, String, //리터럴
		Ident, Declar, MethodDeclar, Var, Formula, Equation, Python, Return,//식별자

		Operator, Plus, Minus, Multi, Divi, //연산자
		Brack, Brack_end,//소괄호
		Block, Block_end,//블록
		Assign,//대입 =
		Comma,//콤마
		Equal, Not_Equal, Less, Greater,//== != < > (비교 연산자)
	};
}
