using System;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;

using System.Net;
using System.Net.Http;
using System.Web;

public class Rates
{
    public double AUD { get; set; }
    public double BGN { get; set; }
    public double BRL { get; set; }
    public double CAD { get; set; }
    public double CHF { get; set; }
    public double CNY { get; set; }
    public double CZK { get; set; }
    public double DKK { get; set; }
    public double GBP { get; set; }
    public double HKD { get; set; }
    public double HRK { get; set; }
    public double HUF { get; set; }
    public double IDR { get; set; }
    public double ILS { get; set; }
    public double INR { get; set; }
    public double JPY { get; set; }
    public double KRW { get; set; }
    public double MXN { get; set; }
    public double MYR { get; set; }
    public double NOK { get; set; }
    public double NZD { get; set; }
    public double PHP { get; set; }
    public double PLN { get; set; }
    public double RON { get; set; }
    public double RUB { get; set; }
    public double SEK { get; set; }
    public double SGD { get; set; }
    public double THB { get; set; }
    public double TRY { get; set; }
    public double USD { get; set; }
    public double ZAR { get; set; }
}

public class Fixer
{
    public string baseA { get; set; }
    public string date { get; set; }
    public Rates rates { get; set; }
}

public class GetRate
{
    public async static Task<Fixer> GetRateData()
    {
        var http = new HttpClient();
        string requestURL = "http://api.fixer.io/latest?base=EUR";
        var response = await http.GetAsync(requestURL);
        var result = await response.Content.ReadAsStringAsync();
        Fixer exchangeJSON = JsonConvert.DeserializeObject<Fixer>(result);
        return exchangeJSON;
    }
}

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
    protected string baseConvert = "";
    protected string[] currencyLib = { "AUD", "BGN", "BRL", "CAD", "CHF", "CNY", "CZK", "DKK", "GBP", "HKD", "HRK", "HUF", "IDR", "ILS", "INR", "JPY", "KRW", "MXN", "MYR", "NOK", "NZD", "PHP", "PLN", "RON", "RUB", "SEK", "SGD", "THB", "TRY", "USD", "ZAR", "EUR" };

    public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
        context.Wait(MessageReceived);
    }


    [LuisIntent("Greeting")]
    public async Task Greeting(IDialogContext context, LuisResult result)
    {
        await context.PostAsync("Hi there! I'm currency exchange bot!"); //
        context.Wait(MessageReceived);
    }

    [LuisIntent("BaseCurrency")]
    public async Task BaseCurrency(IDialogContext context, LuisResult result)
    {
        string enteredCur = result.Entities[0].Entity.ToUpper();
        foreach (string x in currencyLib)
        {
            if (x.Equals(enteredCur))
            {
                this.baseConvert = enteredCur;
                await context.PostAsync("Alright, " + baseConvert + " is registed as base currency.");
            }
            else
            {
                await context.PostAsync("Sorry, I can't find " + enteredCur + " in my library at the moment.");
            }
        }
        context.Wait(MessageReceived);
    }

    [LuisIntent("ConvertToBase")]
    public async Task ConvertToBase(IDialogContext context, LuisResult result)
    {
        //string targetCur = result.Entities[0].Entity.ToUpper();
        //double targetAmount = result.Entities[1].Entity;
        double calculatedResult  = 0;
        if(this.baseConvert == "")
        {
            await context.PostAsync("Please set your base currency first!");
        }
        else
        {
            Fixer exchangedata = await GetRate.GetRateData();
            calculatedResult = exchangedata.rates.MYR / exchangedata.rates.SGD;
            await context.PostAsync("Current exchange rate is " + calculatedResult.ToString());
        }
        context.Wait(MessageReceived);
    }
}