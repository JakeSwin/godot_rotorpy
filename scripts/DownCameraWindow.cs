using Godot;
using System;

using NetMQ;
using NetMQ.Sockets;
using System.Drawing;
using System.IO;

public partial class DownCameraWindow : Window
{
	private PublisherSocket sender;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sender = new PublisherSocket();
		sender.Bind("tcp://*:5557");
	}
	
	public void _on_pub_image_timer_timeout() 
	{
		byte[] imageBytes = GetViewport().GetTexture().GetImage().SaveJpgToBuffer();
		var message = new NetMQMessage();
		message.Append("image");
		message.Append(imageBytes);
		sender.SendMultipartMessage(message);
	}
}
