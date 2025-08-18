using Godot;
using System;
using MinecraftChunks;

[Tool]
[SceneTree]
public partial class Chunk : StaticBody3D
{
	public static Vector3I Dimensions = new(16, 64, 16);
	private static readonly Vector3I[] _vertices =
	[
		new(0, 0, 0),
		new(1, 0, 0),
		new(0, 1, 0),
		new(1, 1, 0),
		new(0, 0, 1),
		new(1, 0, 1),
		new(0, 1, 1),
		new(1, 1, 1)
	];

	private static readonly int[] _top = [2, 3, 7, 6];
	private static readonly int[] _bottom = [0, 4, 5, 1];
	private static readonly int[] _left = [6, 4, 0, 2];
	private static readonly int[] _right = [3, 1, 5, 7];
	private static readonly int[] _back = [7, 5, 4, 6];
	private static readonly int[] _front = [2, 0, 1, 3];

	private SurfaceTool _surfaceTool = new();
	private Block[,,] _blocks = new Block[Dimensions.X, Dimensions.Y, Dimensions.Z];
	public Vector2I ChunkPosition { get; private set; }

	[Export]
	public FastNoiseLite Noise { get; set; }

	public override void _Ready()
	{
		ChunkPosition = new Vector2I(Mathf.FloorToInt(GlobalPosition.X / Dimensions.X), Mathf.FloorToInt(GlobalPosition.Z / Dimensions.Z));

		Generate();
		Update();
	}

	public void Generate()
	{
		for (var x = 0; x < Dimensions.X; x++)
		{
			for (var y = 0; y < Dimensions.Y; y++)
			{
				for (var z = 0; z < Dimensions.Z; z++)
				{
					Block block;

					var globalBlockPosition = ChunkPosition * new Vector2I(Dimensions.X, Dimensions.Z) + new Vector2I(x, z);;
					var groundHeight = (int)(Dimensions.Y * (Noise.GetNoise2D(globalBlockPosition.X, globalBlockPosition.Y) + 1f) / 2f);

					if (y < groundHeight / 2)
					{
						block = BlockManager.Instance.Stone;
					}
					else if (y < groundHeight)
					{
						block = BlockManager.Instance.Dirt;
					}
					else if (y == groundHeight)
					{
						block = BlockManager.Instance.Grass;
					}
					else
					{
						block = BlockManager.Instance.Air;
					}

					_blocks[x, y, z] = block;
				}
			}
		}
	}

	public void Update()
	{
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		for (var x = 0; x < Dimensions.X; x++)
		{
			for (var y = 0; y < Dimensions.Y; y++)
			{
				for (var z = 0; z < Dimensions.Z; z++)
				{
					CreateBlockMesh(new Vector3I(x, y, z));
				}
			}
		}

		_surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);
		var mesh = _surfaceTool.Commit();

		MeshInstance3D.Mesh = mesh;
		CollisionShape3D.Shape = mesh.CreateTrimeshShape();
	}

	private void CreateBlockMesh(Vector3I blockPosition)
	{
		var block = _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z];

		if(block == BlockManager.Instance.Air) return;

		if (CheckTransparent(blockPosition + Vector3I.Up))
		{
			CreateFaceMesh(_top, blockPosition, block.TopTexture ?? block.Texture );
		}
		if (CheckTransparent(blockPosition + Vector3I.Down))
		{
			CreateFaceMesh(_bottom, blockPosition, block.BottomTexture ?? block.Texture);
		}
		if (CheckTransparent(blockPosition + Vector3I.Left))
		{
			CreateFaceMesh(_left, blockPosition, block.Texture);
		}
		if (CheckTransparent(blockPosition + Vector3I.Right))
		{
			CreateFaceMesh(_right, blockPosition, block.Texture);
		}
		if (CheckTransparent(blockPosition + Vector3I.Forward))
		{
			CreateFaceMesh(_front, blockPosition, block.Texture);
		}
		if (CheckTransparent(blockPosition + Vector3I.Back))
		{
			CreateFaceMesh(_back, blockPosition, block.Texture);
		}
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition, Texture2D texture)
	{
		var texturePosition = BlockManager.Instance.GetTextureAtlasPosition(texture);
		var textureAtlasSize = BlockManager.Instance.TextureAtlasSize;

		var uvOffset = texturePosition / textureAtlasSize;
		var uvWidth = 1f / textureAtlasSize.X;
		var uvHeight = 1f / textureAtlasSize.Y;

		var uvA = uvOffset + new Vector2(0, 0);
		var uvB = uvOffset + new Vector2(0, uvHeight);
		var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
		var uvD = uvOffset + new Vector2(uvWidth, 0);

		var a = _vertices[face[0]] + blockPosition;
		var b = _vertices[face[1]] + blockPosition;
		var c = _vertices[face[2]] + blockPosition;
		var d = _vertices[face[3]] + blockPosition;

		var uvTriangle1 = new[] {uvA, uvB, uvC};
		var uvTriangle2 = new[] {uvA, uvC, uvD};

		var triangle1 = new Vector3[] { a, b, c };
		var triangle2 = new Vector3[] { a, c, d };

		_surfaceTool.AddTriangleFan(triangle1, uvTriangle1);
		_surfaceTool.AddTriangleFan(triangle2, uvTriangle2);
	}

	private bool CheckTransparent(Vector3I blockPosition)
	{
		if (blockPosition.X < 0 || blockPosition.X >= Dimensions.X) return true;
		if (blockPosition.Y < 0 || blockPosition.Y >= Dimensions.Y) return true;
		if (blockPosition.Z < 0 || blockPosition.Z >= Dimensions.Z) return true;

		return _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] == BlockManager.Instance.Air;
	}
}
