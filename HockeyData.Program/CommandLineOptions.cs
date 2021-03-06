﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Program
{
	public class CommandLineOptions
	{
		[Option('i', "initializeFullLoad", Required = false, HelpText = "Initialize database in full.")]
		public bool InitializeFullLoadTask { get; set; }
	}
}
