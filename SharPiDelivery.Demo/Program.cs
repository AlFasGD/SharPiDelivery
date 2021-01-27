using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;

namespace SharPiDelivery.Demo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Write("Starting index: ");
            long startingIndex = long.Parse(ReadLine());

            Write("Digit count: ");
            int digits = int.Parse(ReadLine());

            WriteLine("\nThe requested digits are:\n");

            var deliveryClient = new PiDelivery();
            var deliveryTask = deliveryClient.GetDigits(startingIndex, digits);
            Task.WaitAll(DisplayDigits(deliveryTask, startingIndex));
        }

        private static async Task DisplayDigits(IAsyncEnumerable<string> deliveryTask, long currentIndex)
        {
            bool isFinalizedLine = false;

            int lineIndex = (int)(currentIndex % 50 - 1);
            if (lineIndex > -1)
                CursorLeft = lineIndex + lineIndex / 10;

            await foreach (var digitsBunch in deliveryTask)
                foreach (var digit in digitsBunch)
                    WriteDigit(digit);
            
            FinalizeLine();

            void WriteDigit(char digit)
            {
                Write(digit);
                isFinalizedLine = false;

                if (currentIndex == 0)
                {
                    WriteLine('.');
                    currentIndex++;
                    return;
                }

                if (currentIndex % 10 == 0)
                    Write(' ');

                if (currentIndex % 50 == 0)
                    FinalizeLine();

                currentIndex++;
            }
            void FinalizeLine()
            {
                if (isFinalizedLine)
                    return;

                CursorLeft = 55;
                WriteLine($"- {currentIndex:#,#}");
                isFinalizedLine = true;
            }
        }
    }
}
