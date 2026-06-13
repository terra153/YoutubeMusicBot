using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace YoutubeMusicBot.Services
{
    public static class YoutubeCookieParser
    {
        public static List<Cookie> ParseCookieFile(string filePath)
        {
            var cookiesList = new List<Cookie>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл куки не найден по пути: {filePath}");
            }

            // Читаем файл построчно
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                // Пропускаем пустые строки и комментарии (#)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                // В Netscape формате столбцы разделены табуляцией '\t'
                string[] tokens = line.Split('\t');

                // В валидной строке куки должно быть ровно 7 колонок
                if (tokens.Length < 7)
                {
                    continue;
                }

                try
                {
                    string domain = tokens[0];
                    // Переводим "TRUE"/"FALSE" во флаг IncludeSubdomains
                    bool includeSubdomains = tokens[1].Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                    string path = tokens[2];
                    // Флаг Secure (передача только по HTTPS)
                    bool isSecure = tokens[3].Equals("TRUE", StringComparison.OrdinalIgnoreCase);

                    // Время истечения (Unix Timestamp). Если 0 — куки сессионные
                    long.TryParse(tokens[4], out long expiresUnix);

                    string name = tokens[5];
                    string value = tokens[6];

                    // Создаем стандартный .NET объект Cookie
                    var cookie = new Cookie(name, value, path, domain)
                    {
                        Secure = isSecure,
                        HttpOnly = false // В текстовом формате этот флаг обычно не хранится отдельно
                    };

                    // Настраиваем дату истечения, если она указана
                    if (expiresUnix > 0)
                    {
                        cookie.Expires = DateTimeOffset.FromUnixTimeSeconds(expiresUnix).DateTime;
                    }

                    cookiesList.Add(cookie);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка парсинга строки куки: {ex.Message}");
                }
            }

            return cookiesList;
        }
    }
}