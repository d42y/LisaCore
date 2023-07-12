using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Keep this as LisaCore
namespace LisaCore
{
    /// <summary>
    /// Part of Lisa class
    /// </summary>
    public partial class Lisa
    {
        public async Task<string> GetResponseAsync(string userId, string? converstationId, string input, int? recursionDepth = null)
        {
            Bot.Conversations.Query query = new Bot.Conversations.Query();
            query.Input = input;
            if (!string.IsNullOrEmpty(converstationId)) { query.ConversationId = converstationId; }
            if (string.IsNullOrEmpty(_aiKnowledgeDirectory) || _chatlBot == null) return "Error. Please initlize knowledge before I can process your request.";



            //process words
            var result = await _chatlBot.GetResponseAsync(userId, query, recursionDepth);
            if (result != null)
            {
                return result.Message;
            }

            return "I'm sorry, I don't have information for you request. Please restate your query.";
        }

        public async Task<string> Learn(string topic, string pattern, string template)
        {
            if (string.IsNullOrEmpty(_aiKnowledgeDirectory) || _chatlBot == null) return "Error. Please initlize knowledge before I can process your request.";
            try
            {
                await _chatlBot.LearnAsync(topic, pattern, template);
            }
            catch (Exception ex) { return $"Exception. {ex.Message}"; }

            return "Ok";
        }

        public bool IsKnowledgeInit()
        {
            return !(string.IsNullOrEmpty(_aiKnowledgeDirectory) || _chatlBot == null);
        }

        public List<Bot.Conversations.Contracts.Conversation> GetConversations(string userId)
        {
            var conversations = _chatlBot?.ConversationManager.GetAllConversations(userId) ?? new();
            return conversations;
        }

        public List<Bot.Conversations.Contracts.Query> GetQuery(string userId, string converstationId)
        {
            var queries = _chatlBot?.ConversationManager.GetConversationQueries(userId, converstationId) ?? new();
            return queries;
        }

        public List<string> GeConverstationTopics(string userId, string converstationId)
        {
            var topics = _chatlBot?.ConversationManager.GetConversationTopics(userId, converstationId) ?? new();
            return topics;
        }


    }
}
