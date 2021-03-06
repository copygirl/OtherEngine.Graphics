﻿using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OtherEngine.Core;
using OtherEngine.Core.Attributes;
using OtherEngine.Core.Events;
using OtherEngine.Core.Tracking;
using Imaging = System.Drawing.Imaging;

namespace OtherEngine.Graphics.OpenGL
{
	[AutoEnable]
	public class TextureController : Controller
	{
		[TrackComponent]
		public EntityCollection<TextureComponent> Textures { get; private set; }


		public EntityRef<TextureComponent, GLHandleComponent> CurrentTexture { get; private set; }


		public EntityRef<TextureComponent, GLHandleComponent> Load(string file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			
			var bmp = new Bitmap(file);
			var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
				Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb);

			var handle = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

			GL.TexImage2D(TextureTarget.Texture2D, 0,
				PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			GL.BindTexture(TextureTarget.Texture2D, CurrentTexture?.Second.Value ?? 0);
			bmp.UnlockBits(data);

			return new Entity(Game).AddTypeRef(
				new TextureComponent(),
				new GLHandleComponent(handle));
		}

		public void Bind(EntityRef<TextureComponent, GLHandleComponent> texture)
		{
			GL.BindTexture(TextureTarget.Texture2D, texture.Second.Value);
			CurrentTexture = texture;
		}

		public void Unbind()
		{
			if (CurrentTexture == null)
				throw new InvalidOperationException("No texture currently bound");

			GL.BindTexture(TextureTarget.Texture2D, 0);
			CurrentTexture = null;
		}

		public void Delete(EntityRef<TextureComponent, GLHandleComponent> texture)
		{
			GL.DeleteTexture(texture.Second.Value);
		}


		[SubscribeEvent]
		void OnTextureComponentRemoved(ComponentRemovedEvent<TextureComponent> ev)
		{
			Delete(ev.Entity);
		}
	}
}

