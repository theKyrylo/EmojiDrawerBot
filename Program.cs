using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;

// HERE USE YOUR OWN TELEGRAM BOT TOKEN!!!
string TOKEN = System.IO.File.ReadAllText("./TOKEN.txt");
var botClient = new TelegramBotClient(TOKEN);

using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    //Console.WriteLine(CreateEmojiedMessage(messageText));
    try{
    // Echo received message text
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: CreateEmojiedMessage(messageText),
        cancellationToken: cancellationToken);
    }
    catch{
        Console.WriteLine("ERROR");
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

// Function to convert text messages to
// messages with some words changed by emojis
string CreateEmojiedMessage(string message)
{
    List<string> words = new List<string>(message.Split(" "));
    for (int i = 0; i < words.Count; i++)
    {
        // USE YOUR OPEN EMOJI API TOKEN HERE
        string emoji = SearchEmoji(words[i], System.IO.File.ReadAllText("./TOKENemoji.txt"));
        if (emoji != "")
            words[i] = emoji;
    }
    return string.Join(" ", words);
}

// Function that is doing an http get from Open Emoji API
// to search an emoji that is describes the searchWord.
// You MUST have Open Emoji API access key, that you can get here:
// https://emoji-api.com/
string SearchEmoji(string searchWord, string TOKEN)
{
    using(var client = new HttpClient())
    {
        var endpoint = new Uri($"https://emoji-api.com/emojis?search={searchWord}&access_key={TOKEN}");
        var result = client.GetAsync(endpoint).Result;
        var json = result.Content.ReadAsStringAsync().Result;
        var unicode = json.Substring(json.IndexOf("character")+12, json.IndexOf("\",\"unicodeName")-(json.IndexOf("character")+12));
        return Regex.Unescape(unicode);
    }
}