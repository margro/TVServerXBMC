using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

namespace TVServerKodi.Commands
{
    class DataWriter
    {
        readonly StreamWriter writer;
        readonly BinaryWriter binWriter;

        // will always end with a newline, so this is anything before that
        private String commandSeparator = Environment.NewLine;
        private String argumentSeparator = ";";
        private String listSeparator = ",";
        private Converter<string, string> argumentEncoder = new Converter<string, string>(Uri.EscapeDataString);
        private Converter<string, string> listEncoder = new Converter<string, string>(Uri.EscapeDataString);
        private bool escapeData = true;

        public DataWriter(NetworkStream stream)
        {
            writer = new StreamWriter(stream);
            binWriter = new BinaryWriter(stream);
        }

        private String getSameString(String str)
        {
            return str;
        }

        public void setHumanEncoders()
        {
            commandSeparator = Environment.NewLine +  " ---- End ----" + Environment.NewLine + "> ";
            //argumentSeparator = ",";
            //listSeparator = Environment.NewLine;
            argumentEncoder = new Converter<string, string>(getSameString);
            listEncoder = new Converter<string, string>(getSameString);
            escapeData = false;
        }

        public void setArgumentSeparator(String argSep)
        {
            if (argSep.Length > 0)
                argumentSeparator = argSep;
        }

        public void setListSeparator(String listSep)
        {
            if (listSep.Length > 0)
                listSeparator = listSep;
        }

        public String makeItemSmart(params Object[] arguments)
        {
            List<String> allArguments = new List<String>();


            foreach (Object o in arguments)
            {
                if (o == null)
                {
                    allArguments.Add("");
                }
                else if (o is String)
                {
                    allArguments.Add(o.ToString());
                }
                else if (o is ChannelInfo)
                {
                    ChannelInfo c = (ChannelInfo)o;
                    allArguments.Add(c.channelID);
                    allArguments.Add(c.name);
                    if(c.epgNow != null){
                        allArguments.Add(c.epgNow.description);
                        allArguments.Add(c.epgNow.timeInfo);
                    } else {
                        allArguments.Add("");
                        allArguments.Add("");
                    }
                    if (c.epgNext != null)
                    {
                        allArguments.Add(c.epgNext.description);
                        allArguments.Add(c.epgNext.timeInfo);
                    } else {
                        allArguments.Add("");
                        allArguments.Add("");
                    }
                    allArguments.Add(c.isScrambled.ToString());
                }
            }

            return makeItem(allArguments.ToArray());
        }

        public String makeItem(params String[] arguments)
        {
            try
            {
                ensureNoNull(arguments, "");

                // escape all the arguments
                String[] escaped = Array.ConvertAll<string, string>(arguments, argumentEncoder);

                // join them up
                return string.Join(argumentSeparator, arguments);
            }
            catch
            {
                return "";
            }
        }

        // writes a command out, ensure that it is written out
        public void write(String line)
        {
            try
            {
                writer.Write(line + commandSeparator);
                Console.WriteLine("Socket write: " + line + commandSeparator);
                writer.Flush();
            }
            catch
            { }
        }
        public void write(String line, bool escapeMe)
        {
            if (escapeData && escapeMe)
                write(Uri.EscapeDataString(line));
            else
                write(line);
        }


        public void writeBytes(Byte[] bytes)
        {
            try
            {
                binWriter.Write(bytes);
                binWriter.Flush();
            }
            catch
            { }
        }
        
        public void writeList(List<string> list)
        {
            try
            {
                ensureNoNull(list, "");

                // escape every value
                List<string> escaped = list.ConvertAll<string>(listEncoder);

                // send it as one line
                write(String.Join(listSeparator, escaped.ToArray()));
            }
            catch
            { }
        }



        // Ensure that no null strings exist, replace them with ""
        private void ensureNoNull<T>(T[] array, T def)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    array[i] = def;
                }
            }
        }

        private void ensureNoNull<T>(List<T> list, T def)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    list[i] = def;
                }
            }
        }

    }
}
