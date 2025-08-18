using Godot;
using System;

[Tool]
public partial class Block : Resource
{
	[Export] public Texture2D Texture { get; set; }
}
