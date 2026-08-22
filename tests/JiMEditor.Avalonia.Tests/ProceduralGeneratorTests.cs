using JiME;
using JiME.Procedural;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class ProceduralGeneratorTests
{
	[Fact]
	public void GenerateScenario_with_defaults_produces_scenario()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.EnsureUtilsInitialized();

			var generator = new ProceduralGenerator();
			var ctx = generator.GenerateScenario( generator.GetDefaultParameters() );

			Assert.False( ctx.HasErrors, string.Join( "\n", ctx.GeneratorLogs.Select( l => l.Message ) ) );
			Assert.NotNull( ctx.Scenario );
			Assert.NotEmpty( ctx.Scenario!.chapterObserver );
		} );
	}
}
