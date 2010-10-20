using System;
using System.IO;
using System.Text;

namespace OpenC1.Parsers
{
    //
	// Ported from the c1 & c1 demo file decoders by Shayde (shayde@ihug.co.nz), thanks!
    //
	static class TextFileDecryptor
	{

		static byte[] DemoTable = new byte[] 
		{
			0x58, 0x50, 0x3A, 0x76, 0xCB, 0xB6, 0x85, 0x65,
			0x15, 0xCD, 0x5B, 0x07, 0xB1, 0x68, 0xDE, 0x3A
		};

		static byte[] Table1 = 
		{
			0x6C, 0x1B, 0x99, 0x5F, 0xB9, 0xCD, 0x5F, 0x13,
			0xCB, 0x04, 0x20, 0x0E, 0x5E, 0x1C, 0xA1, 0x0E
		};

		static byte[] Table2 = 
		{
			0x67, 0xA8, 0xD6, 0x26, 0xB6, 0xDD, 0x45, 0x1B,
			0x32, 0x7E, 0x22, 0x13, 0x15, 0xC2, 0x94, 0x37
		};

		static byte SPACEBYTE = (byte)' ';
		static byte ENCRYPTED_LINE_START = (byte)'@';


		public static byte[] DecryptDemoFile(string filename)
		{
			byte[] data = File.ReadAllBytes(filename);
			MemoryStream ms = new MemoryStream(data.Length);

			int filePos = 0;

			while (true)
			{
				int strLen = ReadLine(data, filePos);
				
				if (strLen == 0) break;

				if (data[filePos] == ENCRYPTED_LINE_START)
				{
					int i, l = strLen - 2, c = l % 16, decoded = 0;

					for (i = 1; i <= l; i++)
					{
						int pos = filePos + i;

						if (data[pos] == 9)
							data[pos] = 0x9f;
						decoded = (((data[pos] - SPACEBYTE) ^ DemoTable[c]) & 0x7f) + SPACEBYTE;
						data[pos] = (byte)decoded;
						if (data[pos] == 0x9f)
							data[pos] = 9;
						c = (c + 7) % 16;
					}
					ms.Write(data, filePos + 1, strLen);
				}
				filePos += strLen + 1;
			}
			return ms.ToArray();
		}


		public static byte[] DecryptFile(string filename)
		{	
			byte[] data = File.ReadAllBytes(filename);
			MemoryStream ms = new MemoryStream(data.Length);

			int filePos = 0;

			while (true)
			{
				int strLen = ReadLine(data, filePos);

				if (strLen == 0) break;

				if (data[filePos] == ENCRYPTED_LINE_START)
				{
					int i, l = strLen - 2, c = l % 16, decoded = 0;
					bool inComment = false;

					for (i = 1; i <= l; i++)
					{
						int pos = filePos + i;

						if (data[pos] == 9)
							data[pos] = 0x9f;
						if (i >= 2 && data[pos - 2] == '/' && data[pos - 1] == '/')
							inComment = true;
						if (inComment)
							decoded = (((data[pos] - SPACEBYTE) ^ Table2[c]) & 0x7f) + SPACEBYTE;
						else
							decoded = (((data[pos] - SPACEBYTE) ^ Table1[c]) & 0x7f) + SPACEBYTE;
						data[pos] = (byte)decoded;
						if (data[pos] == 0x9f)
							data[pos] = 9;
						c = (c + 7) % 16;
					}
					ms.Write(data, filePos + 1, strLen);
				}
				else
				{
					ms.Write(data, filePos, strLen);
				}
				filePos += strLen + 1;
			}
			return ms.ToArray();
		}



		// returns length of string, including crlf
		private static int ReadLine(byte[] fileData, int pos)
		{
			int i = pos;
			while (true)
			{
				if (i == fileData.Length) return 0;
				if (fileData[i] == '\r' && fileData[i+1] == '\n')
					return (i + 1) - pos;

				i++;
			}
		}
	}
}
