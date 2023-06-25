using ProgSieciowe.Client;

namespace ProgSieciowe.ClientRuntime
{
    internal class ConsoleInputOutput : IInputOutput
    {
        public string GetString()
        {
            return Console.ReadLine()!;
        }

        public void WriteString(string str)
        {
            Console.WriteLine(str);
        }
    }
}
