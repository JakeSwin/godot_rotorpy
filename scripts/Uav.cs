using Godot;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;

public partial class Uav : Node3D
{
	private MemoryMappedFile file;
	private MemoryMappedViewAccessor accessor;

	private float[] memFileData;

	private int floatSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		try {
			var filestr = new FileStream("uavdata.bin", FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
			file = MemoryMappedFile.CreateFromFile(filestr, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
			accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
		} catch (Exception e) {
			GD.Print($"Error: {e.Message}");
		}
		floatSize = 4;
		memFileData = new float[7];
		var new_window = new Window();
		AddChild(new_window);

		new_window.Size = new Vector2I(800, 600);
		new_window.Position = new Vector2I(100, 100);
		new_window.Visible = true;

		// var camera = GetChild<Camera3D>();
	}

	private void ResetMemFile()
	{
		float toWrite = -1.0f;
		for (long i = 0; i < 28; i += floatSize)
		{
			accessor.Write(i, ref toWrite);
		}
	}

	private void ReadMemFile()
	{
		int idx = 0;
		for (long i = 0; i < 28; i += floatSize)
		{
			accessor.Read(i, out memFileData[idx]);
			idx++;
		}
	}

	private void PrintMem()
	{
		foreach (float elem in memFileData)
		{
			GD.Print(elem);
		}
	}

	private void SetPosRot()
	{
		Position = new Vector3(x: memFileData[0], y: memFileData[2], z: -memFileData[1]);
		Quaternion = new Quaternion(x: memFileData[3], y: memFileData[5], z: -memFileData[4], w: memFileData[6]);
	}

	private bool NoNewMemData()
	{
		return memFileData.SequenceEqual(new float[] { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f });
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ReadMemFile();
		if (!NoNewMemData()) {
			SetPosRot();
			ResetMemFile();
		} 
	}

	public override void _ExitTree()
	{
		file.Dispose();
		accessor.Dispose();
		base._ExitTree();
	}
}
