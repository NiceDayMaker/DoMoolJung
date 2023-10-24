using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DoMoolJung
{
	public partial class MainWindow : Window
	{
		public static List<Token> Tokens = new();
		public static Dictionary<string, Token> Methods = new();
		public static Dictionary<string, Token> Variables = new();
		public static readonly string[] UnUsableIdent = new string[32] { "반환", "함수", "파이썬", "공허", "변환", "기다리기", "지우기", "출력", "입력", "정수", "실수", "문자열", "깃발", "이동", "식", "논리", "더하기", "뺴기", "곱하기", "나누기", "같다", "다르다", "크다", "작다", "면", "이면", "만약", "아니면", "한다", "은", "는", "주석" };

		Stopwatch watch = new Stopwatch();

		public bool waitForInit = true;
		public MainWindow()
		{
			InitializeComponent();
			filePath = "이름없음";
			Title = $"ㄷ井 두물정 1.0 - {filePath}";
			Console.Title = Title;
			waitForInit = false;
			ScrSync();

			CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
		}

		int fontSize = 100;
		Stopwatch kc = Stopwatch.StartNew();
		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.Escape))
			{
				StopWithKeyInterrupt = true;
			}
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (Source.IsEnabled)
				{
					if (Keyboard.IsKeyDown(Key.S))
					{
						Source.IsEnabled = false;
						btnSaveFile_Click(sender, new());
						Source.IsEnabled = true;
					}
					else if (Keyboard.IsKeyDown(Key.O))
					{
						Source.IsEnabled = false;
						btnOpenFile_Click(sender, new());
						Source.IsEnabled = true;
					}
					else if (Keyboard.IsKeyDown(Key.P))
					{
						OnCompile(sender, new());
					}
				}
				if (kc.ElapsedMilliseconds > 50 && Source.IsFocused)
				{
					if (Keyboard.IsKeyDown(Key.OemMinus))
					{
						if (fontSize > 20)
						{
							fontSize -= 5;
							Source.FontSize -= .75;
							Source.Padding = new Thickness(Source.Padding.Left - 1.4, 1, 0, 0);
							LinerWidth.Width = new GridLength(LinerWidth.Width.Value - 1.4);
							Liner.FontSize -= .75;
							Scale.Content = $"{fontSize}%";
						}

					}
					else if (Keyboard.IsKeyDown(Key.OemPlus))
					{
						if (fontSize < 500)
						{
							fontSize += 5;
							Source.FontSize += .75;
							Source.Padding = new Thickness(Source.Padding.Left + 1.4, 1, 0, 0);
							LinerWidth.Width = new GridLength(LinerWidth.Width.Value + 1.4);
							Liner.FontSize += .75;
							Scale.Content = $"{fontSize}%";
						}
					}
					else if (Keyboard.IsKeyDown(Key.D0))
					{
						fontSize = 100;
						Source.FontSize = 15d;
						Source.Padding = new Thickness(40, 1, 0, 0);
						LinerWidth.Width = new GridLength(50);
						Liner.FontSize = 15d;
						Scale.Content = $"{fontSize}%";
					}
					kc.Restart();
				}
			}
		}

		public async void OpenFileOnStart(string inputFilePath)
		{
			await Task.Run(() => { while (waitForInit) ; });
			filePath = inputFilePath;
			Title = $"ㄷ井 두물정 1.0 - {filePath}";
			Console.Title = Title;
			Source.Text = File.ReadAllText(filePath);
		}

		public async void ScrSync()
		{
			while (true)
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

		private async void OnCompile(object sender, RoutedEventArgs e)
		{
			Initialize();

			Compile.IsEnabled = false;
			Source.IsEnabled = false;
			Save.IsEnabled = false;
			Open.IsEnabled = false;

			try
			{
				Compiler();
				Preprocessor();
				watch.Restart();

				await Task.Run(() => Runner());

				Console.WriteLine($"\n\n--------------------------------------------------\n{watch.Elapsed}에서 실행 완료됨");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n\n--------------------------------------------------\n{watch.Elapsed}에서 [{Tokens[TokenIndex].Line}] {ex.Message}");
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
			StopWithKeyInterrupt = false;

			Methods.Clear();
			Variables.Clear();
			Tokens.Clear();
			Console.Clear();
			Console.WriteLine("\x1b[3J");
			this.WindowState = WindowState.Minimized;

			//Built-in methods
			Methods.Add("출력", new Token("출력", -1, token_type.Method, token_type.Void, new token_type[1] { token_type.None }, BuiltInMethods.Print));
			Methods.Add("입력", new Token("입력", -1, token_type.Method, token_type.String, new token_type[0] { }, BuiltInMethods.Input));
			Methods.Add("이동", new Token("이동", -1, token_type.Method, token_type.Void, new token_type[1] { token_type.Ident }, BuiltInMethods.Goto));
			Methods.Add("변환", new Token("변환", -1, token_type.Method, token_type.Var, new token_type[2] { token_type.Ident, token_type.None }, BuiltInMethods.Casting));
			Methods.Add("기다리기", new Token("기다리기", -1, token_type.Method, token_type.Void, new token_type[1] { token_type.Int }, BuiltInMethods.Delay));
			Methods.Add("지우기", new Token("지우기", -1, token_type.Method, token_type.Void, new token_type[0] { }, BuiltInMethods.Clear));
		}

		public void Preprocessor()
		{
			//변수 선언 전처리
			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
			{
				if (Tokens[TokenIndex].type == token_type.Ident && Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp))
				{
					TokenIndex += tmp.argTypes.Length;
				}
				else if (Tokens[TokenIndex].type == token_type.Declar)
				{
					Token tmp_Variable = Tokens[TokenIndex];
					Tokens.RemoveAt(TokenIndex);
					string ident = Tokens[TokenIndex].text;
					if (UnUsableIdent.Contains(ident)) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 내장 구문은 식별자로 지정할 수 없습니다.");
					if (Methods.ContainsKey(ident) || Variables.ContainsKey(ident)) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 이미 선언된 식별자입니다.");

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
											Tokens.RemoveAt(TokenIndex);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
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
											Tokens.RemoveAt(TokenIndex);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
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
											Tokens.RemoveAt(TokenIndex);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
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
							default:
								{
									if (Tokens[TokenIndex].type != token_type.Null)
										throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 구문 입니다.");
									break;
								}
						}
						TokenIndex--;
					}
					else if (Tokens[TokenIndex].type == token_type.MethodDeclar)
					{
						Tokens.RemoveAt(TokenIndex);

						if (Tokens[TokenIndex].type == token_type.Ident)
						{
							string Mident = Tokens[TokenIndex].text;
							if (UnUsableIdent.Contains(Mident)) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 내장 구문은 식별자로 지정할 수 없습니다.");
							if (Methods.ContainsKey(Mident) || Variables.ContainsKey(Mident)) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 이미 선언된 식별자입니다.");
							Tokens.RemoveAt(TokenIndex);

							Token tmp_Method = new Token("", tmp_Variable.Line, token_type.Method, tmp_Variable.returnType, new token_type[0], BuiltInMethods.MethodRunner);

							if (tmp_Variable.type == token_type.Declar)
							{
								while (Tokens[TokenIndex].type != token_type.Null)
								{
									if (Tokens[TokenIndex].type == token_type.Block_end)
									{
										Tokens.RemoveAt(TokenIndex);
										break;
									}

									tmp_Method.codes.Add(Tokens[TokenIndex]);
									Tokens.RemoveAt(TokenIndex);
								}
							}
							else
							{
								if (Tokens[TokenIndex].type != token_type.Null)
									throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 구문 입니다.");
								break;
							}
							TokenIndex--;
							Methods.Add(Mident, tmp_Method);
						}
						else
						{
							throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
						}

					}
					else
					{
						throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
					}
				}
			}
		}

		public static int TokenIndex;
		public bool StopWithKeyInterrupt;
		public void Runner()
		{
			for (TokenIndex = 0; TokenIndex < Tokens.Count; TokenIndex++)
			{
				if (StopWithKeyInterrupt)
				{
					throw new Exception($": 키보드 인터럽트로 인해 실행이 중지되었습니다.");
				}
				switch (Tokens[TokenIndex].type)
				{
					//식별자
					case token_type.Ident:
						{
							//함수
							if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method))
							{
								Run(tmp_Method);
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
											if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method2))
											{
												tmp_Method2 = Run(tmp_Method2);
												if (tmp_Method2.returnType == token_type.Int || tmp_Method2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(tmp_Method2.text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(((int)double.Parse(tmp_Variable2.text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(((int)double.Parse(Tokens[TokenIndex].text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Formula)
										{
											Token res = BuiltInMethods.Calc(true);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token res = BuiltInMethods.Calc(false);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									//실수
									else if (tmp_Variable.returnType == token_type.Float)
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method2))
											{
												tmp_Method2 = Run(tmp_Method2);
												if (tmp_Method2.returnType == token_type.Int || tmp_Method2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(tmp_Method2.Run().text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token((double.Parse(tmp_Variable2.text)).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].returnType == token_type.Int || Tokens[TokenIndex].returnType == token_type.Float)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(double.Parse(Tokens[TokenIndex].text).ToString(), tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Formula)
										{
											Token res = BuiltInMethods.Calc(true);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token res = BuiltInMethods.Calc(false);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
									//문자열
									else if (tmp_Variable.returnType == token_type.String)
									{
										if (Tokens[TokenIndex].type == token_type.Ident)
										{
											if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method2))
											{
												tmp_Method2 = Run(tmp_Method2);
												if (tmp_Method2.returnType == token_type.Int || tmp_Method2.returnType == token_type.Float || tmp_Method2.returnType == token_type.String)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(tmp_Method2.Run().text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
											{
												if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float || tmp_Variable2.returnType == token_type.String)
												{
													Variables.Remove(tokenName);
													Variables.Add(tokenName, new Token(tmp_Variable2.text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
												}
												else
												{
													throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
												}
											}
											else
											{
												throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
											}
										}
										else if (Tokens[TokenIndex].type == token_type.Var)
										{
											Variables.Remove(tokenName);
											Variables.Add(tokenName, new Token(Tokens[TokenIndex].text, tmp_Variable.Line, tmp_Variable.type, tmp_Variable.returnType));
										}
										else if (Tokens[TokenIndex].type == token_type.Formula)
										{
											Token res = BuiltInMethods.Calc(true);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else if (Tokens[TokenIndex].type == token_type.Equation)
										{
											Token res = BuiltInMethods.Calc(false);

											if (res.type == token_type.Null || res.returnType != tmp_Variable.returnType) throw new Exception($"[{Tokens[--TokenIndex].Line}] {Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");

											Variables.Remove(tokenName);
											Variables.Add(tokenName, res);
										}
										else
										{
											throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
										}
									}
								}
								else
								{
									throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 대입, 호출, 증가 및 감소 구문으로만 사용할 수 있습니다.");
								}
							}
							else
							{
								throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
							}
							break;
						}
					//만약
					case token_type.If:
						{
							TokenIndex++;
							if (Tokens[TokenIndex].type == token_type.Formula)
							{
								Token res = BuiltInMethods.Calc(true);

								if (res.type == token_type.Null || (res.returnType != token_type.Int && res.returnType != token_type.Float)) throw new Exception($" {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");
								if (Tokens[++TokenIndex].type != token_type.Brack_end) throw new Exception($" {Tokens[TokenIndex].text}: 조건문이 정상적으로 닫히지 않았습니다.");
								if (float.Parse(res.text) < 0.5f)
								{
									int cnt = 1;
									while (cnt > 0)
									{
										switch (Tokens[TokenIndex++].type)
										{
											case token_type.If:
												{
													cnt++;
													break;
												}
											case token_type.Else:
												{
													cnt++;
													break;
												}
											case token_type.Block_end:
												{
													cnt--;
													break;
												}
										}
									}
									if (Tokens[TokenIndex].type != token_type.Else)
									{
										TokenIndex--;
									}
								}
							}
							else if (Tokens[TokenIndex].type == token_type.Equation)
							{
								Token res = BuiltInMethods.Calc(false);

								if (res.type == token_type.Null || (res.returnType != token_type.Int && res.returnType != token_type.Float)) throw new Exception($" {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");
								if (Tokens[++TokenIndex].type != token_type.Brack_end) throw new Exception($" {Tokens[TokenIndex].text}: 조건문이 정상적으로 닫히지 않았습니다.");

								if (float.Parse(res.text) < 0.5f)
								{
									int cnt = 1;
									while (cnt > 0)
									{
										switch (Tokens[TokenIndex++].type)
										{
											case token_type.If:
												{
													cnt++;
													break;
												}
											case token_type.Block_end:
												{
													cnt--;
													break;
												}
										}
									}
									if (Tokens[TokenIndex].type != token_type.Else)
									{
										TokenIndex--;
									}
								}
							}
							else if (Tokens[TokenIndex].type == token_type.Ident)
							{
								if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Method2))
								{
									tmp_Method2 = Run(tmp_Method2);
									if (tmp_Method2.returnType == token_type.Int || tmp_Method2.returnType == token_type.Float)
									{
										if (float.Parse(tmp_Method2.text) < 0.5f)
										{
											int cnt = 1;
											while (cnt > 0)
											{
												switch (Tokens[TokenIndex++].type)
												{
													case token_type.If:
														{
															cnt++;
															break;
														}
													case token_type.Block_end:
														{
															cnt--;
															break;
														}
												}
											}
											if (Tokens[TokenIndex].type != token_type.Else)
											{
												TokenIndex--;
											}
										}
									}
									else
									{
										throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
									}
								}
								else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable2))
								{
									if (tmp_Variable2.returnType == token_type.Int || tmp_Variable2.returnType == token_type.Float)
									{
										if (float.Parse(tmp_Variable2.text) < 0.5f)
										{
											int cnt = 1;
											while (cnt > 0)
											{
												switch (Tokens[TokenIndex++].type)
												{
													case token_type.If:
														{
															cnt++;
															break;
														}
													case token_type.Block_end:
														{
															cnt--;
															break;
														}
												}
											}
											if (Tokens[TokenIndex].type != token_type.Else)
											{
												TokenIndex--;
											}
										}
									}
									else
									{
										throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 자료형입니다.");
									}
								}
								else
								{
									throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
								}
							}
							else
							{
								if (float.TryParse(Tokens[TokenIndex].text, out float b))
								{
									if (Tokens[++TokenIndex].type != token_type.Brack_end) throw new Exception($" {Tokens[TokenIndex].text}: 조건문이 정상적으로 닫히지 않았습니다.");
									if (b < 0.5f)
									{
										int cnt = 1;
										while (cnt > 0)
										{
											switch (Tokens[TokenIndex++].type)
											{
												case token_type.If:
													{
														cnt++;
														break;
													}
												case token_type.Block_end:
													{
														cnt--;
														break;
													}
											}
										}
										if (Tokens[++TokenIndex].type != token_type.Else)
										{
											TokenIndex--;
										}
									}
								}
								else
								{
									throw new Exception($" {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");
								}
							}
							break;
						}
					case token_type.Else:
						{
							while (Tokens[TokenIndex].type != token_type.Block_end) TokenIndex++;
							break;
						}
					default:
						{
							if (Tokens[TokenIndex].type != token_type.Null && Tokens[TokenIndex].type != token_type.Block_end)
								throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 구문 입니다.");
							break;
						}
				}
			}
		}

		public static Token Run(Token tmp_Method)
		{
			tmp_Method.args.Clear();
			TokenIndex++;
			for (int j = 0; j < tmp_Method.argTypes.Length; j++, TokenIndex++)
			{
				if (Tokens[TokenIndex].type == token_type.Formula)
				{
					Token res = BuiltInMethods.Calc(true);

					if (res.type == token_type.Null || (tmp_Method.argTypes[j] != token_type.None && res.returnType != tmp_Method.argTypes[j])) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

					tmp_Method.args.Add(res);
				}
				else if (Tokens[TokenIndex].type == token_type.Equation)
				{
					Token res = BuiltInMethods.Calc(false);

					if (res.type == token_type.Null || (tmp_Method.argTypes[j] != token_type.None && res.returnType != tmp_Method.argTypes[j])) throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text}: 잘못된 자료형입니다.");

					tmp_Method.args.Add(res);
				}
				else if ((tmp_Method.argTypes[j] != token_type.None && Tokens[TokenIndex].returnType != tmp_Method.argTypes[j]) || Tokens[TokenIndex].type == token_type.Null)
				{
					throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 인수값입니다.");
				}
				else if (Tokens[TokenIndex].type == token_type.Ident)
				{
					if (Methods.TryGetValue(Tokens[TokenIndex].text, out Token arg_Method))
					{
						tmp_Method.args.Add(Run(arg_Method));
					}
					else if (Variables.TryGetValue(Tokens[TokenIndex].text, out Token tmp_Variable))
					{
						tmp_Method.args.Add(tmp_Variable);
					}
					else
					{
						throw new Exception($"{Tokens[TokenIndex].type} {Tokens[TokenIndex].returnType} {Tokens[TokenIndex].text} : 잘못된 식별자입니다.");
					}
				}
				else
				{
					tmp_Method.args.Add(Tokens[TokenIndex]);
				}
			}
			TokenIndex--;
			return tmp_Method.Run();
		}

		void Compiler()
		{
			Index = 0;
			Line = 0;
			c = ' ';

			while (c != '\0')
			{
				string tok = ""; //토큰 문자열
				if (c == '\n')
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
								case "주석":
									{
										while (c != '\0' && c != '\n')
										{
											c = getChar();
										}
										Line++;
										break;
									}
								case "함수":
									{
										Tokens.Add(new Token("", Line, token_type.MethodDeclar, token_type.None));
										break;
									}
								case "공허":
									{
										Tokens.Add(new Token("", Line, token_type.Declar, token_type.Void));
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
								case "파이썬":
									{
										string pycode = "";
										pycode += getChar();
										while (c != '\0')
										{
											pycode += getChar();
											if (pycode[-2..-1] == "반환")
											{
												pycode = pycode[..-3];
											}
										}
										Tokens.Add(new Token(pycode, Line, token_type.Declar, token_type.Python));
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
								case "이면":
									{
										Tokens.Add(new Token("", Line, token_type.Brack_end, token_type.None));
										break;
									}
								case "면":
									{
										Tokens.Add(new Token("", Line, token_type.Brack_end, token_type.None));
										break;
									}
								case "논리":
									{
										Tokens.Add(new Token("", Line, token_type.Formula, token_type.None));
										break;
									}
								case "같다":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Equal));
										break;
									}
								case "다르다":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Not_Equal));
										break;
									}
								case "크다":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Greater));
										break;
									}
								case "작다":
									{
										Tokens.Add(new Token("", Line, token_type.Operator, token_type.Less));
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
								case "반환":
									{
										Tokens.Add(new Token("", Line, token_type.Return, token_type.None));
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
		bool isChanged;
		private void Source_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			isChanged = true;
			Title = $"ㄷ井 두물정 1.0 - {filePath}*";
		}

		private void btnSaveFile_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "DoMoolJung File (*.dmj)|*.dmj";
			if (saveFileDialog.ShowDialog() == true)
			{
				filePath = saveFileDialog.FileName;
				Title = $"ㄷ井 두물정 1.0 - {filePath}";
				Console.Title = Title;
				File.WriteAllText(filePath, Source.Text);
				isChanged = false;
			}
		}

		private void btnOpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.Filter = "DoMoolJung File (*.dmj)|*.dmj";
			if (openFileDialog.ShowDialog() == true)
			{
				filePath = openFileDialog.FileName;
				Title = $"ㄷ井 두물정 1.0 - {filePath}";
				Console.Title = Title;
				Source.Text = File.ReadAllText(filePath);
			}
		}

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			new Helper().Show();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (isChanged == true)
			{
				switch (MessageBox.Show("아직 저장하지 않은 내용이 있습니다. 저장하시겠습니까?", "확인", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
				{
					case MessageBoxResult.Yes:
						{
							SaveFileDialog saveFileDialog = new SaveFileDialog();
							saveFileDialog.Filter = "DoMoolJung File (*.dmj)|*.dmj";
							if (saveFileDialog.ShowDialog() == true)
							{
								File.WriteAllText(saveFileDialog.FileName, Source.Text);
							}
							else
							{
								e.Cancel = true;
							}
							break;
						}
					case MessageBoxResult.Cancel:
						{
							e.Cancel = true;
							break;
						}
				}
			}
		}
	}
}