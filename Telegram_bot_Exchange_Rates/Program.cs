using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("5355422609:AAGVFUA8bIup8ihBssW7y2weZst_jtm-Phs");
using var cts = new CancellationTokenSource();

 MenuPoint menuPoint = MenuPoint.main;
string userCurrency = default;

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

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: BackResponse(messageText, userInfo), 
        cancellationToken: cancellationToken);

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
    // /start, /help, /setCurrentCurrency, /checkUsdRate, checkEurRate, CheckGbpRate
    string commandList = "Command list:\n /help\n /setCurrency\n /checkRate";
    

    if (menuPoint == MenuPoint.main)
    {
        switch (message.ToLower())
        {
            case "/start":
                return "Hi, " + from.FirstName + "! Here you can check exchange rates in few seconds\n" + commandList;
                break;
            case "/help":
            case "help":
                return "You should messsage @davayponovoy\n" + commandList;
                break;
            case "/checkrate":
                return GetRates(userCurrency);
                break;
            case "/setcurrency":
                menuPoint = MenuPoint.setCurrensy;
                return "Put 'USD', 'GBR', 'EUR' or code of other currency";
                break;
            default:
                return "unknown command";
                break;
        }
    }
    
    if (menuPoint == MenuPoint.setCurrensy)
    {
        if (message.Length == 3)
        {
            userCurrency = message.ToUpper();
            menuPoint = MenuPoint.main;
            return "Your currency: " + userCurrency;
        }
        else
        {
            return "Uncorrect format. Try again";
        }
    }

   /* if (menuPoint == MenuPoint.showRate)
    {
        menuPoint = MenuPoint.main;
        return userCurrency + " to USD = 9999" +
               userCurrency + " to GBR = 9999" +
               userCurrency + " to EUR = 9999";

    }*/

    return "";
}

string GetRates(string userCurrency)
{
    return userCurrency + " to USD = 9999\n" +
              userCurrency + " to GBR = 9999\n" +
              userCurrency + " to EUR = 9999\n";
}



// api kay ------           AiaHLF3rI5kPtkNDCDuOZT5TjYrkkJAF
enum MenuPoint
{
    main = 1,
    setCurrensy,
    showRate
}