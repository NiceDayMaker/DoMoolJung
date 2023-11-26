using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DoMoolJung
{
	public static class BuiltInMethods
	{
		//출력
		public static Token Print(Token[] tokens)
		{
			if (tokens[0].returnType == token_type.Float)
			{
				Console.Write(string.Format("{0:0.000000}", double.Parse(tokens[0].text)));
			}
			else
			{
				Console.Write(tokens[0].text);
			}
			return new Token("", -1, token_type.Void, token_type.None);
		}

		public static Token ConsoleColor(Token[] tokens)
		{
			if (int.TryParse(tokens[0].text, out int fc))
			{
				if (fc < 0 || fc > 15)
				{
					throw new Exception($"{tokens[0].text} : 잘못된 색코드입니다. 색코드는 0~15입니다. 자세한 내용은 도움말을 참고하십시오.");
				}
			}
			else
			{
				throw new Exception($"{tokens[0].text} : 잘못된 색코드입니다. 색코드는 0~15입니다. 자세한 내용은 도움말을 참고하십시오.");
			}
			if (int.TryParse(tokens[1].text, out int bc))
			{
				if (bc < 0 || bc > 15)
				{
					throw new Exception($"{tokens[1].text} : 잘못된 색코드입니다. 색코드는 0~15입니다. 자세한 내용은 도움말을 참고하십시오.");
				}
			}
			else
			{
				throw new Exception($"{tokens[1].text} : 잘못된 색코드입니다. 색코드는 0~15입니다. 자세한 내용은 도움말을 참고하십시오.");
			}

			Console.ForegroundColor = (System.ConsoleColor)fc;
			Console.BackgroundColor = (System.ConsoleColor)bc;
			return new Token("", -1, token_type.Void, token_type.None);
		}

		//입력
		public static Token Input(Token[] tokens)
		{
			return new Token(Console.ReadLine(), -1, token_type.Var, token_type.String);
		}

		//지우기
		public static Token Clear(Token[] tokens)
		{
			Console.BackgroundColor = System.ConsoleColor.Black;
			Console.ForegroundColor = System.ConsoleColor.White;
			Console.Clear();
			return new Token("", -1, token_type.Void, token_type.None);
		}

		//자료형 변환
		public static Token Casting(Token[] tokens)
		{
			if (tokens[1].type != token_type.Declar) throw new Exception($"[{tokens[1].Line}] {tokens[1].type} {tokens[1].returnType} {tokens[1].text} : 잘못된 자료형입니다.");
			switch (tokens[1].returnType)
			{
				case token_type.Int:
					{
						if (double.TryParse(tokens[0].text, out double a))
						{
							return new Token(((long)a).ToString(), -1, token_type.Var, tokens[1].returnType);
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

		//기다리기
		public static Token Delay(Token[] tokens)
		{
			if (long.Parse(tokens[0].text) < 0) throw new Exception($"[{tokens[0].Line}] {tokens[0].text} : 0보다 작은 시간을 지연시킬 수 없습니다.");
			Thread.Sleep(int.Parse(tokens[0].text));
			return new Token("", -1, token_type.Void, token_type.None);
		}

		//이동
		public static Token Goto(Token[] tokens)
		{
			MainWindow.TokenIndex = int.Parse(tokens[0].text);
			return new Token();
		}

		//----------환경변수----------

		public static bool isInformDuplicateDeclar;

		//----------환경변수----------

		public static Token EnvVar(Token[] tokens)
		{
			switch (tokens[0].text)
			{
				case "중복선언경고":
					{
						isInformDuplicateDeclar = tokens[1].text != "0";
						break;
					}
				default:
					{
						throw new Exception($"[{tokens[0].Line}] {tokens[0].text} : 존재하지 않는 환경변수입니다.");
					}
			}
			return new Token();
		}

		public static Token PythonRunner(Token[] tokens)
		{
			List<Token> tmpArgs = new();
			MainWindow.TokenIndex++;
			for (; MainWindow.Tokens[MainWindow.TokenIndex].type != token_type.Code; MainWindow.TokenIndex++)
			{
				if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.Var)
				{
					tmpArgs.Add(MainWindow.Tokens[MainWindow.TokenIndex]);
				}
				else if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.Formula)
				{
					tmpArgs.Add(Calc(true));
				}
				else if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.Equation)
				{
					tmpArgs.Add(Calc(false));
				}
				else if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.Ident)
				{
					if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token arg_Method))
					{
						if (arg_Method.returnType != token_type.Int && arg_Method.returnType != token_type.Float && arg_Method.returnType != token_type.String) throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text}: 잘못된 자료형입니다.");
						Token res2 = MainWindow.Run(arg_Method);
						if (res2.type != token_type.Var) throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text}: 잘못된 자료형입니다.");
						tmpArgs.Add(res2);
					}
					else if (MainWindow.Variables.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Variable))
					{
						if (tmp_Variable.type != token_type.Var) throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text}: 잘못된 자료형입니다.");
						tmpArgs.Add(tmp_Variable);
					}
					else
					{
						throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
					}
				}
				else
				{
					throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
				}
			}

			Token code = MainWindow.Tokens[MainWindow.TokenIndex];
			string pycode = "args = list()\n";
			for (int i = 0; i < tmpArgs.Count; i++)
			{
				switch (tmpArgs[i].returnType)
				{
					case token_type.Int:
						{
							pycode += $"args.append(int(\"{tmpArgs[i].text}\"))\n";
							break;
						}
					case token_type.Float:
						{
							pycode += $"args.append(float(\"{tmpArgs[i].text}\"))\n";
							break;
						}
					case token_type.String:
						{
							pycode += $"args.append(\"{tmpArgs[i].text}\")\n";
							break;
						}
					default:
						{
							throw new Exception($"{tmpArgs[i].type} {tmpArgs[i].returnType} {tmpArgs[i].text} : 잘못된 자료형입니다.");
						}
				}
			}
			pycode += code.text;

			DirectoryInfo di = new DirectoryInfo($"{Path.GetTempPath()}DoMoolJung");
			if (!di.Exists)
				di.Create();
			string FileName = $"{Path.GetTempPath()}DoMoolJung\\{Path.GetRandomFileName()}";

			using (var sw = new StreamWriter(File.Create(FileName)))
			{
				sw.Write($"{pycode}");
			}

			Token res = new Token($"{{List:String}}", MainWindow.Tokens[MainWindow.TokenIndex].Line, token_type.List, token_type.String, new token_type[0] { }, ListHandler);
			Process pro = new Process();

			pro.StartInfo.FileName = "python";
			pro.StartInfo.Arguments = FileName;
			pro.StartInfo.CreateNoWindow = true;
			pro.StartInfo.UseShellExecute = false;
			pro.StartInfo.RedirectStandardOutput = true;
			pro.StartInfo.RedirectStandardError = true;
			pro.OutputDataReceived += (object sender, DataReceivedEventArgs outLine) =>
			{
				if (outLine.Data == null)
				{
					return;
				}
				else if (outLine.Data.StartsWith("echo "))
				{
					res.datas.Add(new Token(outLine.Data[5..], MainWindow.Tokens[MainWindow.TokenIndex].Line, token_type.Var, token_type.String));
				}
				else
				{
					Console.WriteLine(outLine.Data);
				}
			};

			try
			{
				pro.Start();
				pro.BeginOutputReadLine();
			}
			catch
			{
				throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 기기에 설치된 파이썬을 찾을 수 없습니다. 파이썬을 설치한 후 환경변수에 등록해 주십시오.");
			}

			string err = pro.StandardError.ReadToEnd();
			if (err != "")
			{
				throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : \n\n{err}");
			}

			if (File.Exists(pycode))
				File.Delete(FileName);
			return res;
		}


		//연산 및 판단
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
					if (MainWindow.Variables.TryGetValue(a.text, out tmp))
						a = tmp;
					else if (MainWindow.Methods.TryGetValue(a.text, out tmp))
						a = MainWindow.Run(tmp);
					else
						throw new Exception($"[{a.Line}] {a.text}: 잘못된 식별자입니다.");
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
					if (MainWindow.Variables.TryGetValue(b.text, out tmp))
						b = tmp;
					else if (MainWindow.Methods.TryGetValue(b.text, out tmp))
						b = MainWindow.Run(tmp);
					else
						throw new Exception($"[{b.Line}] {b.text}: 잘못된 식별자입니다.");
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
					if (MainWindow.Variables.TryGetValue(a.text, out tmp))
						a = tmp;
					else if (MainWindow.Methods.TryGetValue(a.text, out tmp))
						a = MainWindow.Run(tmp);
					else
						throw new Exception($"[{a.Line}] {a.text}: 잘못된 식별자입니다.");
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
					if (MainWindow.Variables.TryGetValue(b.text, out tmp))
						b = tmp;
					else if (MainWindow.Methods.TryGetValue(b.text, out tmp))
						b = MainWindow.Run(tmp);
					else
						throw new Exception($"[{b.Line}] {b.text}: 잘못된 식별자입니다.");
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
					return a + b;
				case token_type.Minus:
					return a - b;
				case token_type.Multi:
					return a * b;
				case token_type.Divi:
					return a / b;
				case token_type.Modulo:
					return a % b;
				case token_type.Equal:
					return a == b;
				case token_type.Not_Equal:
					return a != b;
				case token_type.Greater:
					return a > b;
				case token_type.Less:
					return a < b;
				default:
					return new Token("", a.Line, token_type.Null, token_type.Null);
			}
		}

		//배열
		public static Token ListHandler(Token[] tokens)
		{
			string thisList = MainWindow.Tokens[MainWindow.TokenIndex++].text;
			int count = -1;

			switch (MainWindow.Tokens[MainWindow.TokenIndex].type)
			{
				case token_type.Reset:
					{
						MainWindow.Methods[thisList].datas.Clear();
						return new Token("", -1, token_type.Void, token_type.None);
					}
				case token_type.Add:
					{
						switch (MainWindow.Tokens[++MainWindow.TokenIndex].type)
						{
							case token_type.Ident:
								{
									if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Method2))
									{
										if (tmp_Method2.returnType == MainWindow.Methods[thisList].returnType)
										{
											MainWindow.Methods[thisList].datas.Add(MainWindow.Run(tmp_Method2));
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else if (MainWindow.Variables.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Variable2))
									{
										if (tmp_Variable2.returnType == MainWindow.Methods[thisList].returnType)
										{
											MainWindow.Methods[thisList].datas.Add(tmp_Variable2);
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
									}
									break;
								}
							case token_type.Var:
								{
									if (MainWindow.Tokens[MainWindow.TokenIndex].returnType == MainWindow.Methods[thisList].returnType)
									{
										MainWindow.Methods[thisList].datas.Add(MainWindow.Tokens[MainWindow.TokenIndex]);
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									break;
								}
							case token_type.Formula:
								{
									Token res = Calc(true);

									if (res.type == token_type.Null || res.returnType != MainWindow.Methods[thisList].returnType)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									MainWindow.Methods[thisList].datas.Add(res);
									break;
								}
							case token_type.Equation:
								{
									Token res = Calc(false);

									if (res.type == token_type.Null || res.returnType != token_type.Int)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									MainWindow.Methods[thisList].datas.Add(res);
									break;
								}
							default:
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 인수값입니다.");
								}
						}
						return new Token("", -1, token_type.Void, token_type.None);
					}
				case token_type.Remove:
					{
						switch (MainWindow.Tokens[++MainWindow.TokenIndex].type)
						{
							case token_type.Ident:
								{
									if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Method2))
									{
										if (tmp_Method2.returnType == token_type.Int)
										{
											count = int.Parse(MainWindow.Run(tmp_Method2).text);
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else if (MainWindow.Variables.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Variable2))
									{
										if (tmp_Variable2.returnType == token_type.Int)
										{
											count = int.Parse(tmp_Variable2.text);
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type}   {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
									}
									break;
								}
							case token_type.Var:
								{
									if (MainWindow.Tokens[MainWindow.TokenIndex].returnType == token_type.Int)
									{
										count = int.Parse(MainWindow.Tokens[MainWindow.TokenIndex].text);
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									break;
								}
							case token_type.Formula:
								{
									Token res = Calc(true);

									if (res.type == token_type.Null || res.returnType != token_type.Int)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									count = int.Parse(res.text);
									break;
								}
							case token_type.Equation:
								{
									Token res = Calc(false);

									if (res.type == token_type.Null || res.returnType != token_type.Int)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									count = int.Parse(res.text);
									break;
								}
							default:
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 인수값입니다.");
								}
						}

						if (0 <= count && count < MainWindow.Methods[thisList].datas.Count)
							MainWindow.Methods[thisList].datas.RemoveAt(count);
						else
							throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {count} : 배열 범위를 벗어났습니다.");

						return new Token("", -1, token_type.Void, token_type.None);
					}
				case token_type.Size:
					{
						return new Token(MainWindow.Methods[thisList].datas.Count.ToString(), -1, token_type.Var, token_type.Int);
					}
				case token_type.Assign:
					{
						MainWindow.TokenIndex++;
						if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.Ident)
						{
							if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Method))
							{
								if (tmp_Method.type == token_type.Method && tmp_Method.returnType == token_type.StringList)
								{
									MainWindow.Methods[thisList] = MainWindow.Run(tmp_Method);
									return new Token("", -1, token_type.Void, token_type.None);
								}
								else
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
								}
							}
							else
							{
								throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
							}
						}
						else if (MainWindow.Tokens[MainWindow.TokenIndex].type == token_type.List && MainWindow.Tokens[MainWindow.TokenIndex].returnType == MainWindow.Methods[thisList].returnType)
						{
							MainWindow.Methods[thisList] = MainWindow.Tokens[MainWindow.TokenIndex];
							return new Token("", -1, token_type.Void, token_type.None);
						}
						else
						{
							throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
						}
					}
				case token_type.Index:
					{
						switch (MainWindow.Tokens[++MainWindow.TokenIndex].type)
						{
							case token_type.Ident:
								{
									if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Method2))
									{
										if (tmp_Method2.returnType == token_type.Int)
										{
											count = int.Parse(MainWindow.Run(tmp_Method2).text);
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else if (MainWindow.Variables.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Variable2))
									{
										if (tmp_Variable2.returnType == token_type.Int)
										{
											count = int.Parse(tmp_Variable2.text);
										}
										else
										{
											throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type}   {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
									}
									break;
								}
							case token_type.Var:
								{
									if (MainWindow.Tokens[MainWindow.TokenIndex].returnType == token_type.Int)
									{
										count = int.Parse(MainWindow.Tokens[MainWindow.TokenIndex].text);
									}
									else
									{
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									break;
								}
							case token_type.Formula:
								{
									Token res = Calc(true);

									if (res.type == token_type.Null || res.returnType != token_type.Int)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									count = int.Parse(res.text);
									break;
								}
							case token_type.Equation:
								{
									Token res = Calc(false);

									if (res.type == token_type.Null || res.returnType != token_type.Int)
									{
										MainWindow.TokenIndex--;
										throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
									}
									count = int.Parse(res.text);
									break;
								}
							default:
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 인수값입니다.");
								}
						}
						break;
					}
				default:
					{
						throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 명령어입니다.");
					}
			}

			if (MainWindow.Tokens[++MainWindow.TokenIndex].type == token_type.Assign)
			{
				switch (MainWindow.Tokens[++MainWindow.TokenIndex].type)
				{
					case token_type.Ident:
						{
							if (MainWindow.Methods.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Method2))
							{
								if (tmp_Method2.returnType == MainWindow.Methods[thisList].returnType)
								{
									MainWindow.Methods[thisList].datas[count] = MainWindow.Run(tmp_Method2);
								}
								else
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
								}
							}
							else if (MainWindow.Variables.TryGetValue(MainWindow.Tokens[MainWindow.TokenIndex].text, out Token tmp_Variable2))
							{
								if (tmp_Variable2.returnType == MainWindow.Methods[thisList].returnType)
								{
									MainWindow.Methods[thisList].datas[count] = tmp_Variable2;
								}
								else
								{
									throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
								}
							}
							else
							{
								throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 식별자입니다.");
							}
							break;
						}
					case token_type.Var:
						{
							if (MainWindow.Tokens[MainWindow.TokenIndex].returnType == MainWindow.Methods[thisList].returnType)
							{
								MainWindow.Methods[thisList].datas[count] = MainWindow.Tokens[MainWindow.TokenIndex];
							}
							else
							{
								throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
							}
							break;
						}
					case token_type.Formula:
						{
							Token res = Calc(true);

							if (res.type == token_type.Null || res.returnType != MainWindow.Methods[thisList].returnType)
							{
								MainWindow.TokenIndex--;
								throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
							}
							MainWindow.Methods[thisList].datas[count] = res;
							break;
						}
					case token_type.Equation:
						{
							Token res = Calc(false);

							if (res.type == token_type.Null || res.returnType != token_type.Int)
							{
								MainWindow.TokenIndex--;
								throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 자료형입니다.");
							}
							MainWindow.Methods[thisList].datas[count] = res;
							break;
						}
					default:
						{
							throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {MainWindow.Tokens[MainWindow.TokenIndex].text} : 잘못된 인수값입니다.");
						}
				}
				return new Token("", -1, token_type.Void, token_type.None);
			}
			else
			{
				MainWindow.TokenIndex--;
				if (0 <= count && count < MainWindow.Methods[thisList].datas.Count)
					return MainWindow.Methods[thisList].datas[count];
				else if (0 > count && count >= -MainWindow.Methods[thisList].datas.Count)
					return MainWindow.Methods[thisList].datas[^(-count)];
				else
					throw new Exception($"{MainWindow.Tokens[MainWindow.TokenIndex].type} {MainWindow.Tokens[MainWindow.TokenIndex].returnType} {count} : 배열 범위를 벗어났습니다.");
			}
		}

	}

	public struct Token
	{
		public string text;
		public long Line;
		public token_type type;
		public token_type returnType;
		public token_type[] argTypes;
		public List<Token> args;
		public List<Token> datas;
		public Func<Token[], Token> func;

		public Token(string s, long line, token_type t, token_type r, token_type[] argType, Func<Token[], Token> fun)
		{
			text = s;
			Line = line;
			type = t;
			returnType = r;
			argTypes = argType;
			func = fun;
			args = new();
			datas = new();
		}

		public Token(string s, long line, token_type t, token_type r)
		{
			text = s;
			Line = line;
			type = t;
			returnType = r;
			argTypes = Array.Empty<token_type>();
			func = null;
			args = new();
			datas = new();
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
								return new Token(((long)(av + bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token(((long)(av - bv)).ToString(), a.Line, a.type, a.returnType);
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
								return new Token(((long)(av * bv)).ToString(), a.Line, a.type, a.returnType);
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
							for (long i = long.Parse(b.text); i > 0; i--)
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
								return new Token(((long)(av / bv)).ToString(), a.Line, a.type, a.returnType);
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

		public static Token operator %(Token a, Token b)
		{
			if ((a.returnType == token_type.Int || a.returnType == token_type.Float) && (b.returnType == token_type.Int || b.returnType == token_type.Float))
			{
				if (a.returnType == token_type.Int && b.returnType == token_type.Int)
				{
					if (long.TryParse(a.text, out long av) && long.TryParse(b.text, out long bv))
					{
						return new Token((av % bv).ToString(), a.Line, a.type, token_type.Int);
					}
					throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
				}
				else
				{
					if (double.TryParse(a.text, out double av) && double.TryParse(b.text, out double bv))
					{
						return new Token((av % bv).ToString(), a.Line, a.type, token_type.Float);
					}
					throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
				}
			}
			else
			{
				throw new Exception($"[{a.Line}] {a.text} {b.text} : 잘못된 자료형입니다.");
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
		None = 0, Null,
		Method, Flag, If, Else, Reset, Remove, Add, Size,//함수
		Void, Int, Float, String, List, StringList, Code, Index,//리터럴
		Ident, Declar, Var, Formula, Equation,//식별자
		Operator, Plus, Minus, Multi, Divi, Modulo, //연산자
		Brack, Brack_end,//소괄호
		Block, Block_end,//블록
		Assign,//대입 =
		Equal, Not_Equal, Less, Greater,//== != < > (비교 연산자)
	};
}
