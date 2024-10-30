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
			// file = MemoryMappedFile.CreateFromFile(filestr, FileMode.OpenOrCreate, null, 28, MemoryMappedFileAccess.ReadWrite);
			file = MemoryMappedFile.CreateFromFile(filestr, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
			accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
		} catch (Exception e) {
			GD.Print($"Error: {e.Message}");
		}
		floatSize = 4;
		memFileData = new float[7];
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
		var pos = new Vector3(x: memFileData[0], y: memFileData[2], z: memFileData[1]);
		pos.Z = -pos.Z;
		var rot = new Quaternion(x: memFileData[3], y: memFileData[4], z: memFileData[5], w: memFileData[6]).GetEuler(EulerOrder.Xyz);
		rot.Y = -rot.Y;
		Position = pos;
		Rotation = rot;
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
