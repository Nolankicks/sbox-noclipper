using System.Formats.Tar;
using Sandbox;

public sealed class MapChanger : Component
{
	[Property] public MapInstance MapInstance { get; set; }
	protected override void OnEnabled()
	{
		MapInstance.OnMapLoaded += HandelMap;
	}

	protected override void OnDisabled()
	{
		MapInstance.OnMapLoaded -= HandelMap;
	}
	[Broadcast]
	public void LoadMap(string Indent)
	{
		MapInstance.MapName = Indent;
	}

	[Broadcast]
	public void HandelMap()
	{
		var playerList = Scene.GetAllComponents<NoClip>().ToList();
		foreach (var player in playerList)
		{
			var randomSpawnPoint = Game.Random.FromList(Scene.GetAllComponents<SpawnPoint>().ToList());
			player.Transform.Position = randomSpawnPoint.Transform.Position;
		}
	}
}
