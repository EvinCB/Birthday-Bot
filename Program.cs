using Discord;
using Discord.WebSocket;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotNetEnv;

class Program
{
    private DiscordSocketClient _client;
    private Dictionary<string, string> _birthdays = new(); //stores the birthdays and userID's
    private string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "birthdays.json");     // this makes a path to a file named birthdays.json in the same folder as the .exe. this is where the birthdays are saved



    //this is where the instance of the program class starts. basically starts the bot
    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.MainAsync();

    }

    public async Task MainAsync()
    {
        Env.Load(); //Loads the.env file
        string token = Env.GetString("DISCORD_TOKEN");
        Console.WriteLine($"Loaded token: {(string.IsNullOrEmpty(token) ? "MISSING" : "OK")}");

        var config = new DiscordSocketConfig
        {
            //these are the gateways needed for the bot to listen for. if you use all then it will listen for things that it doesnt need to like Voice chat ect.
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildMembers
        };


        _client = new DiscordSocketClient(config); // this creates the bot client
        _client.Log += Log;                         //logs info for the bot whe it is connected and when a message is received   
        _client.Ready += OnReady;                        
        _client.MessageReceived += HandleMessage;         


        //this if statement will load the saved birthday from the file. if it does not exist then it will start a new one
        if (File.Exists(_filePath))
        {

            string json = File.ReadAllText(_filePath);
            _birthdays = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();

        }
        else
        {
            _birthdays = new Dictionary<string,string>();  
        }

        //these three lines of code logs in and connects the bot to discord using the generated token
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
       
       


        //this keeps the app runniing 
        await Task.Delay(-1);
    }


    //the Log method will log discord client events like connection or warnings ect.
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }


    //the OnReady method logs when the bot is fully connected to discord and starts the birthday check task
    private Task OnReady()
    {
        Console.WriteLine($"✅ Bot is connected! Logged in as {_client.CurrentUser}");
        _ = StartDailyBirthdayCheck();
        return Task.CompletedTask;



    }


    private async Task OnDisconnected (Exception ex)
    {
        Console.WriteLine($"⚠️ Bot disconnected! Reason: {ex?.Message ?? "Unknown"}");

        //wait 5 seconds
        await Task.Delay(5000);

        try
        {
            await _client.StartAsync(); //this trys to reconnect the bot 
            Console.WriteLine("Attempted to reconnect.");
        }

        catch (Exception reconnectEx)
        {
            Console.WriteLine($"❌ Reconnect failed: {reconnectEx.Message}");
        }
    }

    //this handles the commands that users tpye. the bot scans to see if what was type starts with any of these slash commands. If it does then the code is carried out
    private async Task HandleMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return; // ignore bots

        string userId = message.Author.Id.ToString();

        if (message.Content.StartsWith("/birthday"))
        {
            string[] parts = message.Content.Split(' ');

            if (parts.Length == 2 && Regex.IsMatch(parts[1], @"^\d{2}-\d{2}$"))
            {
                string birthday = parts[1];
                _birthdays[userId] = birthday;

                string json = JsonSerializer.Serialize(_birthdays, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);

                Console.WriteLine("✅ Birthday has been saved");
                await message.Channel.SendMessageAsync($"🎉 Got it, {message.Author.Username}! Your birthday is set to {birthday}.");
            }
            else
            {
                await message.Channel.SendMessageAsync("❌ Format error! Please use `/birthday MM-DD` (e.g. `/birthday 04-03`)");
            }
        }
        else if (message.Content.StartsWith("/mybirthday"))
        {
            if (_birthdays.ContainsKey(userId))
            {
                string savedBirthday = _birthdays[userId];
                await message.Channel.SendMessageAsync($"🎂 {message.Author.Username}, your birthday is saved as {savedBirthday}.");
            }
            else
            {
                await message.Channel.SendMessageAsync($"❌ {message.Author.Username}, you haven’t set a birthday yet. Use `/birthday MM-DD`.");
            }
        }
        else if (message.Content.StartsWith("/removebirthday"))
        {
            if (_birthdays.ContainsKey(userId))
            {
                _birthdays.Remove(userId);

                string json = JsonSerializer.Serialize(_birthdays, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);

                Console.WriteLine($"🗑️ {message.Author.Username}'s birthday was removed.");
                await message.Channel.SendMessageAsync($"🗑️ {message.Author.Username}, your birthday has been removed.");
            }
            else
            {
                await message.Channel.SendMessageAsync($"⚠️ {message.Author.Username}, you don’t have a birthday saved yet. Use `/birthday MM-DD` to set it!");
            }
        }
    }






    //this method always runs and is checking for birthdays everyday at 9am
    private async Task StartDailyBirthdayCheck()
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            DateTime nextRun = now.Date.AddDays(1).AddHours(9); //runs at 9am


            TimeSpan delay = nextRun - now;
            Console.WriteLine($"⏰ Waiting until {nextRun} to check birthdays...");

            await Task.Delay(delay);

            await CheckBirthdays();
        }
    }








    private async Task CheckBirthdays()
    {

        string today = DateTime.Now.ToString("MM-dd"); //this formats the present days date as MM-DD


        //this loops through each saved birthday
        foreach (var kvp in _birthdays)
        {

            string userId = kvp.Key;
            string birthday = kvp.Value;

            if (birthday == today) //if the present day is a users birthday then the bot finds that user
            {
                SocketUser user = _client.GetUser(ulong.Parse(userId)); //it grabs the users ID here and says happy birthday and mentions everyone
                if (user != null)
                {
                    foreach (var guild in _client.Guilds)
                    {
                        SocketGuildUser guildUser = guild.GetUser(user.Id);

                        ulong birthdayChannelId = ulong.Parse(Env.GetString("BIRTHDAY_CHANNEL_ID")); //reads the .env file for the channel id that you enter
                        var channel = _client.GetChannel(birthdayChannelId) as IMessageChannel; //this is the harcoded so that i places the message in the channel i want.

                        if (guildUser != null)
                        {
                            var defaultChannel = guild.DefaultChannel; //i was using the discord default channel but is acts weird and post the message in randomplaces.
                            if (defaultChannel != null)
                            {
                                await channel.SendMessageAsync($"🎉 @everyone, Today is  {guildUser.Mention} birthday! Happy Birthday!! 🎉");
                            }
                        }
                    }
                }


            }


        }

    }
}