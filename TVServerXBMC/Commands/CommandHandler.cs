using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Commands
{
    abstract class CommandHandler
    {
        private ConnectionHandler connection;

        public CommandHandler(ConnectionHandler connection)
        {
            this.connection = connection;
        }

        public ConnectionHandler getConnection()
        {
            return this.connection;
        }

        public DataWriter writer 
        {
            get {
                return this.connection.getWriter();
            }
        }

        public abstract void handleCommand(String command, String[] arguments, ref TvControl.IUser me);
        public abstract String getCommandToHandle();
    }
}
