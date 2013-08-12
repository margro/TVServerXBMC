using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TVServerXBMC;

namespace TVServerXBMC.Commands
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

          strThumbName.Replace(":", "_");
          strThumbName.Replace("/", "_");
          strThumbName.Replace("\\", "_");

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
                  Int64 fileLength = thumbData.LongLength;
                  //string strFileLength = fileLength.ToString();

                  string strThumbInfo = "1|"
                    + strThumbName + "|"
                    + fileLength.ToString();

                  writer.write(strThumbInfo);
                  writer.writeBytes(thumbData);
                  
                  writer.write("|END");

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
