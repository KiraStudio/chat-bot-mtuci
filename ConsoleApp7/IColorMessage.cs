using System;
using System.Collections.Generic;
using System.Text;

namespace ConversationBot
{
    interface IColorMessage
    {
        public void Print(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            text += "\n";
            Console.ForegroundColor = color;
            Console.WriteLine(text);

        }
    }
}
