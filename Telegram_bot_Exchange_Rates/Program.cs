using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ServiceStack;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Text;

var botClient = new TelegramBotClient("token");
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
                replyMarkup: (ReplyKeyboardMarkup)new(new[] { new KeyboardButton[] {  "USD", "EUR", "GBP","UAH" } }) { ResizeKeyboard = true }
                );
            
            break;
        case MenuPoint.choseCurrency:
            if (messageText == "USD" || messageText == "EUR" || messageText == "GBP"|| messageText =="UAH")
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


 string GetRates(string UserCur)
{
    string shootStr =
    "https://docs.google.com/spreadsheets/d/e/2PACX-1vRoFIaxhE_wRPEHGcpN5HNB1hdw5EVAFOOpERucK6oNi0tWliFwPVNA9iGSLNbVygNWaUJY4lnGqWDV/pubhtml".GetStringFromUrl();

    var html = new HtmlDocument();
    html.LoadHtml(shootStr);

    var document = html.DocumentNode;


    var pars = document.QuerySelectorAll(".s0");
    var values = document.QuerySelectorAll(".s1");
    int i = 1;

    StringBuilder para = new StringBuilder();
    StringBuilder vals = new StringBuilder();

    foreach (var cur in pars)
    {
        para.Append(cur.InnerText);
        if (i % 2 == 0)
            para.Append('|');
        else
            para.Append('/');
        i++;
    }
    foreach (var val in values)
    {
        vals.Append(val.InnerText);
        vals.Append('|');
    }


    Dictionary<string, string> rates = new Dictionary<string, string>();
    var exchCur = para.ToString().Split('|');
    var curRates = vals.ToString().Split('|');

    i = 0;
    while (i < exchCur.Length-1)
    {
        rates.Add(exchCur[i], curRates[i]);
        i++;
    }
    para.Clear();
    string responce = "";
    string str;//useless thing.
    foreach (var item in rates)
    {
        str = item.Key;
        if (str.Split('/')[1] == UserCur)
        {
            responce += item.Key + " - " + ((item.Value.Length >7)?item.Value.Remove(7): item.Value) + "\n";
        }
    }


    return responce;
}


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

