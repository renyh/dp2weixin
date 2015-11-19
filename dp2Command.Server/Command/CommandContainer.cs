using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Server
{
    public class CommandContainer
    {
        public Dictionary<string, BaseCommand> CmdDict = null;

        public CommandContainer()
        {
            CmdDict = new Dictionary<string, BaseCommand>();
        }

        public BaseCommand GetCommand(string cmdName)
        {
            if (CmdDict.ContainsKey(cmdName))
            {
                return CmdDict[cmdName];
            }

            // todo这里改为具体的类型
            BaseCommand command = null;
            if (cmdName == dp2CommandUtility.C_Command_Search)
            {
                command = new SearchCommand();
            }
            else
            {
                command = new BaseCommand();
            }
            command.CommandName = cmdName;
            CmdDict[cmdName] = command;
            return command;
        }

    }
}
