using System;

namespace Gost28147AppBykovDenisov
{
    /// <summary>
    /// Упрощенная реализация одного раунда ГОСТ 28147-89
    /// <summary>
    public static class GostCipher
    {
        // S-блоки (таблицы замены)
        private static readonly int[,] SBlocks = new int[4, 16]
        {
            { 4, 10, 9, 2, 13, 8, 0, 14, 6, 11, 1, 12, 7, 15, 5, 3 },
            { 14, 11, 4, 12, 6, 13, 15, 10, 2, 3, 8, 1, 0, 7, 5, 9 },
            { 5, 8, 1, 13, 10, 3, 4, 2, 14, 15, 12, 7, 6, 0, 9, 11 },
            { 7, 13, 10, 1, 0, 8, 9, 15, 14, 4, 6, 12, 11, 2, 5, 3 }
        };

        /// <summary>
        /// Функция раунда шифрования
        /// </summary>
        /// <param name="rightPart">Правая 32-битная часть</param>
        /// <param name="roundKey">32-битный ключ раунда</param>
        /// <returns>Результат преобразования</returns>
        public static uint RoundFunction(uint rightPart, uint roundKey)
        {
            uint sum = rightPart + roundKey;
            uint substituted = Substitute(sum);
            uint rotated = (substituted << 11) | (substituted >> (32 - 11));
            return rotated;
        }

        /// <summary>
        /// Замена через S-блоки
        /// </summary>
        private static uint Substitute(uint value)
        {
            uint result = 0;
            for (int i = 0; i < 8; i++)
            {
                int nibble = (int)((value >> (4 * i)) & 0xF);
                int sBlockIndex = i % 4;
                int replaced = SBlocks[sBlockIndex, nibble];
                result |= (uint)(replaced << (4 * i));
            }
            return result;
        }

        /// <summary>
        /// Один раунд шифрования 64-битного блока
        /// </summary>
        /// <param name="block">64-битный блок</param>
        /// <param name="roundKey">32-битный ключ</param>
        /// <returns>Зашифрованный блок</returns>
        public static ulong EncryptRound(ulong block, uint roundKey)
        {
            uint left = (uint)(block >> 32);
            uint right = (uint)(block & 0xFFFFFFFF);
            uint newRight = left ^ RoundFunction(right, roundKey);
            uint newLeft = right;
            return ((ulong)newLeft << 32) | newRight;
        }

        /// <summary>
        /// Один раунд дешифрования 64-битного блока
        /// </summary>
        public static ulong DecryptRound(ulong block, uint roundKey)
        {
            return EncryptRound(block, roundKey);
        }
    }

    /// <summary>
    /// Главный класс программы
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Упрощенный ГОСТ 28147-89 ===\n");

            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1 - Шифрование");
                Console.WriteLine("2 - Дешифрование");
                Console.WriteLine("3 - Выход");
                Console.Write("Ваш выбор: ");

                string choice = Console.ReadLine();

                if (choice == "3") break;

                try
                {
                    Console.Write("\nВведите блок данных (HEX, 16 символов): ");
                    string blockHex = Console.ReadLine().Replace(" ", "").Replace("0x", "");

                    if (blockHex.Length != 16)
                    {
                        Console.WriteLine("ОШИБКА: Нужно 16 HEX символов!\n");
                        continue;
                    }

                    Console.Write("Введите ключ (HEX, 8 символов): ");
                    string keyHex = Console.ReadLine().Replace(" ", "").Replace("0x", "");

                    if (keyHex.Length != 8)
                    {
                        Console.WriteLine("ОШИБКА: Нужно 8 HEX символов!\n");
                        continue;
                    }

                    ulong block = Convert.ToUInt64(blockHex, 16);
                    uint key = Convert.ToUInt32(keyHex, 16);

                    ulong result;
                    if (choice == "1")
                    {
                        result = GostCipher.EncryptRound(block, key);
                        Console.WriteLine($"\nРезультат шифрования: {result:X16}\n");
                    }
                    else if (choice == "2")
                    {
                        result = GostCipher.DecryptRound(block, key);
                        Console.WriteLine($"\nРезультат дешифрования: {result:X16}\n");
                    }
                    else
                    {
                        Console.WriteLine("Неверный выбор!\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ОШИБКА: {ex.Message}\n");
                }
            }

            Console.WriteLine("\nПрограмма завершена. Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}