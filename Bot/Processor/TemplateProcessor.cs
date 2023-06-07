using Google.Protobuf.WellKnownTypes;
using LisaCore.Bot.Conversations;
using LisaCore.Bot.Elements;
using LisaCore.Bot.Functions;
using LisaCore.Bot.Tasks;
using LisaCore.Bot.Users;
using LisaCore.Extensions;
using LisaCore.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LisaCore.Bot.Processor
{
    internal class TemplateProcessor
    {

        private readonly List<string> _gossips = new List<string>();
        private readonly Chat _aimlBot;

        private string _previousOutput = string.Empty;
        private readonly int _maxRecursionDepth;
        private int _currentRecursionDepth;
        private readonly ILogger? _logger;

        public TemplateProcessor(Chat aimlBot, int maxRecursionDepth = 10, ILogger? logger = null)
        {
            _logger = logger;
            _aimlBot = aimlBot;

            _maxRecursionDepth = maxRecursionDepth;
            _currentRecursionDepth = 0;
        }

        public async Task<Response?> ProcessAsync(UserManager userManager, XElement template, Query query, int? recursionDepth = null)
        {
            if (template == null) return null;
            var input = query.Input.ToUpperInvariant();
            var match = Regex.Match(input, GetPatternFromKey(template), RegexOptions.IgnoreCase);
            var output = await ProcessTemplateAsync(userManager, template.Element("template"), match, query, recursionDepth);
            _previousOutput = output.Message;
            output.Template = template;
            output.Match = match;
            return output;
        }

        private async Task<Response?> ProcessTemplateAsync(UserManager userManager, XElement template, Match match, Query query, int? recursionDepth = null)
        {
            if (template == null) return null;
            if (recursionDepth == null) recursionDepth = _maxRecursionDepth;
            if (_currentRecursionDepth >= recursionDepth) return null; // Add this line to prevent infinite recursion

            _currentRecursionDepth++; // Increment the recursion depth

            Response output = new();
            foreach (var node in template.Nodes())
            {
                switch (node)
                {
                    case XElement element when element.Name == "action":

                        var actionId = element.Attribute("ID")?.Value;
                        if (!string.IsNullOrEmpty(actionId))
                        {
                            // Replace Context and Result with the appropriate types for your application
                            //var context = new Context();
                            //var result = new Result();

                            //_actions.ExecuteAction(actionId, context, result);
                        }
                        break;
                    case XElement element when element.Name == "star":
                        var index = int.TryParse(element.Attribute("index")?.Value, out var i) ? i : 1;
                        output.Message = match.Groups.Count > index ? match.Groups[index].Value : string.Empty;
                        break;
                    case XElement element when element.Name == "that":
                        output.Message += _previousOutput;
                        break;

                    //Srai is redirect the processing of an AIML pattern to another pattern.
                    //The acronym SRAI stands for Symbolic Reduction Algorithm Invocation.
                    case XElement element when element.Name == "srai":

                        var sraiInput = element.Value.ToUpperInvariant();
                        if (sraiInput == "LIST ALL REMINDERS")
                        {
                            var tasks = await _aimlBot.TaskReminderService.ListAllTasksAsync(userManager.UserId);
                            output.Message += $"You have {tasks.Count} tasks.";
                            foreach (var t in tasks)
                            {
                                output.Message += t.ToString();
                            }
                        }
                        else if (sraiInput == "GET TOTAL TASKS")
                        {
                            output.Message += "0"; // await _aimlBot.TaskReminderService.GetTotalTasksAsync(_aimlBot.UserId);
                        }
                        else if (sraiInput == "GET OVERDUE TASKS")
                        {
                            output.Message += "0"; // await _aimlBot.TaskReminderService.GetOverdueTasksAsync(_aimlBot.UserId);
                        }
                        else if (sraiInput == "GET OPEN TASKS")
                        {
                            output.Message += "0"; // await _aimlBot.TaskReminderService.GetOpenTasksAsync(_aimlBot.UserId);
                        }
                        else if (sraiInput == "GET RECENTLY CLOSED TASKS")
                        {
                            output.Message += "0"; // await _aimlBot.TaskReminderService.GetRecentlyClosedTasksAsync(_aimlBot.UserId);
                        }
                        else if (sraiInput == "GET IN PROGRESS TASKS")
                        {
                            output.Message += "0"; // await _aimlBot.TaskReminderService.GetInProgressTasksAsync(_aimlBot.UserId);
                        }
                        else
                        {
                            Query sraiQuery = new Query();
                            sraiQuery.Id = query.Id;
                            sraiQuery.Input = sraiInput;
                            sraiQuery.IsSraiInput = true;
                            sraiQuery.Timestamp = query.Timestamp;
                            sraiQuery.ConversationId = query.ConversationId;

                            var result = await _aimlBot.GetResponseAsync(userManager.UserId, sraiQuery);
                            output.Message += result.Message;
                            foreach (var action in result.Actions)
                            {
                                output.Actions.Add(action);
                            }
                        }
                        break;
                    case XElement element when element.Name == "set":
                        var setName = element.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(setName))
                        {
                            var result = await ProcessTemplateAsync(userManager, element, match, query);
                            userManager.SetVariableAsync(setName, result.Message);
                            output.Message += result.Message;
                        }
                        break;
                    case XElement element when element.Name == "get":
                        var getName = element.Attribute("name")?.Value;
                        if (!string.IsNullOrEmpty(getName))
                        {
                            string variableValue = await userManager.GetVariableAsync(getName) ?? "unknown";
                            output.Message += variableValue;
                        }
                        break;
                    case XElement element when element.Name == "think":
                        var setElements = element.Elements("set").ToList();
                        string? task = null;
                        DateTime? time = null;

                        foreach (var setElement in setElements)
                        {
                            var setName1 = setElement.Attribute("name")?.Value;
                            if (setName1 == "reminder_task")
                            {
                                var result = await ProcessTemplateAsync(userManager, setElement, match, query);
                                task = result.Message.Replace("TASK: ", "").Trim();
                                time = TextRecognizer.ExtractDateTime(match.Value);
                                if (!time.HasValue) time = DateTime.UtcNow.AddDays(1);
                                if (!string.IsNullOrEmpty(task) && time.HasValue)
                                {
                                    try
                                    {
                                        if (time.Value.ToUniversalTime() < DateTime.UtcNow)
                                        {
                                            time = time.Value.AddDays(7);
                                        }
                                        await _aimlBot.TaskReminderService.AddTaskReminderAsync(task, time.Value, new() { userManager.UserId });
                                        //// Create a copy of the original template XElement
                                        //XElement templateCopy = new XElement(template);

                                        //// Remove the 'think' element from the copied template
                                        //templateCopy.Descendants("think").Remove();
                                        //output += await ProcessTemplateAsync(templateCopy, match);
                                    }
                                    catch (Exception ex) { }
                                }
                                else
                                {
                                    output.Message = "Seems like you want to set a reminder. Please let me know what you want me to remind you.";
                                    return output;
                                }

                            }

                        }

                        break;
                    case XElement element when element.Name == "random":
                        var listItems = element.Elements("li").ToList();
                        if (listItems.Count > 0)
                        {
                            var randomIndex = new Random().Next(listItems.Count);
                            var randomResult = await ProcessTemplateAsync(userManager, listItems[randomIndex], match, query);
                            output.Message += randomResult.Message;
                        }
                        break;
                    case XElement element when element.Name == "condition":
                        output.Message += await ProcessConditionAsync(userManager, element, match, query);

                        break;
                    case XElement element when element.Name == "gossip":
                        _gossips.Add(element.Value);
                        break;
                    case XElement element when element.Name == "person":
                        output.Message += StringTransforms.TransformPerson(element.Value);
                        break;
                    case XElement element when element.Name == "person2":
                        output.Message += StringTransforms.TransformPerson2(element.Value);
                        break;
                    case XElement element when element.Name == "gender":
                        output.Message += StringTransforms.TransformGender(element.Value);
                        break;
                    case XElement element when element.Name == "formal":
                        output.Message += StringTransforms.ToFormalCase(element.Value);
                        break;
                    case XElement element when element.Name == "uppercase":
                        output.Message += element.Value.ToUpperInvariant();
                        break;
                    case XText textNode:
                        output.Message += textNode.Value;
                        break;

                }
            }

            _currentRecursionDepth--; // Decrement the recursion depth
            return output;
        }


        private async Task<string> ProcessConditionAsync(UserManager userManager, XElement condition, Match match, Query query, int? recursionDepth = null)
        {
            var name = condition.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                string? value = null;
                if (name.Equals("Conditions.TimeOfDay"))
                {
                    value = Conditions.TimeOfDay();
                }
                else
                {

                    value = await userManager.GetVariableAsync(name);
                }


                if (value != null)
                {
                    XElement? listItem = null;
                    foreach (var li in condition.Elements("li"))
                    {
                        string conditionValue = li.Attribute("value")?.Value ?? "";
                        string operatorValue = "";
                        if (!string.IsNullOrEmpty(conditionValue))
                        {

                            var operators = new List<string> { "<=", ">=", "==", "<", ">" };
                            operatorValue = operators.FirstOrDefault(op => conditionValue.Contains(op));
                            conditionValue = conditionValue.Substring(operatorValue?.Length ?? 0);
                            bool comparisonResult = false;

                            if (double.TryParse(value, out var val) && double.TryParse(conditionValue, out var condVal))
                            {
                                // If the value and the condition value are both numbers, compare them as numbers
                                switch (operatorValue)
                                {
                                    case "<":
                                        comparisonResult = val < condVal;
                                        break;
                                    case "<=":
                                        comparisonResult = val <= condVal;
                                        break;
                                    case ">":
                                        comparisonResult = val > condVal;
                                        break;
                                    case ">=":
                                        comparisonResult = val >= condVal;
                                        break;
                                    case "==":
                                        comparisonResult = val == condVal;
                                        break;
                                }
                            }
                            else
                            {
                                comparisonResult = value.Equals(conditionValue, StringComparison.InvariantCultureIgnoreCase);

                            }
                            if (comparisonResult)
                            {
                                var result = await ProcessTemplateAsync(userManager, li, match, query, recursionDepth);
                                return result.Message;
                            }
                        }
                    }
                }
            }

            var defaultListItem = condition.Elements("li")
                .FirstOrDefault(li => li.Attribute("value") == null);

            return defaultListItem != null ? (await ProcessTemplateAsync(userManager, defaultListItem, match, query, recursionDepth))?.Message ?? "" : "";
        }


        private string GetPatternFromKey(XElement category)
        {
            var pattern = category.Element("pattern")?.Value.ToUpperInvariant();
            return Regex.Escape(pattern).Replace("\\*", "(.+)");
        }
    }
}
