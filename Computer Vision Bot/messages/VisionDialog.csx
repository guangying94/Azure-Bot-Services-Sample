using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;

public class ComputerVision
{
    public static async Task<Rootobject> GetImageJSON(string imageURL)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<Computer Vision API Key>");
        var uri = "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0/analyze?language=en&visualFeatures=Tags,Description";
        HttpResponseMessage response;
        byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageURL + "\"}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            Rootobject ImageJSON = JsonConvert.DeserializeObject<Rootobject>(result);
            return ImageJSON;
        }
    }

    public static async Task<string> getOcrResult(string imageURL)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<Computer Vision API Key>");
        var uri = "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0/ocr?language=unk&detectOrientation=true";
        HttpResponseMessage response;
        byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageURL + "\"}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            Rootobject2 ocrJSON = JsonConvert.DeserializeObject<Rootobject2>(result);
            List<string> scanOCR = new List<string>();
            string returnText = "";
            foreach (var region in ocrJSON.regions)
            {
                foreach (var line in region.lines)
                {
                    returnText = returnText + "[" + line.words[0].text + "] ";
                }
            }
            if (returnText == "")
            {
                returnText = "No text detected.";
            }
            else
            {
                returnText = returnText + " is detected.";
            }
            return returnText;
        }
    }
}

public class Tag
{
    public string name { get; set; }
    public double confidence { get; set; }
}

public class Caption
{
    public string text { get; set; }
    public double confidence { get; set; }
}

public class Description
{
    public List<string> tags { get; set; }
    public List<Caption> captions { get; set; }
}

public class Metadata
{
    public int width { get; set; }
    public int height { get; set; }
    public string format { get; set; }
}

public class Rootobject
{
    public List<Tag> tags { get; set; }
    public Description description { get; set; }
    public string requestId { get; set; }
    public Metadata metadata { get; set; }
}


// JSON for OCR
public class Rootobject2
{
    public string language { get; set; }
    public string orientation { get; set; }
    public Region[] regions { get; set; }
}

public class Region
{
    public string boundingBox { get; set; }
    public Line[] lines { get; set; }
}

public class Line
{
    public string boundingBox { get; set; }
    public Word[] words { get; set; }
}

public class Word
{
    public string boundingBox { get; set; }
    public string text { get; set; }
}

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var userURL = await argument;
        //OCR
        await context.PostAsync("Applying Computer Vision......");
        string detectedText = await ComputerVision.getOcrResult(userURL.Text);
        await context.PostAsync(detectedText);
        //Caption
        Rootobject JSON = await ComputerVision.GetImageJSON(userURL.Text);
        string captionStr = "Caption: " + JSON.description.captions[0].text;
        await context.PostAsync(captionStr);
        //ComputerVision
        string detectedObject = "Object detected: ";
        int tagCount = JSON.tags.Count();
        for(int i = 0; i < tagCount; i++)
        {
            detectedObject = detectedObject + " [" + JSON.tags[i].name + "]";
        }
        await context.PostAsync(detectedObject);
        context.Wait(MessageReceivedAsync);
    }

}