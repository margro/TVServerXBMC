using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TVServerKodi;

namespace TVServerKodi.Commands
{
    class GetChannelThumb : CommandHandler
    {
        private readonly string[] strThumbExt = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
      
        public GetChannelThumb(ConnectionHandler connection) : base(connection)
        {

        }

        private string ToThumbFileName(string strName)
        {
          string strThumbName = strName;

          strThumbName = strThumbName.Replace(":", "_");
          strThumbName = strThumbName.Replace("/", "_");
          strThumbName = strThumbName.Replace("\\", "_");

          return strThumbName;
        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {

            if ((arguments != null) && (arguments.Length == 2))
            {
              try
              {
                string strChannelName = arguments[0];
                Boolean bIsRadio = Boolean.Parse(arguments[1]);
                string strThumbName = "False";
                Boolean bFound = false;

                string strThumbPath = TVServerConnection.GetLogoPath(bIsRadio);

                string strThumbBaseName = ToThumbFileName(strChannelName);

                foreach (string strExt in strThumbExt)
                {
                  strThumbName = strThumbBaseName + strExt;
                  if (File.Exists(strThumbPath + strThumbName))
                  {
                    bFound = true;
                    break;
                  }
                }

                if (!bFound)
                {
                  writer.write("0|False");
                }
                else
                {
                  Byte[] thumbData = File.ReadAllBytes(strThumbPath + strThumbName);
                  Int32 fileLength = thumbData.Length;
                  Int32 thumbNameLength = strThumbName.Length;
                  Byte[] utf8ThumbName = System.Text.Encoding.UTF8.GetBytes(strThumbName);
                  Byte[] thumbNameLengthBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(utf8ThumbName.Length));
                  Byte[] fileLengthBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(fileLength));

                  Byte[] strThumbInfo = { 0x31, 0x7C }; // ASCII: "1|"

                  writer.writeBytes(strThumbInfo);
                  writer.writeBytes(thumbNameLengthBytes);
                  writer.writeBytes(utf8ThumbName);
                  writer.writeBytes(fileLengthBytes);
                  writer.writeBytes(thumbData);

                  Byte[] strEnd = { 0x7C, 0x45, 0x4E, 0x44 }; // ASCII: "|END"
                  writer.writeBytes(strEnd);

                }
              }
              catch
              {
                writer.write("0|Error");
              }
            }
            else
            {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":ChannelName|IsRadio");
            }
        }

        public override string getCommandToHandle()
        {
            return "GetChannelThumb";
        }
    }
}
