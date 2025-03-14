﻿#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Text;
using Microsoft.CodeAnalysis;
using Uno.Roslyn;
using Uno.UI.SourceGenerators.Telemetry;

#if NETFRAMEWORK
using Uno.SourceGeneration;
#endif

namespace Uno.UI.SourceGenerators.XamlGenerator
{
#if NETFRAMEWORK
	[GenerateAfter("Uno.UI.SourceGenerators.DependencyObject." + nameof(DependencyObject.DependencyPropertyGenerator))]
#endif
	[Generator]
	public class XamlCodeGenerator : ISourceGenerator
	{
		private readonly GenerationRunInfoManager _generationRunInfoManager = new GenerationRunInfoManager();

		public void Initialize(GeneratorInitializationContext context)
		{
			DependenciesInitializer.Init();
		}

		public void Execute(GeneratorExecutionContext context)
		{
			// No initialization required for this one
			//if (!Process.GetCurrentProcess().ProcessName.Equals("omnisharp.exe", StringComparison.OrdinalIgnoreCase))
			//{
			//	Debugger.Launch();
			//}

			if (PlatformHelper.IsValidPlatform(context))
			{
				_generationRunInfoManager.Update(context);

				var gen = new XamlCodeGeneration(context);
				var genereratedTrees = gen.Generate(_generationRunInfoManager.CreateRun());

				foreach (var tree in genereratedTrees)
				{
					context.AddSource(tree.Key, tree.Value);
				}
			}
		}
	}
}
