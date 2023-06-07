using LisaCore.Bot.Enums;
using LisaCore.Bot.Processor;
using LisaCore.Bot.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LisaCore.Bot.Users;
using LisaCore.Bot.Learn;
using LisaCore.Bot.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using LisaCore.Bot.Conversations;
using Azure;
using Microsoft.EntityFrameworkCore.Internal;
using LisaCore.Nlp;
using System.IO;
using LisaCore.Nlp.BERT;
using Google.Protobuf.WellKnownTypes;

namespace LisaCore.Bot
{
    public class Chat
    {
        private readonly CategoryMatcher _categoryMatcher;
        private readonly TemplateProcessor _templateProcessor;
        private readonly ConcurrentDictionary<string, UserManager> _users;
        private readonly Learner _aimlLearner;
        private readonly TaskReminderService _taskReminderService;
        private readonly string _knowledgePath;
        private readonly ILogger? _logger;
        private readonly ConversationManager _conversationManager;
        private Bert? _bert = null;

        public Chat (string knowledgePath, int maxRecursionDepth = 10, ILogger? logger = null)
        {
            _knowledgePath = knowledgePath;
            _logger = logger;

            _categoryMatcher = new CategoryMatcher(_logger);

            //Users
            _users = new ConcurrentDictionary<string, UserManager>();
            string userDbDirectory = Path.Combine(_knowledgePath, "Users");
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(userDbDirectory);
            var user = new UserManager("system", userDbDirectory, _logger);
            if (!_users.ContainsKey(user.UserId)) { _users.TryAdd(user.UserId, user); }
            else { _users[user.UserId] = user; }

            //processor
            _templateProcessor = new TemplateProcessor(this, maxRecursionDepth, _logger);

            //learning
            var learnDirectory = Path.Combine(knowledgePath, "Learned");
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(learnDirectory);
            _aimlLearner = new Learner(learnDirectory, _logger);

            //load knowledage
            _categoryMatcher.LoadSystemKnowledge();
            _categoryMatcher.LoadTrainedKnowledge(_knowledgePath);
            _categoryMatcher.LoadLearnedKnowledge(_aimlLearner.Directory);

            _taskReminderService = new TaskReminderService(knowledgePath);
            _taskReminderService.TaskReminderNotification += (sender, e) => OnTaskReminderNotification(e);

            //converstation
            var converstationPath = Path.Combine(_knowledgePath, "Converstations");
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(converstationPath);
            _conversationManager = new ConversationManager(converstationPath);


        }
        
        internal ConversationManager ConversationManager { get { return _conversationManager; } }

        internal TaskReminderService TaskReminderService { get { return _taskReminderService; } }

        internal Bert? Bert { get { return _bert; } }

        internal void SetBert(Bert bert)
        {
            _bert = bert;
        }

        internal void SetNlpContext(Dictionary<int, string> context)
        {
            _categoryMatcher.SetNlpConext(context);
        }

        #region Tasks
        public event EventHandler<TaskReminderEventArgs> TaskReminderNotification;

        protected virtual void OnTaskReminderNotification(TaskReminderEventArgs e)
        {
            TaskReminderNotification?.Invoke(this, e);
        }
        #endregion

        internal async Task<Response?> GetResponseAsync(string userId, Query query, int? recursionDepth = null)
        {
            Conversation? conversation = _conversationManager.GetConversation(userId, query.ConversationId);
            if (conversation == null)
            {
                conversation = new();
                query.ConversationId = conversation.Id;
                _conversationManager.UpdateOrInsertConversation(userId, conversation);
            }

            if (!query.IsSraiInput)
            { //Srai Input is redirect the processing of an AIML pattern to another pattern. The acronym SRAI stands for Symbolic Reduction Algorithm Invocation.
                //Ignore update because it is not the origional query but have origional query id.
                _conversationManager.UpdateOrInsertQuery(userId, query);
            }

            Result result = new Result();
            result.QueryId = query.Id;

            #region CheckInput
            UserManager? user = null;
            if (!_users.TryGetValue(userId, out user) || !_users.ContainsKey(userId))
            {
                user = new UserManager(userId, _knowledgePath);
                if (!_users.ContainsKey(user.UserId)) { _users.TryAdd(user.UserId, user); }
            }


            if (user == null) {
               
                
                result.Message = $"Unable to intilize user [{userId}].";
                if (string.IsNullOrEmpty(conversation.Name))
                {
                    conversation.Name = result.Message;

                    _conversationManager.UpdateOrInsertConversation(userId, conversation);
                }
                
                _conversationManager.UpdateOrInsertResult(userId, result);
                return new() { IsError = true, Message = result.Message }; 
            }

            if (string.IsNullOrWhiteSpace(query.Input))
            {
               
                result.Message = $"I can't process an empty input.";
                if (string.IsNullOrEmpty(conversation.Name))
                {
                    conversation.Name = result.Message;

                    _conversationManager.UpdateOrInsertConversation(userId, conversation);
                }

                _conversationManager.UpdateOrInsertResult(userId, result);
                return new() { IsError = true, Message = result.Message };
            }
            #endregion CheckInput

            Response? response = null;

            var keywords = Extensions.StringExtension.GetKeywords(query.Input);
            StringBuilder keywordsString = new StringBuilder();
            foreach (var keyword in keywords)
            {
                keywordsString.Append(keyword);
            }

            #region Math
            //process math expression

            var mathValue = MathProcessor.ProcessMathExpression(keywordsString.ToString());

            //if user enter math express; return just the math
            if (mathValue != null)
            {
                response = new Response();
                response.Message = mathValue.ToString();
                response.Topic = "Math";
            }

            #endregion Math


            double score = 0; //find match score
            #region AIML

            if (response == null)
            {
                var input = query.Input.ToUpperInvariant();
                var matchedCategory = _categoryMatcher.FindMatch(input);
                
                if (matchedCategory != null)
                {
                    score = 100;
                    response = await _templateProcessor.ProcessAsync(user, matchedCategory, query, recursionDepth);

                }

                if (matchedCategory == null || response == null)
                {
                    (matchedCategory, score) = _categoryMatcher.FindClosestMatch(input);

                    if (matchedCategory != null)
                    {
                        response = await _templateProcessor.ProcessAsync(user, matchedCategory, query, recursionDepth);
                    }
                }

                
            }

            #endregion AIML

            #region NLP
            if ( (response == null || score < 60) && _bert != null)
            {
                if (response != null && score >= 60)
                {
                    Result result2 = new Result();
                    result2.QueryId = query.Id;
                    result2.HtmlMessage = response.HtmlMessage;
                    result2.Topic = response.Topic;
                    result2.Match = response.Match?.ToString() ?? string.Empty;
                    result2.Template = response.Template?.ToString() ?? string.Empty;
                    _conversationManager.UpdateOrInsertResult(userId, result2);
                }

                var (tokens, proability) = _categoryMatcher.AskBert(_bert, query.Input);
                if (proability != null)
                {
                    if (proability >= 0.30)
                    {
                        response = new Response();
                        response.Message = string.Join(" ", tokens);
                    }
                }

                //return nlp response
            }
            #endregion NLP

            if (response == null) //no response
            { 
                result.Message = $"I'm sorry, I don't have information for you request. Please restate your query.";
                if (string.IsNullOrEmpty(conversation.Name) && !query.IsSraiInput)
                {
                    if (string.IsNullOrEmpty(result.Match))
                    {
                        conversation.Name = DateTime.Now.ToString();
                    }
                    else
                    {
                        conversation.Name = result.Match;
                    }

                    _conversationManager.UpdateOrInsertConversation(userId, conversation);
                }
                _conversationManager.UpdateOrInsertResult(userId, result);
            }
            else //has response
            {

                //final result
                result.Message = response.Message;
                result.HtmlMessage = response.HtmlMessage;
                result.Topic = response.Topic;
                result.Match = response.Match?.ToString() ?? string.Empty;
                result.Template = response.Template?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(conversation.Name) && !query.IsSraiInput)
                {
                    if (string.IsNullOrEmpty(result.Match))
                    {
                        conversation.Name = DateTime.Now.ToString();
                    }
                    else
                    {
                        conversation.Name = result.Match;
                    }

                    _conversationManager.UpdateOrInsertConversation(userId, conversation);
                }

                _conversationManager.UpdateOrInsertResult(userId, result);
            }
            return response;

            
        }

        public async Task LearnAsync(string topic, string pattern, string response)
        {
            await _aimlLearner.DirectLearnAsync(pattern, response, topic);
        }
    }
}
