namespace MinecraftChunks;

using Godot;
using Godot.Collections;

public class BlockManager
{
	[Export] public Block Air { get; set; }
	[Export] public Block Stone { get; set; }
	[Export] public Block Dirt { get; set; }
	[Export] public Block Grass { get; set; }

	private readonly Dictionary<Texture2D, Vector2I> _atlasLookup = new();

	private int _gridWidth = 4;
	private int _gridHeight;

	public Vector2I BlockTextureSize { get; } = new(16, 16);
	public Vector2 TextureAtlasSize { get; private set; }
	public static BlockManager Instance { get; private set; }
	public StandardMaterial3D ChunkMaterial { get; private set; }
}