using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DoMoolJung
{
	public partial class MainWindow : Window
	{
		public static List<Token> Tokens = new();
		public static Dictionary<string, Token> Methods = new();
		public static Dictionary<string, Token> Variables = new();
		public MainWindow()
		{
			InitializeComponent();
			filePath = "이름없음";
			ScrSync();
		}
		
		public async void ScrSync()
		{
			while(true)
			{
				await Task.Delay(10);
				Liner.Text = "1";
				int len = Source.Text.Split('\n').Length + 1;
				for (int i = 2; i < len; i++)
				{
					Liner.Text += $"\n{i}";
				}
				Liner.ScrollToVerticalOffset(Source.VerticalOffset);
			}
		}

		private void OnCompile(object sender, RoutedEventArgs e)
		{
			Initialize();

			Compile.IsEnabled = false;
			Source.IsEnabled = false;
			Save.IsEnabled = false;
			Open.IsEnabled = false;
			Compiler();
			Runner();
			try
			{
				
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n\n--------------------------------------------------\n{ex.Message}");
			}
			finally
			{
				Compile.IsEnabled = true;
				Source.IsEnabled = true;
				Save.IsEnabled = true;
				Open.IsEnabled = true;
			}
		}

		private void Initialize()
		{
			Methods.Clear();
			Variables.Clear();
			Tokens.Clear();
			Console.Clear();

			//Built-in methods
			Methods.Add("출력", new Token("출력", Line, token_type.Method, token_type.Void, new token_type[1] { token_type.None }, BuiltInMethods.print));
			Methods.Add("이동", new Token("이동", Line, token_type.Method, token_type.Void, new token_type[1] { token_type.Ident }, BuiltInMethods.Goto));
		}

		public static int TokenIndex;
		public bool StopByKeyInterrupt;
		void Runner()
		{
			//변수 선언 전처리
			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
			{

				if (Tokens[TokenIndex].type == token_type.Declar)
				{
					Token tmp_Variable = Tokens[TokenIndex];
					Tokens.RemoveAt(TokenIndex);
					string ident = Tokens[TokenIndex].text;
					if (Variables.ContainsKey(ident)) throw new Exception($"[{Tokens[TokenIndex].Line}]  {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 이미 선언된 식별자입니다.");

					if (Tokens[TokenIndex].type == token_type.Ident)
					{
						Tokens.RemoveAt(TokenIndex);
						switch (tmp_Variable.returnType)
						{
							case token_type.Int:
								{
									if (Tokens[TokenIndex].type == token_type.Assign)
									{
										Tokens.RemoveAt(TokenIndex);
										if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Add(ident, new Token(((int)double.Parse(Tokens[TokenIndex].text)).ToString(), tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										Variables.Add(ident, new Token("0", tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
									}
									break;
								}
							case token_type.Float:
								{
									if (Tokens[TokenIndex].type == token_type.Assign)
									{
										Tokens.RemoveAt(TokenIndex);
										if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Add(ident, new Token(double.Parse(Tokens[TokenIndex].text).ToString(), tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										Variables.Add(ident, new Token("0.0", tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
									}
									break;
								}
							case token_type.String:
								{
									if (Tokens[TokenIndex].type == token_type.Assign)
									{
										Tokens.RemoveAt(TokenIndex);
										if (Tokens[TokenIndex].returnType == token_type.String)
										{
											Variables.Add(ident, new Token(Tokens[TokenIndex].text, tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									else
									{
										Variables.Add(ident, new Token("", tmp_Variable.Line, token_type.Var, tmp_Variable.returnType));
									}
									break;
								}
							case token_type.Flag:
								{
									Variables.Add(ident, new Token((TokenIndex - 1).ToString(), tmp_Variable.Line, token_type.Var, token_type.Flag));
									break;
								}
						}
						TokenIndex--;
					}
					else
					{
						throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
					}
				}
			}

			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
			{
				if (StopByKeyInterrupt)
				{
					StopByKeyInterrupt = false;
					Console.WriteLine("\n\n--------------------------------------------------\n실행 중지됨");
					return;
				}

				switch (Tokens[TokenIndex].type)
				{
					//식별자
					case token_type.Ident:
						{
							//함수
							if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method))
							{
								tmp_Method.args.Clear();
								TokenIndex++;
								for (int j = 0; j < tmp_Method.argTypes.Length; j++, TokenIndex++)
								{
									if (Tokens[TokenIndex].type == token_type.Equation)
									{
										Token a = Tokens[++TokenIndex];
										Token op = Tokens[++TokenIndex];
										Token b = Tokens[++TokenIndex];
										Token res = BuiltInMethods.Calc(a, b, op);

										if (res.type == token_type.Null || (tmp_Method.argTypes[j] != token_type.None && res.returnType != tmp_Method.argTypes[j])) throw new Exception($"[{Tokens[TokenIndex].Line}]  {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

										tmp_Method.args.Add(res);
									}
									else if ((tmp_Method.argTypes[j] != token_type.None && Tokens[TokenIndex].returnType != tmp_Method.argTypes[j]) || Tokens[TokenIndex].type == token_type.Null)
									{
										throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 인수값입니다.");
									}
									else
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable))
											{
												tmp_Method.args.Add(tmp_Variable);
											}
											else
											{
												throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else
										{
											tmp_Method.args.Add(Tokens[TokenIndex]);
										}
									}
								}
								TokenIndex--;
								if (tmp_Method.func != null)
								{
									tmp_Method.Run();
								}
							}
							//변수
							else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable))
							{
								string tokenName = Tokens[TokenIndex++].text;
								if (Tokens[TokenIndex++].type == token_type.Assign)
								{
									//정수
									if (tmp_Variable.returnType == token_type.Int)
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(((int)double.Parse(tmp_Variable2.text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(((int)double.Parse(Tokens[TokenIndex].text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token a = Tokens[++TokenIndex];
											Token op = Tokens[++TokenIndex];
											Token b = Tokens[++TokenIndex];
											Token res = BuiltInMethods.Calc(a, b, op);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[TokenIndex].Line}]  {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									//실수
									else if (tmp_Variable.returnType == token_type.Float)
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token((double.Parse(tmp_Variable2.text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(double.Parse(Tokens[TokenIndex].text).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token a = Tokens[++TokenIndex];
											Token op = Tokens[++TokenIndex];
											Token b = Tokens[++TokenIndex];
											Token res = BuiltInMethods.Calc(a, b, op);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									//문자열
									else if (tmp_Variable.returnType == token_type.String)
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(tmp_Variable2.text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].type == token_type.Var)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(Tokens[TokenIndex].text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token a = Tokens[++TokenIndex];
											Token op = Tokens[++TokenIndex];
											Token b = Tokens[++TokenIndex];
											Token res = BuiltInMethods.Calc(a, b, op);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
								}
								else
								{
									throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 대입, 호출, 증가 및 감소 구문으로만 사용할 수 있습니다.");
								}
							}
							else
							{
								throw new Exception($"[{Tokens[TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
							}
							break;
						}
					//만약
					case token_type.If:
						{
							TokenIndex++;
							if (Tokens[TokenIndex].type == token_type.Formula)
							{
								Token a = Tokens[++TokenIndex];
								TokenIndex++;
								Token b = Tokens[++TokenIndex];
								TokenIndex++;
								Token op = Tokens[++TokenIndex];

								Token res = BuiltInMethods.Calc(a, b, op);

								if (res.type == token_type.Null || res.returnType != token_type.Int) throw new Exception($"[{Tokens[TokenIndex].Line}]  {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

								if (int.Parse(res.text) == 0)
								{
									while (Tokens[TokenIndex].type != token_type.Else && Tokens[TokenIndex].type != token_type.Block_end) TokenIndex++;
								}
							}
							else
							{
								if (int.TryParse(Tokens[TokenIndex].text, out int b))
								{
									if (b == 0)
									{
										while (Tokens[TokenIndex].type != token_type.Else && Tokens[TokenIndex].type != token_type.Block_end) TokenIndex++;
									}
								}
								else
								{
									throw new Exception($"[{Tokens[TokenIndex].Line}]  {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");
								}
							}
							break;
						}
					case token_type.Else:
						{
							while (Tokens[TokenIndex++].type != token_type.Block_end) ;
							break;
						}
				}
			}
			Console.WriteLine("\n\n--------------------------------------------------\n실행 완료됨");
		}

		void Compiler()
		{
			Index = 0;
			Line = 0;
			c = ' ';

			while (c != '\0')
			{
				string tok = ""; //토큰 문자열
				if (c == '#')
				{
					while (c != '\0' && c != '\n')
					{
						c = getChar();
					}
					Line++;
				}
				else if (c == '\n')
				{
					c = getChar();
					Line++;
				}
				else if (c == '\r' || c == ' ' || c == '\t')
				{
					c = getChar();
				}
				//식별자
				else if (c.isKorean() || c == '_')
				{
					tok += c;
					while (c != '\n' && c != '\0')
					{
						c = getChar();
						if (!(c.isKorean() || " _1234567890\n\0\r".Contains(c)))
						{
							tok += c;
							throw new Exception($"[{Line}] {tok} {c} : 잘못된 문자");
						}

						if (c == ' ' || c == '\n' || c == '\0' || c == '\r')
						{
							switch (tok)
							{
								case "공허함수":
									{
										Tokens.Add(new Token("", Line, token_type.Method, token_type.Void, null, null));
										break;
									}
								case "정수함수":
									{
										Tokens.Add(new Token("", Line, token_type.Method, token_type.Int, null, null));
										break;
									}
								case "실수함수":
									{
										Tokens.Add(new Token("", Line, token_type.Method, token_type.Float, null, null));
										break;
									}
								case "문자열함수":
									{
										Tokens.Add(new Token("", Line, token_type.Method, token_type.String, null, null));
										break;
									}
								case "정수":
									{
										Tokens.Add(new Token("", Line, token_type.Declar, token_type.Int));
										break;
									}
								case "실수":
									{
										Tokens.Add(new Token("", Line, token_type.Declar, token_type.Float));
										break;
									}
								case "문자열":
									{
										Tokens.Add(new Token("", Line, token_type.Declar, token_type.String));
										break;
									}
								case "깃발":
									{
										Tokens.Add(new Token("", Line, token_type.Declar, token_type.Flag));
										break;
									}
								case "은":
									{
										Tokens.Add(new Token("", Line, token_type.Assign, token_type.None));
										break;
									}
								case "는":
									{
										Tokens.Add(new Token("", Line, token_type.Assign, token_type.None));
										break;
									}
								case "식":
									{
										Tokens.Add(new Token("", Line, token_type.Equation, token_type.None));
										break;
									}
								case "더하기":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Plus));
										break;
									}
								case "빼기":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Minus));
										break;
									}
								case "곱하기":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Multi));
										break;
									}
								case "나누기":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Divi));
										break;
									}
								case "만약":
									{
										Tokens.Add(new Token("", Line, token_type.If, token_type.None));
										break;
									}
								case "논리":
									{
										Tokens.Add(new Token("", Line, token_type.Formula, token_type.None));
										break;
									}
								case "같다면":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Equal));
										break;
									}
								case "다르다면":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Not_Equal));
										break;
									}
								case "크다면":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Greater));
										break;
									}
								case "작다면":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Less));
										break;
									}
								case "그리고":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.And));
										break;
									}
								case "또는":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Or));
										break;
									}
								case "아니면":
									{
										Tokens.Add(new Token("", Line, token_type.Else, token_type.None));
										break;
									}
								case "한다":
									{
										Tokens.Add(new Token("", Line, token_type.Block_end, token_type.None));
										break;
									}
								default:
									{
										Tokens.Add(new Token(tok, Line, token_type.Ident, token_type.Ident));
										break;
									}
							}
							break;
						}
						else
						{
							tok += c;
						}
					}
				}
				//문자
				else if (c == '\"')
				{
					string tmp = "";
					c = getChar();
					for (; c != '\"'; c = getChar())
					{
						if (c == '\0' || c == '\n') throw new Exception($"[{Line}] {tok} 입력 받은 값: {tmp} : 잘못된 문자열");
						if (c == '\\')
						{
							c = getChar();
							switch (c)
							{
								case '\\':
									{
										tmp += '\\';
										break;
									}
								case 'n':
									{
										tmp += '\n';
										break;
									}
								case 't':
									{
										tmp += '\t';
										break;
									}
								case '\"':
									{
										tmp += '\"';
										break;
									}
								default:
									{
										throw new Exception($"[{Line}] {tok} 입력 받은 값: {tmp} : 잘못된 문자열");
									}
							}
						}
						else
						{
							tmp += c;
						}
					}
					tmp.Replace("\\n", "\n");
					tmp.Replace("\\t", "\t");
					Tokens.Add(new Token(tmp, Line, token_type.Var, token_type.String));
					c = getChar();
				}
				//숫자
				else if ("-1234567890".Contains(c))
				{
					tok += c;
					while (c != '\n' && c != '\0')
					{
						c = getChar();
						if (!" .-1234567890\n\0\r".Contains(c))
						{
							tok += c;
							throw new Exception($"[{Line}] {tok} {c} : 잘못된 문자");
						}

						if (c == ' ' || c == '\n' || c == '\0' || c == '\r')
						{
							if (tok.Contains('.'))
							{
								if (double.TryParse(tok, out double result))
								{
									Tokens.Add(new Token(tok, Line, token_type.Var, token_type.Float));
									break;
								}
								else
								{
									throw new Exception($"[{Line}] {tok} : 잘못된 형식");
								}
							}
							else
							{
								if (int.TryParse(tok, out int result))
								{
									Tokens.Add(new Token(tok, Line, token_type.Var, token_type.Int));
									break;
								}
								else
								{
									throw new Exception($"[{Line}] {tok} : 잘못된 형식");
								}
							}
						}
						else
						{
							tok += c;
						}
					}
				}
				//예외
				else
				{
					tok += c;
					throw new Exception($"[{Line}] {tok} : 잘못된 문법");
				}
			}
			Tokens.Add(new Token("", Line, token_type.Null, token_type.Null));
			Tokens.Add(new Token("", Line, token_type.Null, token_type.Null));
			Tokens.Add(new Token("", Line, token_type.Null, token_type.Null));
			Tokens.Add(new Token("", Line, token_type.Null, token_type.Null));
		}

		int Index;
		int Line;
		char c;
		char getChar()
		{
			if (Index >= Source.Text.Length)
			{
				Console.SetCursorPosition(0, 0);
				Console.WriteLine($"[##################################################] = 100.000000%\n--------------------------------------------------\n\n");
				return '\0';
			}
			string bar = "[";
			for (int i = 0; i < 50; i++)
			{
				bar += ((float)Index / Source.Text.Length) * 50 > i ? '#' : '-';
			}
			Console.SetCursorPosition(0, 0);
			Console.WriteLine($"{bar}] = {((float)Index / Source.Text.Length) * 100f}%   ");

			return Source.Text[Index++];
		}

		string filePath;
		private void Source_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Title = $"ㄷ井 두물정 1.0 - {filePath}*";
		}

		private void textBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
			{
				Source.IsEnabled = false;
				btnSaveFile_Click(sender, e);
				Source.IsEnabled = true;
			}
			else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
			{
				Source.IsEnabled = false;
				btnOpenFile_Click(sender, e);
				Source.IsEnabled = true;
			}
		}

		private void btnSaveFile_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "DoMoolJung File (*.dmj)|*.dmj";
			if (saveFileDialog.ShowDialog() == true)
			{
				filePath = saveFileDialog.FileName;
				Title = $"ㄷ井 두물정 1.0 - {filePath}";
				File.WriteAllText(filePath, Source.Text);
			}
		}

		private void btnOpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "DoMoolJung File (*.dmj)|*.dmj";
			if (openFileDialog.ShowDialog() == true)
			{
				filePath = openFileDialog.FileName;
				Title = $"ㄷ井 두물정 1.0 - {filePath}";
				Source.Text = File.ReadAllText(filePath);
			}
		}
	}
}