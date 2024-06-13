using Sandbox;
using Sandbox.Network;

public sealed class Clothing : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer Body { get; set; }

	public void OnNetworkSpawn(Connection conn)
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize(conn.GetUserData("avatar"));
		clothing.Apply(Body);
	}

	protected override void OnStart()
	{
		if (!GameNetworkSystem.IsActive)
		{
			var clothing = ClothingContainer.CreateFromLocalUser();
			clothing.Apply(Body);
		}
	}
}
