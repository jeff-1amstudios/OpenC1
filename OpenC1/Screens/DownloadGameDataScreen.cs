using System;
using System.Collections.Generic;
using System.Text;
using OneAmEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenC1.Parsers;
using System.IO;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Net;
using Ionic.Zip;

namespace OpenC1.Screens
{
	class DownloadGameDataScreen : BaseMenuScreen
	{

		Thread _downloadThread;
		long _dataContentLength, _dataDownloaded;
		bool _downloadError;
		bool _unpacking;

		public DownloadGameDataScreen(BaseMenuScreen parent)
			: base(parent)
		{

			_downloadThread = new Thread(DownloadDataThreadProc);
			_downloadThread.Priority = ThreadPriority.AboveNormal;
			_downloadThread.Start();
		}

		public override void Render()
		{
			base.Render();

			Engine.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

			RenderDefaultBackground();

			WriteTitleLine("Downloading demo content...");

			if (_dataDownloaded > 0)
			{
				Engine.SpriteBatch.DrawString(_font, "[", new Vector2(30, 150), Color.White);
				int ratio = (int)(((double)_dataDownloaded / (double)_dataContentLength) * 40);
				Engine.SpriteBatch.DrawString(_font, new string('|', ratio), new Vector2(45, 150), Color.White);
				Engine.SpriteBatch.DrawString(_font, "]", new Vector2(750, 150), Color.White);

				long downloadedMb = _dataDownloaded / 1024 / 1024;
				long contentLengthMb = _dataContentLength / 1024 / 1024;
				WriteLine(String.Format("Downloaded {0}mb / {1}mb", downloadedMb, contentLengthMb), 200);
			}

			if (_unpacking)
			{
				WriteLine("Unpacking into GameData folder...", 270);
			}

			if (_downloadError)
			{
				WriteLine("An error occured while downloading.", 270);
				WriteLine("Please check OpenC1.log for more details.");
				WriteLine("");
				WriteLine("Press Enter to exit.");
			}
			
			Engine.SpriteBatch.End();
		}

		private void DownloadDataThreadProc()
		{
			try
			{
				string url = "http://www.1amstudios.com/download/carmageddon_demo_data.zip";
				//url = "http://127.0.0.1/carmageddon_demo_data.zip";

				Logger.Log("Downloading demo content from " + url);
				WebRequest request = WebRequest.Create(url);
				var response = request.GetResponse();
				_dataContentLength = long.Parse(response.Headers["Content-Length"]);

				byte[] buffer = new byte[4096];
				string tempFileName = Path.GetTempFileName();
				Stream fileStream = File.Open(tempFileName, FileMode.Create);
				using (Stream s = response.GetResponseStream())
				{
					while (true)
					{
						int read = s.Read(buffer, 0, 4096);
						_dataDownloaded += read;
						if (read == 0)
							break;
						fileStream.Write(buffer, 0, read);
					}
				}
				fileStream.Close();
				_unpacking = true;

				var zipFile = ZipFile.Read(tempFileName);
				Directory.CreateDirectory("GameData");
				zipFile.ExtractAll("GameData");
				zipFile.Dispose();
				File.Delete(tempFileName);
			}
			catch (Exception ex)
			{
				_downloadError = true;
				Logger.Log(ex.ToString());
			}
		}

		public override void Update()
		{
			base.Update();

			if (_downloadThread.Join(1) && !_downloadError)
			{
				Engine.Screen = new GameSelectionScreen(null);
			}
		}

		public override void OnOutAnimationFinished()
		{
			if (_downloadError)
				Engine.Game.Exit();
		}
	}
}
