using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using xNet;
using HtmlAgilityPack;

namespace ConversationBot
{
    class BotManager
    {
        private const string PATH = @"D:\Tokens\";
        private const string FILENAME = @"tokens.txt";
        private static string commandsPath;

        private string groupId = "199433852";
        private ulong applicationId = 7633852;
        private string peerId;
        private string fromId;
        private string id;
        private string message;

        private DirectoryInfo dirInfo;
        private IColorMessage log;
        private LoggerImage logImg;
        private bool empty = false;

        public bool user = false;
        public string data;
        public VkApi vkApi;
        public Random random;
        public WebClient web;

        private List<Commands> commands;
        private List<RssNews> listNews;
        const string BOTCOMMANDSUF = "";

        private void Initialization()
        {
            web = new WebClient() { Encoding = Encoding.UTF8 };
            random = new Random();
            dirInfo = new DirectoryInfo(PATH);
            log = new Logger();
            vkApi = new VkApi();
            logImg = new LoggerImage();
            commandsPath = Environment.CurrentDirectory;
        }

        public void StartBot()
        {
            Initialization();
            LoadCommands();
            IsUser();
            Authorization(CreateTokenAndAuth());
            if (vkApi.IsAuthorized)
                EyeInit();
        }

        private void IsUser()
        {
            string forLs = "лс";
            string forConversation = "беседа";
            log.Print($"[{DateTime.Now:HH:mm}] Бот нужен для личных сообщений или для беседы?", ConsoleColor.Cyan);
            log.Print($"[{DateTime.Now:HH:mm}] Напишите: беседа или ЛС");
            string check = Console.ReadLine();
                if (string.IsNullOrEmpty(check))
                {
                log.Print($"[{DateTime.Now:HH:mm}] Вы не выбрали значение", ConsoleColor.Red);
                StartBot();
                }
                else if (check.ToLower().Equals(forConversation))
                {
                log.Print($"[{DateTime.Now:HH:mm}] Вы выбрали бота для {forConversation.Replace("а", "ы")}", ConsoleColor.Green);
                user = false;
                }
                else if (check.ToLower().Equals(forLs))
                {
                log.Print($"[{DateTime.Now:HH:mm}] Вы выбрали бота для {forLs}", ConsoleColor.Green);
                user = true;
                }
                else
                {
                log.Print($"[{DateTime.Now:HH:mm}] Выберите правильное значение", ConsoleColor.Red);
                StartBot();
                }
        }

        public string CreateTokenAndAuth()
        {
            byte[] array;

            if (!dirInfo.Exists || empty)
            {
                dirInfo.Create();
                    using (FileStream fs = new FileStream($"{PATH}{FILENAME}", FileMode.OpenOrCreate))
                    {
                        log.Print($"[{DateTime.Now:HH:mm}] Значение токена будет сохранено по адресу {fs.Name}");
                        log.Print($"[{DateTime.Now:HH:mm}] Введите токен вашего аккаунта");
                        data = Console.ReadLine();
                        array = Encoding.Default.GetBytes(data);
                        fs.Write(array, 0, array.Length);
                    if (array.Length == 0)
                    {
                        empty = true;
                        log.Print($"[{DateTime.Now:HH:mm}] Вы не ввели токен", ConsoleColor.Red);
                    }
                    else
                    {
                        empty = false;
                    }
                    }            
            }            
            else
            {
                using (FileStream fs = new FileStream($"{PATH}{FILENAME}", FileMode.OpenOrCreate))
                {
                    log.Print($"[{DateTime.Now:HH:mm}] Значение токена взято из файла по адресу {fs.Name}");
                    array = new byte[fs.Length];
                    if (array.Length == 0)
                    {
                        log.Print($"[{DateTime.Now:HH:mm}] Упс, кажется у вас пустое значение токена", ConsoleColor.Red);
                        empty = true;                        
                    }
                    fs.Read(array, 0, array.Length);
                    data = Encoding.Default.GetString(array);
                }
            }
            if (empty)
                CreateTokenAndAuth();
            return data;
        }

        public void Authorization(string token)
        {
            try
            {
                vkApi.Authorize(new ApiAuthParams {
                    AccessToken = token,
                    //ApplicationId = applicationId,
                    //Settings = Settings.All
                });
                logImg.Print();
                log.Print($"[{DateTime.Now:HH:mm}] Вы успешно авторизовались!", ConsoleColor.Green);
            }
            catch(Exception ex)
            {
                log.Print($"[{DateTime.Now:HH:mm}] Не удалось совершить авторизацию" + Environment.NewLine + "Код ошибки: " + ex.Message, ConsoleColor.Red);
                data = null;
                log.Print($"[{DateTime.Now:HH:mm}] Значение токена было удалено" + Environment.NewLine + "Пожалуйста, запишите правильно значение токена...", ConsoleColor.Cyan);
                using (FileStream fs = new FileStream($"{PATH}{FILENAME}", FileMode.Truncate))
                {

                }
                    Authorization(CreateTokenAndAuth());
            }
        }



        private void LoadCommands()
        {
            commands = new List<Commands>();
            FileStream file = new FileStream(commandsPath + @"\Commands.txt", FileMode.OpenOrCreate);
            StreamReader readFile = new StreamReader(file);

            if (readFile.EndOfStream)
                log.Print($"[{DateTime.Now:HH:mm}] В библиотеке нет ни одной команды. Добавьте команды через сообщения: ~<Команда>~<Ответ>", ConsoleColor.Red);

            while (!readFile.EndOfStream)
            {
                try
                {
                    string line = readFile.ReadLine();

                    commands.Add(new Commands
                    {
                        command = line.Substring(0, line.IndexOf('~')).ToLower(),
                        answer = line.Substring(line.IndexOf('~') + 1).ToLower()
                    });
                }
                catch(Exception ex)
                {
                    log.Print($"[{DateTime.Now:HH:mm}] Ошибка добавления команд из файла в список" + Environment.NewLine + $"Код ошибки: {ex.Message}", ConsoleColor.Red);
                    continue;
                }
            }
            readFile.Close();
        }

        public void EyeInit()
        {
            var webClient = new WebClient() { Encoding = Encoding.UTF8};
            var param = new VkParameters() { };
            param.Add("group_id", groupId);
            param.Add("access_token", data);

            dynamic responseLongPoll = JObject.Parse(vkApi.Call("groups.getLongPollServer", param).RawJson);
            string json = string.Empty;
            string url = string.Empty;

            while (true)
            {
                try
                {
                    url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=25&mode=2&version=3",
                                        responseLongPoll.response.server.ToString(),
                                        responseLongPoll.response.key.ToString(),
                                        json != string.Empty ? JObject.Parse(json)["ts"].ToString() : responseLongPoll.response.ts.ToString());
                    json = webClient.DownloadString(url);

                    var col = JObject.Parse(json)["updates"].ToList();
                    foreach (var item in col)
                    {
                        if (item["type"].ToString() == "message_new")
                        {
                            fromId = item["object"]["message"]["from_id"].ToString();
                            peerId = item["object"]["message"]["peer_id"].ToString();
                            //id = item["object"]["message"]["id"].ToString();
                            message = item["object"]["message"]["text"].ToString();

                            if (long.Parse(peerId) > 2000000000)
                                log.Print($"[{DateTime.Now:HH:mm}] Новое сообщение из беседы от ({WhoIs(long.Parse(fromId))}) {item["object"]["message"]["text"]}", ConsoleColor.White);
                            else
                                log.Print($"[{DateTime.Now:HH:mm}] Новое сообщение от ({WhoIs(long.Parse(fromId))}) {item["object"]["message"]["text"]}", ConsoleColor.White);

                            CheckCommands(message);
                        }
                    }
                }
                catch(Exception ex)
                {
                    log.Print($"[{DateTime.Now:HH:mm}] Время сеанса истекло!", ConsoleColor.Red);
                    log.Print($"[{DateTime.Now:HH:mm}] Код ошибки: {ex.Message}", ConsoleColor.Red);
                    log.Print($"[{DateTime.Now:HH:mm}] Вы автоматически будете переавторизованы...", ConsoleColor.White);
                    Authorization(CreateTokenAndAuth());
                }
            }
        }

        public void CheckCommands(string msg)
        {
            message = msg.ToLower();
            
            foreach(var v in commands)
            {
                if (message.Equals(v.command))
                {
                    SendMessages(v.answer);
                }
            }

            switch(msg.ToLower())
            {
                case "новости":
                    SendMessages(LoadNews("https://lenta.ru/rss/news"));
                    break;
                case "помощь":
                    SendMessages(HelpText());
                    break;
                case "очистить историю погоды":
                    DeleteHistoryWeather();
                    SendMessages("История была очищена!");
                    break;
                default:
                    if (msg.ToLower().Contains("учи~"))
                        Learn(msg.Substring(5, msg.Length - 5));
                    else if (msg.ToLower().Contains("случайное число от"))
                        SendMessages(Convert.ToString(RandomValue(msg.ToLower())));
                    else if (msg.ToLower().Contains("погода"))
                        SendMessages(ShowWeather(msg.ToLower()));
                    break;
            }
        }

        public void DeleteHistoryWeather()
        {
            using (FileStream fs = new FileStream(commandsPath + @"\Weather.txt", FileMode.Truncate))
            {
            }

        }

        public string ShowWeather(string city)
        {
            try
            {
                string url = $"http://api.openweathermap.org/data/2.5/forecast?q={ city.Substring(city.IndexOf(" ") + 1)}&lang=ru&mode=json&units=metric&appid=54e668d8c6f2ed344d170c0d9e091532";
                var webClient = new WebClient() { Encoding = Encoding.UTF8 };
                string json = webClient.DownloadString(url);
                var col = JObject.Parse(json)["list"].ToList();
                string cityName = JObject.Parse(json)["city"]["name"].ToString();
                int count = 0;
                string[] str = System.IO.File.ReadAllLines(commandsPath + @"\Weather.txt");

                if (str.Length == 0)
                {
                    DeleteHistoryWeather();
                    if (System.IO.File.Exists(commandsPath + @"\Weather.txt"))
                    {
                        foreach (var item in col)
                        {
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", $"------------{cityName}------------" + Environment.NewLine);
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", "-------------------------------------" + Environment.NewLine);
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", $"Температура на {item["dt_txt"]}" + Environment.NewLine);
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", $"Ощущается как: {item["main"]["temp"]} ({item["weather"][0]["description"]})" + Environment.NewLine);
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", $"Скорость ветра: {item["wind"]["speed"]}" + Environment.NewLine);
                            System.IO.File.AppendAllText(commandsPath + @"\Weather.txt", "-------------------------------------" + Environment.NewLine);
                            count++;
                            if (count > 16)
                                break;
                        }
                        return System.IO.File.ReadAllText(commandsPath + @"\Weather.txt");
                    }
                    else
                    {
                        return "Пока я тебе не могу показать прогноз погоды";
                    }
                }
                else
                {
                    return System.IO.File.ReadAllText(commandsPath + @"\Weather.txt") + Environment.NewLine + "Чтобы обновить погоду, воспользуйся командой: очистить историю погоды";
                }
            }
            catch(Exception ex)
            {
                return "Неправильный формат ввода или не удалось сделать запрос погоды!";
            }

        }

        public string HelpText()
        {
            if (System.IO.File.Exists(commandsPath + @"\Help.txt"))
            {
                string text = System.IO.File.ReadAllText(commandsPath + @"\Help.txt");
                return text;
            }

            return "Описание команд не было добавлено разработчиком";
        }

        public int RandomValue(string values)
        {
            string numbers = values.Substring(values.IndexOf("от") + 3);
            int minValue = int.Parse(numbers.Substring(0, numbers.IndexOf('д')));
            int maxValue = int.Parse(numbers.Substring(numbers.IndexOf('о') + 2));

            if (minValue > maxValue)
            {
                return random.Next(maxValue, minValue);
            }
            else if (minValue == maxValue)
            {
                return minValue;
            }

            return random.Next(minValue, maxValue);
        }

        public string LoadNews(string url)
        {
            string rss = web.DownloadString(url);            
            XDocument doc = XDocument.Parse(rss);
            listNews = (from descendant in doc.Descendants("item")
                        select new RssNews()
                        {
                            title = descendant.Element("title").Value,
                            description = descendant.Element("description").Value,
                            link = descendant.Element("link").Value,
                            pubDate = descendant.Element("pubDate").Value
                        }).ToList();

            string text = string.Empty;
            if (listNews != null)
            {
                int i = random.Next(0, listNews.Count - 1);
                text = "Запись опубликована: " +
                    listNews[i].pubDate.Substring(0, listNews[i].pubDate.IndexOf("+")) +
                    Environment.NewLine +
                    "----------------------------------" +
                    Environment.NewLine +
                    listNews[i].title +
                    Environment.NewLine +
                    "----------------------------------" +
                    Environment.NewLine +
                    listNews[i].description +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Источник: " +
                    listNews[i].link;
                return text;
            }
            else
                return "Не удалось загрузить новости";
        }

        public void Learn(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(@"Commands.txt", msg + Environment.NewLine);
                commands.Add(new Commands
                {
                    command = msg.Substring(0, msg.IndexOf('~')).ToLower(),
                    answer = msg.Substring(msg.IndexOf('~') + 1).ToLower()
                });
                log.Print($"[{DateTime.Now:HH:mm}] Добавлена новая команда {msg.Substring(0, msg.IndexOf("~"))}", ConsoleColor.Green);
                SendMessages("Команда добавлена");
            }
            catch(Exception ex)
            {
                log.Print($"[{DateTime.Now:HH:mm}] Я не смог добавить команду" + Environment.NewLine + $"Код ошибки: {ex.Message}", ConsoleColor.Red);
                SendMessages("Я не смог добавить эту команду");
            }
        }

        public string WhoIs(long fromId)
        {
                var userInfo = vkApi.Users.Get(fromId);
                return userInfo.FirstName + " " + userInfo.LastName;
        }

        public void SendMessages(string msg)
        {
            vkApi.Messages.Send(new MessagesSendParams { 
                PeerId = long.Parse(peerId), 
                Message = msg, 
                RandomId = new Random().Next() 
            });
        }
        
    }
}