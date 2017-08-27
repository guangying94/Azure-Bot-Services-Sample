#load "BasicForm.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using System.Net.Http.Headers;
using System.Web;
using System.Text;
using System.Globalization;
using System.Net;
using System.IO;


// JSON object for camera images
public class Rootobject
{
    public string odatametadata { get; set; }
    public Value[] value { get; set; }
}

public class Value
{
    public string CameraID { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string ImageLink { get; set; }
}

// function to get traffic images from LTA data mall api
public class GetTrafficImages
{
    public async static Task<Rootobject> GetImage()
    {
        var http = new HttpClient();
        //replace the "api key" with your api key
        http.DefaultRequestHeaders.Add("AccountKey", "<Data mall API>");
        var response = await http.GetAsync("http://datamall2.mytransport.sg/ltaodataservice/Traffic-Images");
        var result = await response.Content.ReadAsStringAsync();
        Rootobject TrafficImages = JsonConvert.DeserializeObject<Rootobject>(result);
        return TrafficImages;
    }
}

// JSON objects for sentiment analysis
public class Document
{
    public double score { get; set; }
    public string id { get; set; }
}

public class Rootobject2
{
    public IList<Document> documents { get; set; }
    public IList<object> errors { get; set; }
}

// function to use sentiment analysis
public class SentimentAnalysis
{
    private const string baseURL = "https://westus.api.cognitive.microsoft.com/";
    private const string AccountKey = "<API Key for cognitive services text analytics>";

    public static async Task<double> MakeRequests(string input)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AccountKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            byte[] byteData = Encoding.UTF8.GetBytes("{\"documents\":[" +
            "{\"id\":\"1\",\"text\":\"" + input + "\"},]}");
            var uri = "text/analytics/v2.0/sentiment";
            var response = await CallEndpoint(client, uri, byteData);
            return response.documents[0].score;
        }
    }

    public static async Task<Rootobject2> CallEndpoint(HttpClient client, string uri, byte[] byteData)
    {
        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            Rootobject2 sentimentJSON = JsonConvert.DeserializeObject<Rootobject2>(result);
            return sentimentJSON;
        }
    }
}

// Computer vision
public class Rootobject3
{
    public Category[] categories { get; set; }
    public Description description { get; set; }
    public string requestId { get; set; }
    public Metadata metadata { get; set; }
}

public class Description
{
    public string[] tags { get; set; }
    public Caption[] captions { get; set; }
}

public class Caption
{
    public string text { get; set; }
    public float confidence { get; set; }
}

public class Metadata
{
    public int width { get; set; }
    public int height { get; set; }
    public string format { get; set; }
}

public class Category
{
    public string name { get; set; }
    public float score { get; set; }
}



public class Rootobject4
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
    
public class ComputerVision
{
    public static async Task<Rootobject3> GetImageJSON(string imageURL)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<Cognitive Services Computr Vision API>");
        var uri = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Tags,Description&language=en";
        HttpResponseMessage response;
        byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageURL + "\"}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            Rootobject3 ImageJSON = JsonConvert.DeserializeObject<Rootobject3>(result);
            return ImageJSON;
        }
    }

    public static async Task<string> getOcrResult(string imageURL)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<Cognitive Services API Key>");
        var uri = "https://api.projectoxford.ai/vision/v1.0/ocr?language=unk&detectOrientation=true";
        HttpResponseMessage response;
        byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageURL + "\"}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(uri, content);
            var result = await response.Content.ReadAsStringAsync();
            Rootobject4 ocrJSON = JsonConvert.DeserializeObject<Rootobject4>(result);
            List<string> scanOCR = new List<string>();
            string returnText = "";
            int maxRegion = ocrJSON.regions[0].lines.Count();
            for (int i = 0; i < maxRegion; i++)
            {
                scanOCR.Add(ocrJSON.regions[0].lines[i].words[0].text);
                returnText = returnText + " " + ocrJSON.regions[0].lines[i].words[0].text;
            }

            if (returnText == "")
            {
                returnText = "Sorry, I can't detect any text.";
            }
            else
            {
                returnText = returnText + " is detected.";
            }
            return returnText;
        }
    }
}

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
    public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        Double testResponse = await SentimentAnalysis.MakeRequests(result.Query);
        if (testResponse < 0.5)
        {
            await context.PostAsync("Sorry! Let me connect you with an agent immediately.");
        }
        else
        {
            await context.PostAsync("Hey! Sorry, I don't understand what you are saying, can you try ask me other stuff?");
        }
        context.Wait(MessageReceived);
    }

    // Greeting and prompt options to select service
    [LuisIntent("Greeting")]
    public async Task Greeting(IDialogContext context, LuisResult result)
    {
        await context.PostAsync("Hello there!");
        List<string> options = new List<string> { "view camera image", "report illegal parking", "make police report" };
        PromptDialog.Choice(
            context,
            chooseServiceAsync,
            options,
            "Please select a service",
            "Try again"

        );
    }

    // view camera image from data mall
    [LuisIntent("cameraImage")]
    public async Task cameraImage(IDialogContext context, LuisResult result)
    {
        Rootobject Traffic = await GetTrafficImages.GetImage();
        string cameraID = "";
        cameraID = result.Entities[0].Entity;
        // this store the value for future conversation
        context.ConversationData.SetValue<string>("LastCamera", cameraID);
        int cameraNo = 0;
        int maxCameraNo = Traffic.value.Count() - 1;
        while (cameraID != Traffic.value[cameraNo].CameraID)
        {
            if (cameraNo == maxCameraNo)
                break;
            else
                cameraNo += 1;
        }

        if (cameraNo == maxCameraNo)
        {
            await context.PostAsync("I can't find this camera.");
            context.Wait(MessageReceived);
        }
        else
        {
            string cameraImageURL = Traffic.value[cameraNo].ImageLink;
            var replyTraffic = context.MakeMessage();
            replyTraffic.Attachments = new List<Attachment>()
            {
                new Attachment()
                {
                    ContentUrl = cameraImageURL,
                    ContentType = "image/png",
                    Name = "Camera.png"
                }
            };
            await context.PostAsync(replyTraffic);
            context.ConversationData.SetValue<int>("LastCameraNo", cameraNo);
            context.Wait(MessageReceived);
        }
    }

    // Get information on how to get camera images
    [LuisIntent("GetInfo")]
    public async Task GetInfo(IDialogContext context, LuisResult result)
    {
        await context.PostAsync("I can show you images of live traffic conditions along expressways and Woodlands & Tuas Checkpoints. Try asking me like 'show me images of camera 1001'!");
        await context.PostAsync("Oh ya, I can show you where is the camere as well!");
        context.Wait(MessageReceived);
    }

    // get location of police station
    [LuisIntent("GetLocation")]
    public async Task GetLocation(IDialogContext context, LuisResult result)
    {
        Rootobject Traffic = await GetTrafficImages.GetImage();
        string cameraID = "";
        cameraID = result.Entities[0].Entity;
        // similarly, here the value is stored
        context.ConversationData.SetValue<string>("LastCamera", cameraID);
        int cameraNo = 0;
        int maxCameraNo = Traffic.value.Count() - 1;
        while (cameraID != Traffic.value[cameraNo].CameraID)
        {
            if (cameraNo == maxCameraNo)
                break;
            else
                cameraNo += 1;
        }
        if (cameraNo == maxCameraNo)
        {
            await context.PostAsync("I can't find this camera.");
            context.Wait(MessageReceived);
        }
        else
        {
            string cameraLat = Traffic.value[cameraNo].Latitude.ToString();
            string cameraLong = Traffic.value[cameraNo].Longitude.ToString();
            string bingMapKey = "<Bing Map API Key>";
            string mapURL = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/" + cameraLat + "," + cameraLong + "/14?mapSize=280,140&pp=" + cameraLat + "," + cameraLong + "&key=" + bingMapKey;
            var replyTrafficMap = context.MakeMessage();
            replyTrafficMap.Attachments = new List<Attachment>()
        {
            new Attachment()
            {
                ContentUrl = mapURL,
                ContentType = "image/png",
                Name = "map.png"
            }
        };
            await context.PostAsync(replyTrafficMap);
            context.Wait(MessageReceived);
        }
    }

    // previously the value is stored
    // here we can recall the value
    [LuisIntent("RepeatLastCamera")]
    public async Task RepeatLastCamera(IDialogContext context, LuisResult result)
    {
        string responseRepeat = "";
        string lastCameraLocation = string.Empty;
        // if there's no value stored
        if (!context.ConversationData.TryGetValue("LastCamera", out lastCameraLocation))
        {
            responseRepeat = "You haven't tell me the previous camera number.";
        }
        else
        {
            Rootobject Traffic = await GetTrafficImages.GetImage();
            int cameraNo = 0;
            int maxCameraNo = Traffic.value.Count() - 1;

            while (lastCameraLocation != Traffic.value[cameraNo].CameraID)
            {
                if (cameraNo == maxCameraNo)
                    break;
                else
                    cameraNo += 1;
            }
            string cameraLat = Traffic.value[cameraNo].Latitude.ToString();
            string cameraLong = Traffic.value[cameraNo].Longitude.ToString();
            string bingMapKey = "<Bing map API Key>";
            string mapURL = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/" + cameraLat + "," + cameraLong + "/14?mapSize=280,140&pp=" + cameraLat + "," + cameraLong + "&key=" + bingMapKey;
            var replyTrafficMap = context.MakeMessage();
            replyTrafficMap.Attachments = new List<Attachment>()
        {
            new Attachment()
            {
                ContentUrl = mapURL,
                ContentType = "image/png",
                Name = "map.png"
            }
        };
            await context.PostAsync(replyTrafficMap);

            responseRepeat = "This is the location of camera " + lastCameraLocation + ".";

        }

        await context.PostAsync(responseRepeat);
        context.Wait(MessageReceived);
    }

    // Lodge a police report for illegal parking
    // here it will prompt the user to upload an image for verificatoin
    // Computer vision will be applied here
    // if car is detected, then OCR will be applied
    [LuisIntent("PoliceReport")]
    public async Task PoliceReport(IDialogContext context, LuisResult result)
    {
        try
        {
            await context.PostAsync("You will need to upload an image for verification purpose.");
            PromptDialog.Attachment(
            context,
            AfterImageAsync,
            "Please upload an image."
            );
        }
        catch (Exception)
        {
            await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
            context.Wait(MessageReceived);
        }
    }

    // This chatbot integrate will form flow template
    private async Task BasicFormComplete(IDialogContext context, IAwaitable<BasicForm> result)
    {
        try
        {
            var feedback = await result;
            await context.PostAsync("Thanks for reporting to us, we will take necessary action.");
        }
        catch (FormCanceledException)
        {
            await context.PostAsync("Don't want to report anymore? It's okay.");
        }
        catch (Exception)
        {
            await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
        }
        finally
        {
            context.Wait(MessageReceived);
        }
    }

    // function to do computer vision
    // if no car is detected, then it will reply with caption
    // if car is detected, then OCR will be applied to detect the car plate
    public async Task AfterImageAsync(IDialogContext context, IAwaitable<IEnumerable<Attachment>> argument)
    {
        var uploadimage = await argument;
        if (uploadimage != null)
        {
            string firstItem = uploadimage.Last().ContentUrl;
            Rootobject3 ImageJSON = await ComputerVision.GetImageJSON(firstItem);
            bool check = ImageJSON.description.tags.Contains("car");
            if (check)
            {
                await context.PostAsync("I think I saw " + ImageJSON.description.captions[0].text);
                string carPlate = await ComputerVision.getOcrResult(firstItem);
                await context.PostAsync(carPlate);
                await context.PostAsync("Thank you for reporting to us.");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("Sorry, I think I saw " + ImageJSON.description.captions[0].text + ". This is an invalid photo.");
                context.Wait(MessageReceived);
            }

        }
        else
        {
            await context.PostAsync("No image received.");
            context.Wait(MessageReceived);
        }

    }

    // function to choose which service to use
    public async Task chooseServiceAsync(IDialogContext context, IAwaitable<string> argument)
    {
        string chosenChoice = await argument;
        switch (chosenChoice)
        {
            case "view camera image":
                {
                    await context.PostAsync("You can seee the camera images by asking me \"show me image of camera 2701\" :)");
                    context.Wait(MessageReceived);
                    break;
                }
            case "report illegal parking":
                {
                    await context.PostAsync("You will need to upload an image for verification purpose.");
                    PromptDialog.Attachment(
                    context,
                    AfterResetAsync,
                    "Please upload an image."
                    );
                    break;
                }
            case "make police report":
                {
                    await context.PostAsync("I will need some details to proceed.");
                    // this will trigger form flow
                    // refer to BasicForm.csx
                    context.Call(BasicForm.BuildFormDialog(FormOptions.PromptInStart), BasicFormComplete);
                    break;
                }
            default:
                {
                    await context.PostAsync("Sorry, I didn't get that.");
                    break;
                }
        }
    }
}