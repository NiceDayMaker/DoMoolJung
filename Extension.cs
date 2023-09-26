using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoMoolJung
{
	public static class Extension
	{
		public static bool isKorean(this char input)
		{
			return (0x1100 <= input && input <= 0x11ff) || (0x3130 <= input && input <= 0x318f) || (0xa960 <= input && input <= 0xa97f) || (0xac00 <= input && input <= 0xd7af) || (0xd7b0 <= input && input <= 0xd7ff);
		}
	}
}
