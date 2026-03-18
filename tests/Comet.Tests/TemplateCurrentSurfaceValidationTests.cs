using System;
using System.IO;
using Xunit;
using IOPath = System.IO.Path;

namespace Comet.Tests
{
	public class TemplateCurrentSurfaceValidationTests
	{
		static readonly string RepoRoot = IOPath.GetFullPath(IOPath.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
		static readonly string TemplateRoot = IOPath.Combine(RepoRoot, "templates", "single-project", "CometApp1");

		[Fact]
		public void SingleProjectTemplateUsesCurrentComponentSurface()
		{
			var appSource = ReadTemplateFile("App.cs");
			var mainPageSource = ReadTemplateFile("MainPage.cs");
			var combined = $"{appSource}{Environment.NewLine}{mainPageSource}";

			Assert.DoesNotMatch(@"\[Body\]", combined);
			Assert.DoesNotMatch(@"\[State\]", combined);
			Assert.Matches(@"Body\s*=\s*\(\)\s*=>\s*new\s+MainPage\s*\(\s*\)", appSource);
			Assert.Matches(@":\s*Component<\s*CounterState\s*>", mainPageSource);
			Assert.Matches(@"override\s+(?:Comet\.)?View\s+Render\s*\(", mainPageSource);
			Assert.Matches(@"\bReactive<", mainPageSource);
			Assert.Matches(@"\bSetState\s*\(", mainPageSource);
		}

		[Fact]
		public void SingleProjectTemplateTargetsCurrentMauiAndDropsLegacyDependencies()
		{
			var projectFile = ReadTemplateFile("CometApp1.csproj");

			Assert.Contains("net10.0-maccatalyst", projectFile, StringComparison.Ordinal);
			Assert.DoesNotContain("net7.0", projectFile, StringComparison.OrdinalIgnoreCase);
			Assert.DoesNotContain("Reloadify3000", projectFile, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("MAUI_VERSION", projectFile, StringComparison.Ordinal);
			Assert.Contains("COMET_VERSION", projectFile, StringComparison.Ordinal);
		}

		static string ReadTemplateFile(string relativePath)
		{
			var fullPath = IOPath.Combine(TemplateRoot, relativePath);
			Assert.True(File.Exists(fullPath), $"Expected template file at {fullPath}");
			return File.ReadAllText(fullPath);
		}
	}
}
