using System;
using Microsoft.Bot.Builder.FormFlow;

public enum ComplainOptions {CustomerService = 1, SlowService, Others};

// For more information about this template visit http://aka.ms/azurebots-csharp-form
[Serializable]
public class BasicForm
{
    [Prompt("Hi! What is your {&}?")]
    public string Name { get; set; }
    
    //add 1 more questions
    [Prompt("What is your {&}?")]
    public int ContactNo { get; set; }
    
    //add choice
    [Prompt("Please select the type of complain {||}")]
    public ComplainOptions Complain { get; set; }
    
    //add details
    [Prompt("Can you tell me the {&}?")]
    public string Details { get; set; }
    

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
