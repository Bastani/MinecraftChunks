using Godot;
using System;

[Tool]
[SceneTree]
public partial class Chunk : StaticBody3D
{
	public static Vector3I dimensions = new Vector3I(16, 64, 16);
	private static readonly Vector3I[] _vertices = new Vector3I[]
	{
		new Vector3I(0, 0, 0),
		new Vector3I(1, 0, 0),
		new Vector3I(0, 1, 0),
		new Vector3I(1, 1, 0),
		new Vector3I(0, 0, 1),
		new Vector3I(1, 0, 1),
		new Vector3I(0, 1, 1),
		new Vector3I(1, 1, 1)
	};

	private static readonly int[] _top = new int[] { 2, 3, 7, 6 };
	private static readonly int[] _bottom = new int[] { 0, 4, 5, 1 };
	private static readonly int[] _left = new int[] { 6, 4, 0, 2 };
	private static readonly int[] _right = new int[] { 3, 1, 5, 7};
	private static readonly int[] _back = new int[] { 7, 5, 4, 6 };
	private static readonly int[] _front = new int[] { 2, 0, 1, 3 };

	private SurfaceTool _surfaceTool = new();

	private Block[,,] _blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z];

	public override void _Ready()
	{
		Generate();
		Update();
	}

	public void Generate()
	{
		var block = new Block();

		for (int x = 0; x < dimensions.X; x++)
		{
			for (int y = 0; y < dimensions.Y; y++)
			{
				for (int z = 0; z < dimensions.Z; z++)
				{
					_blocks[x, y, z] = block;
				}
			}
		}
	}

	public void Update()
	{
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		for (int x = 0; x < dimensions.X; x++)
		{
			for (int y = 0; y < dimensions.Y; y++)
			{
				for (int z = 0; z < dimensions.Z; z++)
				{
					CreateBlockMesh(new Vector3I(x, y, z));
				}
			}
		}

		var mesh = _surfaceTool.Commit();

		MeshInstance3D.Mesh = mesh;
		CollisionShape3D.Shape = mesh.CreateTrimeshShape();
	}

	private void CreateBlockMesh(Vector3I blockPosition)
	{
		if (CheckTransparent(blockPosition + Vector3I.Up))
		{
			CreateFaceMesh(_top, blockPosition);
		}
		if (CheckTransparent(blockPosition + Vector3I.Down))
		{
			CreateFaceMesh(_bottom, blockPosition);
		}
		if (CheckTransparent(blockPosition + Vector3I.Left))
		{
			CreateFaceMesh(_left, blockPosition);
		}
		if (CheckTransparent(blockPosition + Vector3I.Right))
		{
			CreateFaceMesh(_right, blockPosition);
		}
		if (CheckTransparent(blockPosition + Vector3I.Forward))
		{
			CreateFaceMesh(_front, blockPosition);
		}
		if (CheckTransparent(blockPosition + Vector3I.Back))
		{
			CreateFaceMesh(_back, blockPosition);
		}
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition)
	{
		var a = _vertices[face[0]] + blockPosition;
		var b = _vertices[face[1]] + blockPosition;
		var c = _vertices[face[2]] + blockPosition;
		var d = _vertices[face[3]] + blockPosition;

		var triangle1 = new Vector3[] { a, b, c };
		var triangle2 = new Vector3[] { a, c, d };

		_surfaceTool.AddTriangleFan(triangle1);
		_surfaceTool.AddTriangleFan(triangle2);
	}

	private bool CheckTransparent(Vector3I blockPosition)
	{
		return true;
	}
}
