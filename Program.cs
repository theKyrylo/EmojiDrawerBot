using Telegram.Bot;
string TOKEN = File.ReadAllText(@".\TOKEN.txt");
        
var botClient = new TelegramBotClient(TOKEN);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");