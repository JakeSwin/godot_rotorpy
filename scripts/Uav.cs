using Godot;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;

using NetMQ;
using NetMQ.Sockets;
using System.Data.SqlTypes;

public partial class Uav : Node3D
{
	private MemoryMappedFile file;
	private MemoryMappedViewAccessor accessor;

	private float[] memFileData;

	private int floatSize;

	private ResponseSocket receiver;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		try {
			receiver = new ResponseSocket();
			receiver.Options.ReceiveHighWatermark = 1;
			receiver.Bind("tcp://*:5556");
			// receiver.Bind("ipc:///roterpy.ipc");
		} catch (Exception e) {
			GD.Print($"Error: {e.Message}");
		}
		floatSize = 4;
		memFileData = new float[7];
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

	private float[] ByteArrToFloatArr(byte[] arr) {
		float[] floatarr = new float[arr.Length / 4];
		Buffer.BlockCopy(arr, 0, floatarr, 0, arr.Length);
		return floatarr;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var bytes = new byte[28];
		if (receiver.TryReceiveFrameBytes(TimeSpan.Zero, out bytes)) {
			memFileData = ByteArrToFloatArr(bytes);
			SetPosRot();
			receiver.TrySendFrameEmpty();
		}
	}

	public override void _ExitTree()
	{
		receiver.Dispose();
		base._ExitTree();
	}
}
