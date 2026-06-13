using JiMEditor.Avalonia.Views;
using JiME;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class ProceduralGeneratorWindowTests
{
	[Fact]
	public void ProceduralGeneratorWindow_ctor_initializes_without_throwing()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();
			Utils.Init();

			var window = new ProceduralGeneratorWindow();
			Assert.NotNull( window.GeneratorParameters );
			Assert.True( window.GeneratorParameters.MaxThreat > 0 );
		} );
	}
}
