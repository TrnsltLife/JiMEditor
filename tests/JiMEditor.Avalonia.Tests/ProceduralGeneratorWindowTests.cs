using JiME.Procedural;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

/// <summary>
/// Window ctor smoke tests are not used: Avalonia <c>Window</c> must be created on the
/// dispatcher thread that called <c>SetupWithoutStarting</c>, and <see cref="ModelTestEnvironment.RunInSta"/>
/// uses a fresh STA thread. UI is exercised via Project/Campaign; logic is covered by
/// <see cref="ProceduralGeneratorTests"/>.
/// </summary>
public sealed class ProceduralGeneratorWindowTests
{
	[Fact]
	public void Default_parameters_are_usable_without_opening_the_window()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.EnsureUtilsInitialized();
			var generator = new ProceduralGenerator();
			var parameters = generator.GetDefaultParameters();
			Assert.NotNull( parameters );
			Assert.True( parameters.MaxThreat > 0 );
		} );
	}
}
