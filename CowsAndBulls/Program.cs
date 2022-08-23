using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Игра Быки И Коровы.
/// </summary>
namespace CowsAndBulls
{
    /// <summary>
    /// Класс Program.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Размер файла со статистикой игр (кол-во хранящихся попыток).
        /// </summary>
        private const int STATS_FILE_MAX_SIZE = 100;

        /// <summary>
        /// Название файла с сохраненными играми.
        /// </summary>
        private const string STATS_FILE_PATH = "Your_Games.txt";

        /// <summary>
        /// Аргументы командной строки.
        /// Вынесены, чтобы можно было пользоваться не только из Main.
        /// </summary>
        private static string[] ComandLineArgs;

        /// <summary>
        /// Метод Main.
        /// </summary>
        /// <param name="args"> Аргументы командной строки, которые мы выносим в статический массив в классе Program. </param>
        public static void Main(string[] args)
        {
            try
            {
                ComandLineArgs = args.Length == 0 ? null : args;

                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                RepeatingMethod();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Возникла ошибка: " + exception.Message);
            }
            finally
            {
                Console.WriteLine("Приложение завершено.");
            }
        }

        /// <summary>
        /// Метод повтора (по сути является телом приложения).
        /// </summary>
        private static void RepeatingMethod()
        {
            bool guessed;
            string numberToGuess = string.Empty;
            int attemptsCount = 0;
            do
            {
                Console.Clear();
                try
                {
                    if (attemptsCount == 0)
                        numberToGuess = MakingNumberWithUser();

                    Console.WriteLine($"Попытайтесь угадать число.{Environment.NewLine}Введите свою догадку ввиде числа длины {numberToGuess.Length} и без повторяющихся цифр.");

                    if (CheckEquality(numberToGuess, Console.ReadLine(), out int bulls, out int cows))
                    {
                        guessed = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ПОЗДРАВЛЯЮ! ВЫ УГАДАЛИ!!!");
                        Console.ResetColor();
                        Console.WriteLine($"Попыток затрачено: {++attemptsCount}.");

                        WriteGameToFile(STATS_FILE_PATH, attemptsCount, numberToGuess);

                        Console.WriteLine($"Статистика вашей игры была записана в файл с названием {STATS_FILE_PATH}");
                        
                        // Если ты, Дружище, маковод (ну или юзаешь что-то из списка условий ниже), то извиняй, бипать не будет :(
                        if (!OperatingSystem.IsMacOS() && !OperatingSystem.IsAndroid() && !OperatingSystem.IsTvOS() && !OperatingSystem.IsBrowser())
                            for (int i = 0; i < 3; i++)
                                Console.Beep(5000, 20);
                    }
                    else
                    {
                        guessed = false;
                        Console.WriteLine("Вы не угадали число.");
                        Console.WriteLine("Сведения о попытке.");
                        Console.WriteLine("Быков: " + bulls.ToString());
                        Console.WriteLine("Коров: " + cows.ToString());
                        Console.WriteLine($"Это была ваша {++attemptsCount}я попытка.");
                    }
                }
                catch (ArgumentException argumentException)
                {
                    guessed = false;
                    Console.WriteLine(argumentException.Message);
                }
                catch (FormatException formatException)
                {
                    guessed = false;
                    Console.WriteLine(formatException.Message);
                }

                Console.WriteLine("Нажмите клавишу \"escape\", если хотите выйти из приложения, иначе нажмите любую другую клавишу.");

                if (guessed)
                    Console.WriteLine("Поскольку число вы угадали, Вам будет предложено новое число, если Вы продолжите играть.");

            } while (Console.ReadKey().Key != ConsoleKey.Escape && !guessed);

            Console.WriteLine();
            Console.WriteLine();
        }

        #region Help methods.
        /// <summary>
        /// Метод задания пользователем параметров создания числа и его получения.
        /// </summary>
        /// <returns> Загаданное число. </returns>
        private static string MakingNumberWithUser()
        {
            Console.WriteLine("Есть несколько вариантов задания количества цифр в числе.");

            string[] WayToGetNumberLengthEnumNames = Enum.GetNames(typeof(WaysToGetNumberLength));

            for (int i = 0; i < WayToGetNumberLengthEnumNames.Length; i++)
                Console.WriteLine($"{i + 1}. {WayToGetNumberLengthEnumNames[i]}");

            Console.WriteLine();
            Console.Write("Введите число, соответствующее выбранному способ: ");

            if (!byte.TryParse(Console.ReadLine(), out byte way) || way < 1 || way >= WayToGetNumberLengthEnumNames.Length + 1) 
                throw new FormatException("Некорректно выбран способ задать длину искомого числа.");

            return GenerateRandomNumber(GetNumberLength((WaysToGetNumberLength)(way - 1)));
        }

        /// <summary>
        /// Метод получения от пользователя длины числа.
        /// </summary>
        /// <param name="wayToGetNumberLength"> Способ получения числа. </param>
        /// <returns> Длина числа. </returns>
        private static int GetNumberLength(WaysToGetNumberLength wayToGetNumberLength)
        {
            int length;
            switch (wayToGetNumberLength)
            {
                case WaysToGetNumberLength.FromConsole:
                    Console.Write("Введите количество цифр в числе в консоль: ");
                    if (!int.TryParse(Console.ReadLine(), out length))
                        throw new FormatException("Введено некорректное значение.");
                    return length;

                case WaysToGetNumberLength.FromCommandLine:
                    Console.WriteLine("Количество цифр в числе должно быть введено в качестве первого аргумента командной строке.");
                    if (ComandLineArgs == null || ComandLineArgs.Length != 1 || !int.TryParse(ComandLineArgs[0], out length)) 
                        throw new FormatException("Некорректно введены аргументы командной строки.");
                    return length;

                case WaysToGetNumberLength.Randomly:
                    length = new Random().Next(1, 11);
                    Console.WriteLine($"Количество цифр в числе задано рандомно и равно: {length}");
                    return length;

                default:
                    Console.WriteLine("Способ задания количества цифр в числе не определен или определен неверно.");
                    Console.WriteLine("Количество цифр в числе будет задано рандомно.");
                    goto case WaysToGetNumberLength.Randomly;
            }
        }

        /// <summary>
        /// Метод рандомного создания числа.
        /// </summary>
        /// <param name="length"> Длина числа. </param>
        /// <returns> Рандомно созданное число. </returns>
        private static string GenerateRandomNumber(int length)
        {
            Random rnd = new Random();

            List<int> possibleNumbers = Enumerable.Range(0, 10).ToList();

            string generatedNumber = string.Empty;

            for (int i = 0; i < length; i++)
            {
                int randomIndex = rnd.Next(i == 0 ? 1 : 0, possibleNumbers.Count);
                generatedNumber += possibleNumbers[randomIndex];
                possibleNumbers.RemoveAt(randomIndex);
            }

            return generatedNumber;
        }

        /// <summary>
        /// Сверка загаданного числа и введенного пользователем.
        /// </summary>
        /// <param name="numberToGuess"> Загаданное число. </param>
        /// <param name="inputNumber"> Введенное ползователем число. </param>
        /// <param name="bulls"> Количество быков. </param>
        /// <param name="cows"> Количество коров. </param>
        /// <returns> Отгадал ли пользователь число полностью (true or false). </returns>
        private static bool CheckEquality(string numberToGuess, string inputNumber, out int bulls, out int cows)
        {
            bulls = 0;
            cows = 0;

            if (!inputNumber.All(ch => ch >= '0' && ch <= '9'))
                throw new FormatException("Во введенном числе обнаружены некорректные символы.");

            if (inputNumber.Length != inputNumber.Distinct().Count())
                throw new FormatException("В числе не может быть повторяющихся цифр.");

            if (numberToGuess.Length != inputNumber.Length)
                throw new ArgumentException($"Числа не совпадают по длине.{Environment.NewLine}Введите число длины {numberToGuess.Length}.");

            if (numberToGuess == inputNumber) 
            {
                bulls = numberToGuess.Length;
                return true;
            }

            for (int i = 0; i < inputNumber.Length; i++)
            {
                if (inputNumber[i] == numberToGuess[i])
                {
                    bulls++;
                }
                else if (numberToGuess.Contains(inputNumber[i]))
                {
                    cows++;
                }
            }

            return false;
        }

        /// <summary>
        /// Метод записи в файл сведений об игре.
        /// </summary>
        /// <param name="attemptsCount"> Кол-во попыток. </param>
        /// <param name="numberToGuess"> Искомое число. </param>
        private static void WriteGameToFile(string path, int attemptsCount, string numberToGuess)
        {
            List<string> attemptsLines = new List<string>();
            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None), Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                    attemptsLines.Add(sr.ReadLine());
            }

            attemptsLines.Reverse();

            if (attemptsLines.Count >= STATS_FILE_MAX_SIZE)
                attemptsLines.RemoveAt(STATS_FILE_MAX_SIZE - 1);

            attemptsLines.Add($"Попыток: {attemptsCount}. Исходное число: {numberToGuess}.");
            attemptsLines.Reverse();

            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None), Encoding.UTF8))
            {
                for (int i = 0; i < attemptsLines.Count; i++)
                    sw.WriteLine($"Игра №{i + 1}. {attemptsLines[i]}");
            }
        }
        #endregion
    }
}
