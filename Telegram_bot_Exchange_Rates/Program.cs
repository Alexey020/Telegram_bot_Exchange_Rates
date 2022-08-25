using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: BackResponse(messageText, userInfo), //$"You said:\n '{messageText}'",
        cancellationToken: cancellationToken);

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
    switch (message.ToLower())
    {
        case "hello":
            return "Hi," + from.FirstName;
            break;
        case "help":
            return "you shout messsage @"+from.Username;
            break;
        default:
            return "msg from default";
            break;
    }
       
   // return "";
}