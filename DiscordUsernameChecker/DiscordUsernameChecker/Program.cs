using Spectre.Console;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
internal static class Program
{
    static Program()
    {
        Console.Title = $"Discord Username Checker / V1.0 by -> [gxqk]";

        appSettings = new("config.ini");
    }
    static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private static ConcurrentQueue<string> usernameQueue = new ConcurrentQueue<string>();
    private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public static Settings? appSettings;
    internal static string[] Proxies = File.ReadAllLines("proxies.txt");
	internal static int ProxyIndex = 0;
	private static readonly object ValidLock = new object();
    private static readonly object DebugLock = new object();
    private static string date = DateTime.Now.ToString("MM-dd-yyyy");
    private static int takenCount = 0;
    private static int availableCount = 0;
    private static readonly object counterLock = new object();
    private static Status? statusDisplay;
    public class CheckJson
    {
        public bool taken { get; set; }
        public double retry_after { get; set; }
    }
	public class Data
	{
		public string sitekey { get; set; }
		public string url { get; set; }
		public string proxy { get; set; }
		public string rqdata { get; set; }
	}

	public class createTask
	{
		public string task_type { get; set; }
		public string api_key { get; set; }
		public Data data { get; set; }
	}

	public class createTaskResponse
	{
		public bool error { get; set; }
		public string task_id { get; set; }
	}

	public class getTaskData
	{
		public string task_id { get; set; }
		public string api_key { get; set; }
	}

	public class getTaskDataResponse
	{
		public bool error { get; set; }
		public getTaskDataTask task { get; set; }
	}

	public class getTaskDataTask
	{
		public string captcha_key { get; set; }
		public bool refunded { get; set; }
		public string state { get; set; }
	}

	public class CaptchaStuff
	{
		public List<string> captcha_key { get; set; }
		public string captcha_sitekey { get; set; }
		public string captcha_service { get; set; }
		public string captcha_rqdata { get; set; }
		public string captcha_rqtoken { get; set; }
	}

	static WebProxy GetProxy()
    {
        try
        {
            string proxyAddress;

            lock (Proxies)
            {
                Random random = new Random();

                if (ProxyIndex >= Proxies.Length)
                {
                    ProxyIndex = 0;
                }

                proxyAddress = Proxies[ProxyIndex++];
            }

            // Create a WebProxy object from the proxy address
            if (proxyAddress.Split(':').Length == 4)
            {
                var proxyHost = proxyAddress.Split(':')[0];
                int proxyPort = Int32.Parse(proxyAddress.Split(':')[1]);
                var username = proxyAddress.Split(':')[2];
                var password = proxyAddress.Split(':')[3];
                ICredentials credentials = new NetworkCredential(username, password);
                var proxyUri = new Uri($"http://{proxyHost}:{proxyPort}");
                return new WebProxy(proxyUri, false, null, credentials);
            }
            else if (proxyAddress.Split(':').Length == 2)
            {
                var proxyHost = proxyAddress.Split(':')[0];
                int proxyPort = Int32.Parse(proxyAddress.Split(':')[1]);
                return new WebProxy(proxyHost, proxyPort);
            }
            else
            {
                throw new ArgumentException("Invalid proxy format.");
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid proxy.");
        }
    }
	static async Task LoadUsernamesAsync(string filePath)
    {
        var usernames = await File.ReadAllLinesAsync(filePath);
        foreach (var username in usernames.Distinct())
        {
            usernameQueue.Enqueue(username);
        }
    }

    static async Task Main()
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule("[yellow]Discord Username Tool[/]").RuleStyle("grey").Centered());
            AnsiConsole.WriteLine();
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an option:")
                    .AddChoices(new[] {
                        "1. Username Generator",
                        "2. Username Checker",
                        "3. Exit"
                    }));

            switch (choice)
            {
                case "1. Username Generator":
                    await UsernameGenerator();
                    break;
                case "2. Username Checker":
                    await UsernameChecker();
                    break;
                case "3. Exit":
                    return;
            }
        }
    }

    static async Task UsernameGenerator()
    {
        Console.Clear();
        Console.Title = "Username Generator | Generating usernames...";
        AnsiConsole.Write(new Rule("[yellow]Username Generator[/]").RuleStyle("grey").Centered());
        AnsiConsole.WriteLine();


        int usernameCount = AnsiConsole.Prompt(
            new TextPrompt<int>("How many usernames do you want to generate? ")
                .ValidationErrorMessage("[red]Please enter a valid number[/]")
                .Validate(num => num > 0 ? ValidationResult.Success() : ValidationResult.Error("Number must be greater than 0")));

        int usernameLength = AnsiConsole.Prompt(
            new TextPrompt<int>("What length for each username? ")
                .ValidationErrorMessage("[red]Please enter a valid number[/]")
                .Validate(num => num >= 3 && num <= 32 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("Length must be between 3 and 32 characters")));

        bool includeNumbers = AnsiConsole.Prompt(
            new ConfirmationPrompt("Do you want to include numbers?"));

        bool includeDots = AnsiConsole.Prompt(
            new ConfirmationPrompt("Do you want to include one dot (.) per username?"));

        bool includeUnderscores = AnsiConsole.Prompt(
            new ConfirmationPrompt("Do you want to include underscores (_)?"));

        // Generate usernames
        var random = new Random();
        var usernames = new HashSet<string>(); // HashSet to avoid duplicates
        string chars = "abcdefghijklmnopqrstuvwxyz";
        if (includeNumbers) chars += "0123456789";
        if (includeDots) chars += ".";
        if (includeUnderscores) chars += "_";

        AnsiConsole.Status()
            .Start("Generating usernames...", ctx =>
            {
                while (usernames.Count < usernameCount)
                {
                    var username = new string(Enumerable.Repeat(chars, usernameLength)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
                    

                    if ((!includeDots || (username[0] != '.' && username[^1] != '.')) &&
                        (!includeUnderscores || (username[0] != '_' && username[^1] != '_')))
                    {

                        if (!includeDots || !includeUnderscores ||
                            (username.Count(c => c == '.') < 2 && username.Count(c => c == '_') < 2))
                        {
                            usernames.Add(username);
                        }
                    }
                }
            });


        Directory.CreateDirectory("output");

        string date = DateTime.Now.ToString("dd-MM-yyyy");
        string fileName = Path.Combine("output", $"GeneratedUsernames-{date}.txt");
        await File.WriteAllLinesAsync(fileName, usernames);


        AnsiConsole.MarkupLine($"\n[green]✓[/] {usernameCount} usernames have been generated");
        AnsiConsole.MarkupLine($"[green]✓[/] Saved to file: [yellow]{fileName}[/]");
        
        AnsiConsole.WriteLine("\nPress Enter to return to the main menu...");
        Console.ReadLine();


        Console.Title = $"Username Generator | Generated {usernameCount} usernames";
    }

    static async Task UsernameChecker()
    {
        Console.Clear();
        takenCount = 0;
        availableCount = 0;
        Console.Title = $"Results | Taken: 0 -> Available: 0";

        await LoadUsernamesAsync("usernames.txt");
        var tasks = new Task[appSettings.Threads];
        for (int i = 0; i < appSettings.Threads; i++)
        {
            tasks[i] = Task.Run(() => ConsumeUsernames(cancellationTokenSource.Token));
        }

        await Task.WhenAll(tasks);

        AnsiConsole.Write(new Markup($"[yellow]Checker Completed List\n[/]"));
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public class PostJson
    {
        public string username { get; set; }
    }
    public class ClaimJson
    {
        public string username { get; set; }
        public string password { get; set; }
    }
	static async Task ConsumeUsernames(CancellationToken cancellationToken)
    {
        // Create output directory if it doesn't exist
        Directory.CreateDirectory("output");

        while (!usernameQueue.IsEmpty && !cancellationToken.IsCancellationRequested)
        {
            if (usernameQueue.TryDequeue(out var username))
            {
                try
                {
                    using (var CheckClient = new HttpClient(new HttpClientHandler { Proxy = GetProxy(), UseProxy = true }))
                    {
                        HttpResponseMessage response = await CheckClient.PostAsync("https://discord.com/api/v9/unique-username/username-attempt-unauthed", new StringContent(JsonSerializer.Serialize(new PostJson { username = username }), Encoding.UTF8, "application/json"));
                        CheckClient.DefaultRequestHeaders.Clear();
                        CheckClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        CheckClient.DefaultRequestHeaders.Add("X-Discord-Locale", "en-US");
                        CheckClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");
                        string responseBody = await response.Content.ReadAsStringAsync();
                        CheckJson jsonObject = JsonSerializer.Deserialize<CheckJson>(responseBody);

                        if (jsonObject.taken)
                        {
                            lock (counterLock)
                            {
                                takenCount++;
                                Console.Title = $"Results | Taken: {takenCount} -> Available: {availableCount}";
                            }
                            AnsiConsole.Markup($"[red]Username Taken: {username}[/]\n");
                        }
                        else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            usernameQueue.Enqueue(username);
                        }
                        else if (!jsonObject.taken)
                        {
                            lock (counterLock)
                            {
                                availableCount++;
                                Console.Title = $"Results | Taken: {takenCount} -> Available: {availableCount}";
                            }
                            AnsiConsole.Markup($"[green]Username Available: {username}[/]\n");
                            lock (ValidLock)
                            {
                                File.AppendAllText(Path.Combine("output", $"ValidUsernames-{date}.txt"), $"{username}\n");
                            }
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    usernameQueue.Enqueue(username);
                    if (appSettings.Debug)
                    {
                        lock (DebugLock)
                        {
                            File.AppendAllText(Path.Combine("output", $"DebugLogs-{date}.txt"), $"{e.Message.ToString()}\n");
                        }
                    }
                }
                catch (Exception e)
                {
                    usernameQueue.Enqueue(username);
                    if (appSettings.Debug)
                    {
                        lock (DebugLock)
                        {
                            File.AppendAllText(Path.Combine("output", $"DebugLogs-{date}.txt"), $"{e.Message.ToString()}\n");
                        }
                    }
                }
            }
        }
    }
}
