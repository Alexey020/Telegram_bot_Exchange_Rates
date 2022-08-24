using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("5355422609:AAGVFUA8bIup8ihBssW7y2weZst_jtm-Phs");

using var cts = new CancellationTokenSource();
var reservingOption = new ReceiverOptions()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};


cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

   Console.WriteLine($"Resived '{messageText}' mesasage in chat {chatId}");

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text:$"You said:\n '{messageText}'",
        cancellationToken: cancellationToken);

}