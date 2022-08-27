using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("5355422609:AAGVFUA8bIup8ihBssW7y2weZst_jtm-Phs");
using var cts = new CancellationTokenSource();
//Dictionary<long, MenuPoint> userPoints = new Dictionary<long, MenuPoint>();
MenuPoint menuPoint = MenuPoint.start;


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
  /*  if (!userPoints.ContainsKey(chatId))
    {
        userPoints.Add(chatId, MenuPoint.start);
    }*/
    string userCurrency = default;

    Console.WriteLine($"Resived '{messageText}' mesasage in chat {chatId}");

    Message sentMessage;
    Repeat:
    switch (menuPoint)//(userPoints.GetValueOrDefault(chatId))
    {
        case MenuPoint.start:
            menuPoint = MenuPoint.setCurrensy;
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Hi, " + userInfo.FirstName + "! Here you can check exchange rates in few seconds\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] {new KeyboardButton[] { "SET CURRENCY"} }) { ResizeKeyboard = true}
                );
            
            break;
        case MenuPoint.main:
            if (messageText == "CHANGE CURRENCY")
                menuPoint = MenuPoint.setCurrensy;
            if (messageText == "CHACK RATE")
                menuPoint = MenuPoint.showRate;

            if (menuPoint != MenuPoint.main)
                goto Repeat;
            break;
        case MenuPoint.setCurrensy:
            menuPoint = MenuPoint.choseCurrency;
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Chose youe carrency\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] {  "USD", "EUR", "GBR" } }) { ResizeKeyboard = true }
                );
            
            break;
        case MenuPoint.choseCurrency:
            if (messageText == "USD" || messageText == "EUR" || messageText == "GBR")
                userCurrency = messageText;

            if (userCurrency == messageText)
            {
                menuPoint = MenuPoint.main;
                sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "CHOSE NEXT ACTION\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] { "CHACK RATE", "CHANGE CURRENCY" } }) { ResizeKeyboard = true }
                );
                
            }
            break;
        case MenuPoint.showRate:
           
                menuPoint = MenuPoint.main;
                sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: GetRates(userCurrency),
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] { "CHACK RATE", "CHANGE CURRENCY" } }) { ResizeKeyboard = true }
                ) ;
            menuPoint = MenuPoint.main;
            break; 
        default:
            break;
    }
   /* switch (messageText.ToLower())
    {
        case "/start":
           
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
    }*/

    //Log msg to @davayponovoy
    /*Message logAction = await botClient.SendTextMessageAsync(
        chatId: 470906072,
        text: "msg from: @"+userInfo.Username+" - "+ messageText,
        cancellationToken: cancellationToken);*/

}

string GetRates(string userCurrency)
{
    return "Your currency - " + userCurrency + "\n USD - 22,22\n GBR - 44,44\n EUR - 99,88";
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

// api kay ------           AiaHLF3rI5kPtkNDCDuOZT5TjYrkkJAF
enum MenuPoint
{
    start = 1,
    main,
    setCurrensy,
    choseCurrency,

    showRate
}