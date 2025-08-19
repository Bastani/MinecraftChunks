using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ChunkManager : Node
{
	private Dictionary<Chunk, Vector2I> _chunkToPosition = new();
	private Dictionary<Vector2I, Chunk> _positionToChunk = new();
	private List<Chunk> _chunks;

	public static ChunkManager Instance { get; set; }

	public override void _Ready()
	{
		Instance = this;

		_chunks = GetParent().GetChildren().OfType<Chunk>().ToList();
	}

	public void UpdateChunkPosition(Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
	{
		if (_positionToChunk.TryGetValue(previousPosition, out var chunkPosition) && chunkPosition == chunk)
		{
			_positionToChunk.Remove(previousPosition);
		}

		_chunkToPosition[chunk] = currentPosition;
		_positionToChunk[currentPosition] = chunk;
	}

	public void SetBlock(Vector3I globalPosition, Block block)
	{
		var chunkTilePosition = new Vector2I(Mathf.FloorToInt(globalPosition.X / (float)Chunk.Dimensions.X), Mathf.FloorToInt(globalPosition.Z / (float)Chunk.Dimensions.Z));;
		if (_positionToChunk.TryGetValue(chunkTilePosition, out var chunk))
		{
			chunk.SetBlock((Vector3I)(globalPosition - chunk.GlobalPosition), block);
		}
	}
}
