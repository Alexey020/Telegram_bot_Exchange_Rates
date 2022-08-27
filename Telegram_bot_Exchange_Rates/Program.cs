using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("your token");
using var cts = new CancellationTokenSource();
List<UserInfo> userInfos = new List<UserInfo>();


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

    if (!userInfos.Contains(new UserInfo(chatId)))
    {
        userInfos.Add(new UserInfo(chatId) { menuPoint = MenuPoint.start });
    }

    Console.WriteLine($"Resived '{messageText}' mesasage in chat {chatId}");

    Message sentMessage;
    
    Repeat:
    switch (userInfos.GetById(chatId).menuPoint)
    {
        case MenuPoint.start:

            userInfos.GetById(chatId).menuPoint =  MenuPoint.setCurrensy;
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Hi, " + message.From.FirstName + "! Here you can check exchange rates in few seconds\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] {new KeyboardButton[] { "SET CURRENCY"} }) { ResizeKeyboard = true}
                );
            
            break;
        case MenuPoint.main:
            if (messageText == "CHANGE CURRENCY")
                userInfos.GetById(chatId).menuPoint =  MenuPoint.setCurrensy;
            if (messageText == "CHACK RATE")
                userInfos.GetById(chatId).menuPoint = MenuPoint.showRate;

            if (userInfos.GetById(chatId).menuPoint != MenuPoint.main)
                goto Repeat;
            break;
        case MenuPoint.setCurrensy:
            userInfos.GetById(chatId).menuPoint = MenuPoint.choseCurrency;
            sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Chose youe carrency\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] {  "USD", "EUR", "GBR" } }) { ResizeKeyboard = true }
                );
            
            break;
        case MenuPoint.choseCurrency:
            if (messageText == "USD" || messageText == "EUR" || messageText == "GBR")
                userInfos.GetById(chatId).Currency = messageText;

            if (userInfos.GetById(chatId).Currency == messageText)
            {
                userInfos.GetById(chatId).menuPoint =  MenuPoint.main;
                sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "CHOSE NEXT ACTION\n",
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] { "CHACK RATE", "CHANGE CURRENCY" } }) { ResizeKeyboard = true }
                );
                
            }
            break;
        case MenuPoint.showRate:

            userInfos.GetById(chatId).menuPoint =  MenuPoint.main;
                sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: GetRates(userInfos.GetById(chatId).Currency),
                cancellationToken: cancellationToken,
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] { "CHACK RATE", "CHANGE CURRENCY" } }) { ResizeKeyboard = true }
                ) ;
            break; 
        default:
            if (messageText == "/start")
            {
                userInfos.GetById(chatId).menuPoint = MenuPoint.start;
            }
            break;
    }
   
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
class UserInfo
{
    public long Id { get; set; }
    public MenuPoint menuPoint { get; set; }
    public string Currency { get; set; }

    public UserInfo(long id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is UserInfo info &&
               Id == info.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}
static class ExprndingFuncs
{
public static UserInfo GetById(this List<UserInfo> usersI, long id) => usersI.Find(x => x.Id == id);
}