using Mages.Core.Ast.Statements;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class ConversationManager
    {
        private string _baseDirectoryPath;
        private Dictionary<string, ConversationContext> _contexts;

        public ConversationManager(string baseDirectoryPath)
        {
            _baseDirectoryPath = baseDirectoryPath;
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(_baseDirectoryPath);
            _contexts = new Dictionary<string, ConversationContext>();
        }

        public ConversationContext GetUserContext(string userId)
        {
            var fileName = Helpers.SystemIOUtilities.SanitizeFileName(userId);
            if (!_contexts.ContainsKey(fileName))
            {
                var databasePath = Path.Combine(_baseDirectoryPath, $"{fileName}.db");
                var connectionString = $"Filename={databasePath}";

                var options = new DbContextOptionsBuilder<ConversationContext>()
                    .UseSqlite(connectionString)
                    .Options;

                var context = new ConversationContext(options);
                context.Database.EnsureCreated();
                _contexts[fileName] = context;
            }

            return _contexts[fileName];
        }

        public void AddConversation(string userId, Conversation conversation)
        {
            
            var context = GetUserContext(userId);
            context.Conversations.Add(conversation);
            context.SaveChanges();
        }

        public void UpdateOrInsertConversation(string userId, Conversation conversation)
        {
            var context = GetUserContext(userId);
            var existingConversation = context.Conversations.FirstOrDefault(c => c.Id == conversation.Id);

            if (existingConversation == null)
            {
                // Insert new conversation
                context.Conversations.Add(conversation);
            }
            else
            {
                // Update existing conversation
                existingConversation.Name = conversation.Name;
                existingConversation.Timestamp = DateTime.Now;
                // Update other properties as necessary...
            }

            context.SaveChanges();
        }

        public void DeleteConversation(string userId, string conversationId)
        {
            var context = GetUserContext(userId);
            var conversation = context.Conversations
                                      .Include(c => c.Queries)
                                      .ThenInclude(q => q.Results)
                                      .ThenInclude(r => r.Actions)
                                      .FirstOrDefault(c => c.Id == conversationId);

            if (conversation != null)
            {
                // Remove the conversation
                context.Conversations.Remove(conversation);
                context.SaveChanges();
            }
        }

        

        public Conversation? GetConversation(string userId, string? conversationId)
        {
            var context = GetUserContext(userId);
            if (!string.IsNullOrEmpty(conversationId))
            {
                // Return specific conversation or null if not found
                return context.Conversations.FirstOrDefault(c => c.Id == conversationId);
            }
            return null;
        }

        public List<Contracts.Conversation> GetAllConversations(string userId)
        {
            var context = GetUserContext(userId);
            return context.Conversations
                .Select(c => new Contracts.Conversation { UserId = userId, Id = c.Id, Name = c.Name, Timestamp = c.Timestamp })
                .ToList();
        }

        public List<string> GetConversationTopics(string userId, string conversationId)
        {
            var context = GetUserContext(userId);
            return context.Queries
                .Where(q => q.ConversationId == conversationId)
                .SelectMany(q => q.Results.Select(r => r.Topic))
                .Distinct()
                .ToList();
        }

        public List<Contracts.Query> GetConversationQueries(string userId, string conversationId)
        {
            var context = GetUserContext(userId);
            var results = context.Queries
                .Include(q=>q.Results)
                .Where(q => q.ConversationId == conversationId)
                .ToList();

            List<Contracts.Query> queries = new List<Contracts.Query>();
            foreach (var query in results)
            {
                Contracts.Query q = new Contracts.Query();
                q.Id = query.Id;
                q.ConversationId = query.ConversationId;
                q.Input = query.Input;
                q.Timestamp = query.Timestamp;
                foreach (var item in query.Results)
                {
                    Contracts.Result r = new Contracts.Result();
                    r.Id = item.Id;
                    r.Timestamp = item.Timestamp;
                    r.QueryId = item.QueryId;
                    r.Response = item.Message;
                    r.HtmlResponse = item.HtmlMessage;
                    r.Template = item.Template;
                    r.Topic = item.Topic;
                    q.Results.Add(r);
                }
                queries.Add(q);
            }
            return queries;
        }

        public void UpdateOrInsertResult(string userId, Result result)
        {
            var context = GetUserContext(userId);

            var existingResult = context.Results
                .FirstOrDefault(r => r.Id == result.Id && r.QueryId == result.QueryId);

            if (existingResult == null)
            {
                // Insert new result
                context.Results.Add(result);
                // Update timestamp of the conversation
                var query = context.Queries.FirstOrDefault(q=> q.Id == result.QueryId);
                if (query != null)
                {
                    var conversation = context.Conversations
                        .FirstOrDefault(c => c.Id == query.ConversationId);
                    if (conversation != null)
                    {
                        conversation.Timestamp = DateTime.Now;
                    }
                }
            }
            else
            {
                // Update existing result
                existingResult.Topic = result.Topic;
                existingResult.Template = result.Template;
                existingResult.Message = result.Message;
                existingResult.HtmlMessage = result.HtmlMessage;
                // Update other properties as necessary...
                existingResult.Timestamp = DateTime.Now;
            }

            context.SaveChanges();
        }

        public void UpdateOrInsertQuery(string userId, Query query)
        {
            var context = GetUserContext(userId);

            var existingQuery = context.Queries
                .FirstOrDefault(q => q.Id == query.Id && q.ConversationId == query.ConversationId);

            if (existingQuery == null)
            {
                // Insert new query
                context.Queries.Add(query);
                // Update timestamp of the conversation
                var conversation = context.Conversations
                    .FirstOrDefault(c => c.Id == query.ConversationId);
                if (conversation != null)
                {
                    conversation.Timestamp = DateTime.Now;
                }
            }
            else
            {
                // Update existing query
                existingQuery.Input = query.Input;
                existingQuery.Timestamp = DateTime.Now;
            }

            context.SaveChanges();
        }


    }
}
