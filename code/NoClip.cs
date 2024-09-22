using Sandbox;
using Sandbox.Citizen;

public sealed class NoClip : Component
{
	[Sync] Angles EyeAngles { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }
	[Property, Sync] public float MoveSpeed { get; set; } = 1000;
	[Property, Sync] public bool FirstPerson { get; set; } = true;
	[Property, Sync] public int DistanceFromCamera { get; set; } = 200;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public float RunSpeed { get; set; } = 2000;
	[Property, Sync] public bool NoClipping { get; set; } = true;
	[Property] public CharacterController Controller { get; set; }

	public float GetNoClipSpeed()
	{
		if ( Input.Down( "run" ) )
		{
			return RunSpeed;
		}
		else
		{
			return MoveSpeed;
		}
	}
	public float GetSpeed()
	{
		if ( Input.Down( "run" ) )
		{
			return 350;
		}
		else
		{
			return 200;
		}
	}
	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			var firstPersonText = FileSystem.Data.ReadAllText( "firstperson.txt" );

			if ( firstPersonText is null )
			{
				var firstPerson = firstPersonText?.ToBool();
				FirstPerson = firstPerson ?? false;
			}
			else
			{
				FirstPerson = false;
			}

			EyeAngles = Transform.Rotation.Angles();
		}
	}


	protected override void OnUpdate()
	{
		BodyVis();
		if ( !IsProxy )
		{
			Controller.Enabled = !NoClipping;
			BuildEyeAngles();
			Camera();
			if ( Input.Pressed( "3rd/1st Person Toggle" ) )
			{
				FirstPerson = !FirstPerson;
				FileSystem.Data.WriteAllText( "firstperson.txt", FirstPerson.ToString() );
			}
			if ( Input.Pressed( "Toggle Noclip" ) )
			{
				ToggleNoClip();
			}
			if ( Input.Down( "jump" ) )
			{
				Jump();
			}
		}
	}
	float Friction()
	{
		if ( Controller.IsOnGround ) return 6.0f;
		return 0.2f;
	}
	protected override void OnFixedUpdate()
	{
		Anims();
		AnimationHelper.Target.Transform.Rotation = Rotation.Slerp( AnimationHelper.Target.Transform.Rotation, new Angles( 0, EyeAngles.yaw, 0 ).ToRotation(), Time.Delta * 10 );
		if ( !IsProxy )
		{
			if ( !NoClipping )
			{
				Move();
			}
			else
			{
				NoClipMove();
			}
		}
	}
	public void Jump()
	{
		if ( Controller is null ) return;
		if ( Controller.IsOnGround )
		{
			Controller.Punch( Vector3.Up * 300 );
			AnimationHelper.TriggerJump();
		}
	}
	public void Move()
	{
		if ( Controller is null ) return;
		var cc = Controller;
		var halfGrav = Scene.PhysicsWorld.Gravity * 0.5f * Time.Delta;
		WishVelocity = Input.AnalogMove.Normal;
		if ( !WishVelocity.IsNearlyZero() )
		{
			WishVelocity = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation() * Input.AnalogMove.Normal;
			WishVelocity *= GetSpeed();
			WishVelocity = WishVelocity.WithZ( 0 );
		}
		if ( !cc.IsOnGround )
		{
			WishVelocity.ClampLength( 50 );
		}
		cc.ApplyFriction( Friction() );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = Controller.Velocity.WithZ( 0 );
		}
		else
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity += halfGrav;
		}
		cc.Move();
		if ( !cc.IsOnGround )
		{

			cc.Velocity += halfGrav;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}

	}
	public void BuildEyeAngles()
	{
		var ee = EyeAngles;
		ee += Input.AnalogLook;
		ee.pitch = ee.pitch.Clamp( -89, 89 );
		ee.roll = 0;
		EyeAngles = ee;
	}
	void ToggleNoClip()
	{
		NoClipping = !NoClipping;
		WishVelocity = Vector3.Zero;
	}
	public void NoClipMove()
	{
		WishVelocity = new Angles( EyeAngles.pitch, EyeAngles.yaw, 0 ).ToRotation() * Input.AnalogMove.Normal;
		WishVelocity *= GetNoClipSpeed();
		if ( !WishVelocity.IsNearlyZero() )
		{
			Transform.Position += WishVelocity * Time.Delta;
		}
	}

	public void Camera()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => x.IsMainCamera );
		camera.FieldOfView = Preferences.FieldOfView;
		var lookDirection = EyeAngles.ToRotation();
		var center = Transform.Position + Vector3.Up * 64;
		//Trace to see if the camera is inside a wall
		if ( !FirstPerson )
		{
			camera.Transform.Position = center + lookDirection.Backward * DistanceFromCamera;
		}
		else
		{
			var targetPos = Transform.Position + Vector3.Up * 64;
			camera.Transform.Position = targetPos;
		}

		camera.Transform.Rotation = lookDirection;

	}

	public void BodyVis()
	{
		var target = AnimationHelper.Target;
		if ( FirstPerson )
		{
			var bodyVis = IsProxy ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
			target.RenderType = bodyVis;
			foreach ( var child in target.Components.GetAll<SkinnedModelRenderer>( FindMode.InDescendants ) )
			{
				child.RenderType = bodyVis;
			}
		}
		else
		{
			target.RenderType = ModelRenderer.ShadowRenderType.On;
			foreach ( var child in target.Components.GetAll<SkinnedModelRenderer>( FindMode.InDescendants ) )
			{
				child.RenderType = ModelRenderer.ShadowRenderType.On;
			}
		}
	}

	public void Anims()
	{
		if ( NoClipping )
		{
			AnimationHelper.WithVelocity( WishVelocity );
		}
		else
		{
			AnimationHelper.WithVelocity( Controller.Velocity );
			AnimationHelper.IsGrounded = Controller.IsOnGround;
		}
		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.IsNoclipping = NoClipping;
	}
}
