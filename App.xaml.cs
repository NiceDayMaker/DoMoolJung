using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace DoMoolJung
{
	public partial class App : Application
	{
		static string ext = ".dmj";
		static string fileTypeDesc = "DoMoolJung Source Code File";
		static string extType = "DoMoolJung" + ext + ".v1";
		static string assocExeFileName = "DoMoolJung.exe";

		void Startup_Method(object sender, StartupEventArgs e)
		{
			bool register = true;

            if (e.Args.Length >= 1)
			{
				if (e.Args[0] == "del")
				{
					register = false;
				}
			}
			ProcessFileExtReg(register);

			MainWindow tmp = new MainWindow();
			if (e.Args.Length >= 1)
			{
				if (File.Exists(e.Args[0]))
				{
					tmp.OpenFileOnStart(e.Args[0]);
				}
				else
				{
					MessageBox.Show($"[{e.Args[0]}]은 읽을 수 있는 파일이 아닌 것 같습니다.", "파일 인식 실패", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			tmp.Show();
		}

		private static void ProcessFileExtReg(bool register)
		{
			using (RegistryKey classesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
			{
				if (register)
				{
					using (RegistryKey extKey = classesKey.CreateSubKey(ext, true))
					{
						extKey.SetValue(null, extType);
					}

					// or, use Registry.SetValue method
					using (RegistryKey typeKey = classesKey.CreateSubKey(extType, true))
					{
						typeKey.SetValue(null, fileTypeDesc);
						typeKey.SetValue("EditFlags", new byte[] { 00, 00, 01, 00 }, RegistryValueKind.Binary);
						using (RegistryKey shellKey = typeKey.CreateSubKey("shell", true))
						{
							using (RegistryKey openKey = shellKey.CreateSubKey("open", true))
							{
								using (RegistryKey commandKey = openKey.CreateSubKey("command", true))
								{
									string assocCommand = string.Format("\"{0}\" \"%1\"", GetProcessPath());
									commandKey.SetValue(null, assocCommand);
								}
							}
						}
					}
				}
				else
				{
					DeleteRegistryKey(classesKey, ext, false);
					DeleteRegistryKey(classesKey, extType, true);
					MessageBox.Show("두물정과 관련된 모든 레지스트리 및 임시파일이 제거되었습니다.", "임시파일 제거 완료", MessageBoxButton.OK, MessageBoxImage.Information);
					Environment.Exit(0);
				}
				RegistApplication(classesKey, register);
			}
		}

		private static void RegistApplication(RegistryKey classesKey, bool register)
		{
			using (RegistryKey appKey = classesKey.OpenSubKey("Applications", true))
			{
				if (register == true)
				{
					using (RegistryKey exeKey = appKey.CreateSubKey(assocExeFileName))
					{
						RegistShellOpenCommand(exeKey);
					}
				}
			}
		}

		private static void RegistShellOpenCommand(RegistryKey baseKey)
		{
			using (RegistryKey shellKey = baseKey.CreateSubKey("shell"))
			{
				using (RegistryKey openKey = shellKey.CreateSubKey("open"))
				{
					using (RegistryKey commandKey = openKey.CreateSubKey("command"))
					{
						string assocExePath = GetProcessPath();
						string assocCommand = string.Format("\"{0}\" \"%1\"", assocExePath);

						commandKey.SetValue(null, assocCommand);
					}
				}
			}
		}

		private static void DeleteRegistryKey(RegistryKey classesKey, string subKeyName, bool deleteAllSubKey)
		{
			if (CheckRegistryKeyExists(classesKey, subKeyName) == false)
			{
				return;
			}

			if (deleteAllSubKey == true)
			{
				classesKey.DeleteSubKeyTree(subKeyName);
			}
			else
			{
				classesKey.DeleteSubKey(subKeyName);
			}
		}

		private static bool CheckRegistryKeyExists(RegistryKey classesKey, string subKeyName)
		{
			RegistryKey regKey = null;

			try
			{
				regKey = classesKey.OpenSubKey(subKeyName);
				return regKey != null;
			}
			finally
			{
				if (regKey != null)
				{
					regKey.Close();
				}
			}
		}

		private static string GetProcessPath()
		{
			string path = AppDomain.CurrentDomain.BaseDirectory;
			return Path.Combine(path, assocExeFileName);
		}
	}
}
