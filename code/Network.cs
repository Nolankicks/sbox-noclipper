using Sandbox;
using Sandbox.Network;

public sealed class Network : Component, Component.INetworkListener
{
	[Property] public GameObject Player { get; set; }
	[Property] public bool StartServer { get; set; } = true;
	protected override void OnAwake()
	{
		if ( FileSystem.Data?.ReadAllText("startserver.txt") is null )
		{
			StartServer = FileSystem.Data?.ReadAllText("startserver.txt")?.ToBool() ?? false;
		}
		else
		{
			StartServer = false;
		}

		
		if (!GameNetworkSystem.IsActive && StartServer)
		{
			GameNetworkSystem.CreateLobby();
		}
		else if (!StartServer)
		{
			var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
			if (spawns.Count == 0)
			{
				var player = Player.Clone(Vector3.Zero);
				player.Name = "Local Player";
			}
			else
			{
				var spawn = Game.Random.FromList(spawns);
				var player = Player.Clone(spawn.Transform.Position);
				player.Name = "Local Player";
			}
		}
	}

	void INetworkListener.OnActive(Sandbox.Connection conn)
	{
		var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
		if (spawns.Count == 0)
		{
			var player = Player.Clone(Vector3.Zero);
			player.Name = conn.DisplayName;
			player.NetworkSpawn(conn);
		}
		else
		{
			var spawn = Game.Random.FromList(spawns);
			var player = Player.Clone(spawn.Transform.Position);
			player.Name = conn.DisplayName;
			player.NetworkSpawn(conn);
		}
	}
}
