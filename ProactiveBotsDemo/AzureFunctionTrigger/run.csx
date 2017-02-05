#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;


public class BotMessage
{
    public string Source { get; set; } 
    public string Message { get; set; }
}

public class Address
{
    public string BotId { get; set; }
    public string ChannelId { get; set; } 
    public string UserId { get; set; }
    public string ConversationId { get; set; }
    public string ServiceUrl { get; set; }
}

public class ResumptionCookie
{
    public Address Address { get; set; }
    public string UserName { get; set; }
    public bool IsTrustedServiceUrl { get; set; }
    public bool IsGroup { get; set; }
    public string Locale { get; set; }
}

public class Example
{
    public ResumptionCookie ResumptionCookie { get; set; }
    public string Text { get; set; }
}

public class valueItem
{ 
	 public string Type { get; set; } 
	 public float Latitude { get; set; } 
	 public float Longitude { get; set; } 
	 public string Message { get; set; } 
}

public class RootContract
{ 
	 public string odatametadata { get; set; } 
	 public List<valueItem> value { get; set; } 
}

public class GetAccidentData
{
    public static async Task<string> GetAccidentMessage()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("AccountKey","Y6eiSngkQb2s7a4ZxykDsw==");
        string requestURL = "http://datamall2.mytransport.sg/ltaodataservice/TrafficIncidents";
        var response = await http.GetAsync(requestURL);
        var result = await response.Content.ReadAsStringAsync();
        RootContract accidentJSON = JsonConvert.DeserializeObject<RootContract>(result);
        return accidentJSON.value[0].Message;
    }
}


public static async Task<BotMessage> Run(string myQueueItem, TraceWriter log)
{
    log.Info($"Sending Bot message {myQueueItem}");  

    //Deserialize the JSON received from the conversation
    var modifiedMessage = JsonConvert.DeserializeObject<Example>(myQueueItem);
    //Declare the accident messages for current and latest
    //Will return when latest message is not same as current message
    string currentMsg = await GetAccidentData.GetAccidentMessage();
    string latestMsg = await GetAccidentData.GetAccidentMessage();
    
    //Do checking here
    //function can only run at most 5 minutes, so we check every 30 seconds
    //the loop will break if there's no update
    int round = 0;
    while (currentMsg == latestMsg && round < 9)
    {
        await Task.Delay(30000);
        latestMsg = await GetAccidentData.GetAccidentMessage();
        round = round + 1;
    }

    //Condition if no new update
    //we update users no new accident
    if (currentMsg == latestMsg)
    {
        latestMsg = "There's no new accident happened in the past 5 minutes.";
    }

    //change the return message
    //this will send a JSON object back to the conversation
    modifiedMessage.Text = latestMsg;
    string sendMessage = JsonConvert.SerializeObject(modifiedMessage);

    //create reply message
    BotMessage message = new BotMessage();
    message.Source = "Azure Functions (C#)!"; 
    message.Message = sendMessage;

    return message;
}
