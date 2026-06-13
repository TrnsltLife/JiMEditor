using System.Linq;
using JiME;
using Newtonsoft.Json;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class ChapterReorderTests
{
	[Fact]
	public void ReorderChapter_keeps_start_first_and_updates_json_order()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario();
			scenario.AddChapter( new Chapter( "Alpha" ) );
			scenario.AddChapter( new Chapter( "Beta" ) );

			Assert.True( scenario.ReorderChapter( 2, 1 ) );
			Assert.Equal( "Start", scenario.chapterObserver[0].dataName );
			Assert.Equal( "Beta", scenario.chapterObserver[1].dataName );
			Assert.Equal( "Alpha", scenario.chapterObserver[2].dataName );

			var fm = new FileManager( scenario );
			var json = JsonConvert.SerializeObject( fm );
			var roundTripped = JsonConvert.DeserializeObject<FileManager>( json );
			Assert.NotNull( roundTripped );
			Assert.Equal(
				new[] { "Start", "Beta", "Alpha" },
				roundTripped!.chapters.Select( c => c.dataName ).ToArray() );
		} );
	}

	[Fact]
	public void ReorderChapter_rejects_start_and_out_of_range_indices()
	{
		ModelTestEnvironment.RunInSta( () =>
		{
			ModelTestEnvironment.SetupMinimalUtilsForModelTests();

			var scenario = new Scenario();
			scenario.AddChapter( new Chapter( "Alpha" ) );

			Assert.False( scenario.ReorderChapter( 0, 1 ) );
			Assert.False( scenario.ReorderChapter( 1, 0 ) );
			Assert.False( scenario.ReorderChapter( 5, 1 ) );
			Assert.False( scenario.ReorderChapter( 1, 1 ) );
		} );
	}
}
