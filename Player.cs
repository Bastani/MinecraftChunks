using Godot;
using System;
using MinecraftChunks;

[SceneTree]
public partial class Player : CharacterBody3D
{
	public const float Speed = 16.0f;
	public const float JumpVelocity = 10f;
	public const float MouseSensitivity = 0.03f;

	private float _cameraXRotation;

	public Player Instance { get; set; }

	public override void _Ready()
	{
		Instance = this;

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed(GameInputs.Jump) && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var inputDir = Input.GetVector(GameInputs.StrafeLeft, GameInputs.StrafeRight, GameInputs.Forward, GameInputs.Backward);
		var direction = (Head.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk chunk)
		{
			BlockHighlight.Visible = true;

			var blockPosition = RayCast.GetCollisionPoint() - 0.05f * RayCast.GetCollisionNormal();
			var intBlockPosition = new Vector3I(Mathf.FloorToInt(blockPosition.X), Mathf.FloorToInt(blockPosition.Y), Mathf.FloorToInt(blockPosition.Z));
			BlockHighlight.GlobalPosition = intBlockPosition + new Vector3(0.5f, 0.5f, 0.5f);

			if (Input.IsActionJustPressed(GameInputs.BreakBlock))
			{
				chunk.SetBlock((Vector3I)(intBlockPosition - chunk.GlobalPosition), BlockManager.Instance.Air);
			}

			if (Input.IsActionJustPressed(GameInputs.PlaceBlock))
			{
				ChunkManager.Instance.SetBlock((Vector3I)(intBlockPosition + RayCast.GetCollisionNormal()), BlockManager.Instance.Stone);
			}
		}
		else
		{
			BlockHighlight.Visible = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			var deltaX = mouseMotion.Relative.Y * MouseSensitivity;
			var deltaY = mouseMotion.Relative.X * MouseSensitivity;

			Head.RotateY(Mathf.DegToRad(-deltaY));
			if (_cameraXRotation + deltaX > -90 && _cameraXRotation + deltaX < 90)
			{
				Camera.RotateX(Mathf.DegToRad(-deltaX));
				_cameraXRotation += deltaX;
			}
		}
	}
}
