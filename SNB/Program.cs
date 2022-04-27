using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNB
{
	static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Nes.Init(new SnbGame());
		}
	}
}
