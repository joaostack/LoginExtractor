using Lolcat;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace LoginExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            var asciiart = @"
██╗      ██████╗  ██████╗ ██╗███╗   ██╗    ███████╗██╗  ██╗████████╗██████╗  █████╗  ██████╗████████╗ ██████╗ ██████╗ 
██║     ██╔═══██╗██╔════╝ ██║████╗  ██║    ██╔════╝╚██╗██╔╝╚══██╔══╝██╔══██╗██╔══██╗██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗
██║     ██║   ██║██║  ███╗██║██╔██╗ ██║    █████╗   ╚███╔╝    ██║   ██████╔╝███████║██║        ██║   ██║   ██║██████╔╝
██║     ██║   ██║██║   ██║██║██║╚██╗██║    ██╔══╝   ██╔██╗    ██║   ██╔══██╗██╔══██║██║        ██║   ██║   ██║██╔══██╗
███████╗╚██████╔╝╚██████╔╝██║██║ ╚████║    ███████╗██╔╝ ██╗   ██║   ██║  ██║██║  ██║╚██████╗   ██║   ╚██████╔╝██║  ██║
╚══════╝ ╚═════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝    ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝
- By Ivy Frost";

            var help = @"
|========================================================|
|USE: ./loginextractor LOGINS_FOLDER_PATH SQL_FILE_OUTPUT|
|========================================================|
";

            var style = new RainbowStyle();
            var rainbow = new Rainbow(style);

            rainbow.WriteLineWithMarkup(asciiart);

            if (args.Length < 2)
            {
                rainbow.WriteLineWithMarkup(help);
                return;
            }

            var folderPath = args[0];
            string sqlPath = args[1];

            if (!Directory.Exists(folderPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error: The specified folder path '{folderPath}' does not exist.");
                Console.ResetColor();
                return;
            }

            try
            {
                rainbow.WriteLineWithMarkup($"TARGET: {folderPath}");
                string[] files = Directory.GetFiles(folderPath);

                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".txt" || Path.GetExtension(file) == ".log")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[+] Found {files.Length} leaks.");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[*] Extracting logins...");

                        using (var fileStream = File.OpenRead(file))
                        {
                            using (var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8))
                            {
                                string line;
                                bool foundCreds = false;

                                while ((line = reader.ReadLine()) != null)
                                {
                                    var credsParts = line.Split(':');
                                    if (credsParts.Length < 3)
                                    {
                                        continue;
                                    }


                                    using (StreamWriter outputSqlFile = new StreamWriter(sqlPath, true))
                                    {
                                        var url = credsParts[0].Trim();
                                        var user = credsParts[1].Trim();
                                        var pass = credsParts[2].Trim();
                                        var cleanedUrl = UrlClean(url);

                                        if (user == null)
                                        {
                                            user = "UNKNOW";
                                        }
                                        else if (pass == null)
                                        {
                                            pass = "UNKNOW";
                                        }

                                        outputSqlFile.WriteLine($"INSERT INTO creds(url, username, pass) VALUES ('{cleanedUrl}', '{user}', '{pass}');");

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write($"\r[+] LOGINS INSERTED IN SQL => {user.Length}");

                                        foundCreds = true;
                                    }
                                }

                                if (!foundCreds)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\n[!] Logins not extracted successfully, check that the format is correct 'URL:USER:PASS'.");
                                }
                                else
                                {
                                    rainbow.WriteLineWithMarkup("\n[+] Logins successfully extracted (:");
                                }
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error: {e.Message}");
                return;
            }
            finally
            {
                Console.ResetColor();
            }
        }

        static string UrlClean(string url)
        {
            int index = url.LastIndexOf("/");
            if (index >= 0)
                url = url.Substring(0, index);

            if (url.StartsWith("http//") || url.StartsWith("http://"))
            {
                url = url.Replace("http//", "");
                url = url.Replace("http://", "");
            }
            else if (url.StartsWith("https//") || url.StartsWith("https://"))
            {
                url = url.Replace("https//", "");
                url = url.Replace("https://", "");
            }
            else
            {
                return "UNKNOW";
            }

            return url;
        }
    }
}
