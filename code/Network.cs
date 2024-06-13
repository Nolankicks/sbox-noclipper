using Sandbox;
using Sandbox.Network;

public sealed class Network : Component, Component.INetworkListener
{
	[Property] public GameObject Player { get; set; }
	protected override void OnAwake()
	{
		if (!GameNetworkSystem.IsActive)
		{
			GameNetworkSystem.CreateLobby();
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
