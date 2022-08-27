using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("5355422609:AAGVFUA8bIup8ihBssW7y2weZst_jtm-Phs");
using var cts = new CancellationTokenSource();


var receiverOptions = new ReceiverOptions()
{
    AllowedUpdates = Array.Empty<UpdateType>()
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
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    var userInfo = message.From;

    Console.WriteLine($"Resived '{messageText}' mesasage in chat {chatId}");

    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
    {
        new KeyboardButton[] { "Set currency", "Show rates","svodka" },
    })
    {
        ResizeKeyboard = true
    };

    Message sentMessage;

    switch (messageText.ToLower())
    {
        case "/start":
            sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Hi, " + userInfo.FirstName + "! Here you can check exchange rates in few seconds\n",
        cancellationToken: cancellationToken,
        replyMarkup: replyKeyboardMarkup
        );
            break;
        case "/help":
        case "help":
          //  return "You should messsage @davayponovoy\n";
            break;
        case "show rates":
            sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Your carruncy - XXX" + "\nUSD/XXX - 99.99" + "\nEUR/XXX - 99.99" + "\nGBP/XXX - 99.99",
        cancellationToken: cancellationToken,
        replyMarkup: replyKeyboardMarkup
        );
            break;
        case "set currency":
            sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Put 'USD', 'GBR', 'EUR' or code of other currency",
        cancellationToken: cancellationToken,
        replyMarkup: replyKeyboardMarkup
        );
            break;
        case "svodka":
            sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "svodka,",
        cancellationToken: cancellationToken,
        replyMarkup: replyKeyboardMarkup
        );
            break;
        default:
            
            break;
    }

    //Log msg to @davayponovoy
    /*Message logAction = await botClient.SendTextMessageAsync(
        chatId: 470906072,
        text: "msg from: @"+userInfo.Username+" - "+ messageText,
        cancellationToken: cancellationToken);*/

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


string BackResponse(string message, User from)
{
    // /start, /help, /setCurrentCurrency, /checkRate
    return "fd";
     
           
}

// api kay ------           AiaHLF3rI5kPtkNDCDuOZT5TjYrkkJAF
