using Godot;
using System;

using NetMQ;
using NetMQ.Sockets;
using System.Drawing;
using System.IO;

public partial class DownCameraWindow : Window
{
	private ResponseSocket receiver;
	private Node3D uav;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		receiver = new ResponseSocket();
		receiver.Options.ReceiveHighWatermark = 1;
		receiver.Bind("tcp://*:5557");
		
		uav = GetNode<Node3D>("Uav");
	}
	
	public byte[] Vector3ToBytes(Vector3 vector)
	{
		byte[] bytes = new byte[sizeof(float) * 3];
		
		Buffer.BlockCopy(BitConverter.GetBytes(vector.X), 0, bytes, 0 * sizeof(float), sizeof(float));
		Buffer.BlockCopy(BitConverter.GetBytes(vector.Y), 0, bytes, 1 * sizeof(float), sizeof(float));
		Buffer.BlockCopy(BitConverter.GetBytes(vector.Z), 0, bytes, 2 * sizeof(float), sizeof(float));
		
		return bytes;
	}

	public override void _Process(double delta)
	{
		string empty_string;
		if (receiver.TryReceiveFrameString(TimeSpan.Zero, out empty_string))
		{
			byte[] imageBytes = GetViewport().GetTexture().GetImage().SaveJpgToBuffer();
			var message = new NetMQMessage();
			message.Append("image");
			message.Append(imageBytes);
			message.Append("pos");
			message.Append(Vector3ToBytes(uav.GlobalPosition));
			receiver.SendMultipartMessage(message);
		}
	}
}
