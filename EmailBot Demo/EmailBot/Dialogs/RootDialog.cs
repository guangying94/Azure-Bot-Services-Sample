using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Net.Mail;

namespace EmailBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            SmtpClient client = new SmtpClient("smtp.live.com", 25);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("<Sender Email>", "<Sender Password>");
            try
            {
                client.Send("<sender email>", "<recipient email>", "<subject>", activity.Text);
                await context.PostAsync("email sent!");
            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}