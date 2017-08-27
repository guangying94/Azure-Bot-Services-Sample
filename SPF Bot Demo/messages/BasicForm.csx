using System;
using Microsoft.Bot.Builder.FormFlow;
using System.Drawing;

public enum ReportOptions { CarAccident = 1, IllegalParking, PropertyLost, Others};
public enum SelfVictimOptions { Yes = 1, No };


// For more information about this template visit http://aka.ms/azurebots-csharp-form
// just some simple questions
[Serializable]
public class BasicForm
{

    [Prompt("What is your {&}?")]
    public string Nric { get; set; }


    [Prompt("Please select your report type {||}")]
    public ReportOptions Report { get; set; }

    [Prompt("Can you describe the {&} of this report?")]
    public string Detail { get; set; }
    

    public static IForm<BasicForm> BuildForm()
    {
        // Builds an IForm<T> based on BasicForm
        return new FormBuilder<BasicForm>().Build();
    }

    public static IFormDialog<BasicForm> BuildFormDialog(FormOptions options = FormOptions.PromptInStart)
    {
        // Generated a new FormDialog<T> based on IForm<BasicForm>
        return FormDialog.FromForm(BuildForm, options);
    }
}
