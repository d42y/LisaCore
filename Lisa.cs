using BrickSchema.Net;
using BrickSchema.Net.Classes;
using BrickSchema.Net.Relationships;
using LisaCore.Behaviors;
using LisaCore.Behaviors.Enums;
using LisaCore.Bot;
using LisaCore.Interpreter;
using LisaCore.Nlp.BERT;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LisaCore
{
    public class Lisa
    {
        private string? _aiKnowledgeDirectory = null;
        private Chat? _chatlBot = null;
        private CodeProcessor _codeProcessor;
        private ILogger? _logger;
        private Dictionary<string, List<Conversion>> _conversations;
        private Bert? _bert = null;
        private BrickSchemaManager _graph;
        private bool _saveBrick = false;
        private BehaviorManager _behaviorManager;
        public Lisa(ILogger? logger = null)
        {
            _logger = logger;
            InitCodeProcessor();
        }

        public Lisa(string aiKnowledgeDirectory, ILogger? logger = null)
        {
            _logger = logger;
            InitCodeProcessor();
            InitKnowledge(aiKnowledgeDirectory);
            _behaviorManager = new BehaviorManager();
        }

        private void InitCodeProcessor()
        {

            _codeProcessor = new CodeProcessor();
            LoadDefaultNameSpaces();
        }


        public void InitKnowledge(string aiKnowledgeDirectory)
        {

            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(aiKnowledgeDirectory);
            string brickFile = Path.Combine(aiKnowledgeDirectory, "graph.json");
            _graph = new BrickSchemaManager(brickFile);

            _aiKnowledgeDirectory = aiKnowledgeDirectory;
            _chatlBot = new Chat(_aiKnowledgeDirectory);
            if (_bert != null)
            {
                _chatlBot?.SetBert(_bert);
            }
        }

        #region Nlp
        public void InitNlp(string vocabularyFilePath, string ModelPath, bool useGpu = false)
        {
            _bert = new Bert(vocabularyFilePath, ModelPath, null, useGpu);
            _chatlBot?.SetBert(_bert);
        }

        public void InitNlp(string vocabularyFilePath, string ModelPath, string contextFilePath, bool useGpu = false)
        {
            _bert = new Bert(vocabularyFilePath, ModelPath, contextFilePath, useGpu);
            _chatlBot?.SetBert(_bert);
        }

        public void LoadNlpContextFromFile(string contextFilePath)
        {
            if (_bert != null)
            {
                _bert.LoadContextFromFile(contextFilePath);
            }
            else
            {
                //thwor error
            }
        }

        public void SetNlpContext(Dictionary<int, string> context)
        {
            _bert?.SetContext(context);
            _chatlBot.SetNlpContext(context);
        }

        #endregion Nlp

        #region Brick
        public BrickSchemaManager Graph { get { return _graph; } }


        public List<BrickEntity> GetEquipmentEntities(List<string>? equipmentIds = null)
        {
            List<BrickEntity> equipments = _graph.GetEquipments(equipmentIds ?? new()); ;
            return equipments;
        }

        public List<BrickBehavior> GetEquipmentBehaviors(string equipmentId)
        {
            var brickBehaviors = _graph.GetEquipmentBehaviors(equipmentId);
            return brickBehaviors;
        }

        public List<BrickBehavior> GetBehaviors(List<string>? behaviorIds = null)
        {
            var brickBehaviors = _graph.GetBehaviors(behaviorIds??new());


            return brickBehaviors;
        }
       
        public void SaveBrick()
        {
            if (_saveBrick)
            {
                _graph.SaveSchema();
            }
        }

        public void AddTenant(string id, string name)
        {
            var tenant = _graph.AddTenant(id);
            tenant.Name = name;
        }

        public void AddLocation(LocationTypes type, string id, string name)
        {
            bool save = !_graph.IsEntity(id);
            switch (type)
            {
                case LocationTypes.Building:
                    var bilding = _graph.AddLocationBuilding(id);
                    if (!bilding.Name.Equals(name))
                    {
                        bilding.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.CommonSpace:
                    var commonSpace = _graph.AddLocationCommonSpace(id);
                    if (!commonSpace.Name.Equals(name))
                    {
                        commonSpace.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Entrance:
                    var enterance = _graph.AddLocationEntrance(id);
                    if (!enterance.Name.Equals(name))
                    {
                        enterance.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Floor:
                    var floor = _graph.AddLocationFloor(id);
                    if (!floor.Name.Equals(name))
                    {
                        floor.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.GateHouse:
                    var gateHouse = _graph.AddLocationGateHouse(id);
                    if (!gateHouse.Name.Equals(name))
                    {
                        gateHouse.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.MediaHotDesk:
                    var mediaHotDesk = _graph.AddLocationMediaHotDesk(id);
                    if (!mediaHotDesk.Name.Equals(name))
                    {
                        mediaHotDesk.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.OutdoorArea:
                    var outdoorArea = _graph.AddLocationOutdoorArea(id);
                    if (!outdoorArea.Name.Equals(name))
                    {
                        outdoorArea.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Outside:
                    var outside = _graph.AddLocationOutside(id);
                    if (!outside.Name.Equals(name))
                    {
                        outside.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Parking:
                    var parking = _graph.AddLocationParking(id);
                    if (!parking.Name.Equals(name))
                    {
                        parking.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Region:
                    var region = _graph.AddLocationRegion(id);
                    if (!region.Name.Equals(name))
                    {
                        region.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Room:
                    var room = _graph.AddLocationRoom(id);
                    if (!room.Name.Equals(name))
                    {
                        room.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Site:
                    var site = _graph.AddLocationSite(id);
                    if (!site.Name.Equals(name))
                    {
                        site.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Space:
                    var space = _graph.AddLocationSpace(id);
                    if (!space.Name.Equals(name))
                    {
                        space.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Storey:
                    var storey = _graph.AddLocationStorey(id);
                    if (!storey.Name.Equals(name))
                    {
                        storey.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.TicketingBooth:
                    var ticketingBooth = _graph.AddLocationTicketingBooth(id);
                    if (!ticketingBooth.Name.Equals(name))
                    {
                        ticketingBooth.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Tunnel:
                    var tunnel = _graph.AddLocationTunnel(id);
                    if (!tunnel.Name.Equals(name))
                    {
                        tunnel.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.VerticalSpace:
                    var verticalSpace = _graph.AddLocationVerticalSpace(id);
                    if (!verticalSpace.Name.Equals(name))
                    {
                        verticalSpace.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.WaterTank:
                    var waterTank = _graph.AddLocationWaterTank(id);
                    if (!waterTank.Name.Equals(name))
                    {
                        waterTank.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Wing:
                    var wing = _graph.AddLocationWing(id);
                    if (!wing.Name.Equals(name))
                    {
                        wing.Name = name;
                        save = true;
                    }
                    break;
                case LocationTypes.Zone:
                    var zone = _graph.AddLocationZone(id);
                    if (!zone.Name.Equals(name))
                    {
                        zone.Name = name;
                        save = true;
                    }
                    break;
            }

            if (save)
            {
                _saveBrick = true;
            }
        }

        public void AddEquipment(EquipmentTypes type, string id, string name)
        {
            bool save = !_graph.IsEntity(id);
            switch (type)
            {
                case EquipmentTypes.AHU:
                    var ahu = _graph.AddEquipmentHVACAHU(id);
                    if (!ahu.Name.Equals(name))
                    {
                        ahu.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.Chiller:
                    var chiller = _graph.AddEquipmentHVACChiller(id);
                    if (!chiller.Name.Equals(name))
                    {
                        chiller.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.CoolingTower:
                    var coolingTower = _graph.AddEquipmentHVACCoolingTower(id);
                    if (!coolingTower.Name.Equals(name))
                    {
                        coolingTower.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.FCU:
                    var fcu = _graph.AddEquipmentHVACTerminalUnitFCU(id);
                    if (!fcu.Name.Equals(name))
                    {
                        fcu.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.Meter:
                    var meter = _graph.AddEquipmentMeter(id);
                    if (!meter.Name.Equals(name))
                    {
                        meter.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.Pump:
                    var pump = _graph.AddEquipmentHVACPump(id);
                    if (!pump.Name.Equals(name))
                    {
                        pump.Name = name;
                        save = true;
                    }
                    break;
                case EquipmentTypes.VAV:
                    var vav = _graph.AddEquipmentHVACTerminalUnitVAV(id);
                    if (!vav.Name.Equals(name))
                    {
                        vav.Name = name;
                        save = true;
                    }
                    break;
            }
            if (save)
            {
                _saveBrick = true;
            }
        }

        public void AddPoint(PointTypes type, string id, string name, object? readFunction = null, object? writeFunction = null)
        {
            bool save = !_graph.IsEntity(id);
            switch (type)
            {
                case PointTypes.Alarm:
                    var alarm = _graph.AddPointAlarm(id);
                    if (!alarm.Name.Equals(name))
                    {
                        alarm.Name = name;
                        save = true;
                    }
            
                    
                    break;
                case PointTypes.Command:
                    var cmd = _graph.AddPointCommand(id);
                    if (!cmd.Name.Equals(name))
                    {
                        cmd.Name = name;
                        save = true;
                    }

                   
                    break;
                case PointTypes.Parameter:
                    var parameter = _graph.AddPointParameter(id);
                    if (!parameter.Name.Equals(name))
                    {
                        parameter.Name = name;
                        save = true;
                    }

                   
                    break;
                case PointTypes.Sensor:
                    var sensor = _graph.AddPointSensor(id);
                    if (!sensor.Name.Equals(name))
                    {
                        sensor.Name = name;
                        save = true;
                    }

                    
                    break;
                case PointTypes.Setpoint:
                    var setpoint = _graph.AddPointSetpoint(id);
                    if (!setpoint.Name.Equals(name))
                    {
                        setpoint.Name = name;
                        save = true;
                    }
                    
                    break;
                case PointTypes.Status:
                    var status = _graph.AddPointStatus(id);
                    if (!status.Name.Equals(name))
                    {
                        status.Name = name;
                        save = true;
                    }

                   
                    break;
            }
            if (save)
            {
                _saveBrick = true;
            }
        }

        public Tag? AddTag(string name)
        {
            bool save = !_graph.IsTag(name);
            var tag = _graph.AddTag(name);

            if (save)
            {
                _saveBrick = true;
            }
            return tag;
        }

        public Tag? GetTag(string name)
        {
            return _graph.GetTag(name);
        }

        public void AddRelationship(RelationshipTypes type, string id, string parentId)
        {
            var entity = _graph.GetEntity(id);
            if (entity != null)
            {
                string RelationshipName = string.Empty;
                switch (type)
                {
                    case RelationshipTypes.AssociatedWith:
                        RelationshipName = typeof(AssociatedWith).Name;
                        break;
                    case RelationshipTypes.Fedby:
                        RelationshipName = typeof(Fedby).Name;
                        break;
                    case RelationshipTypes.LocationOf:
                        RelationshipName = typeof(LocationOf).Name;
                        break;
                    case RelationshipTypes.MeterBy:
                        RelationshipName = typeof(MeterBy).Name;
                        break;
                    case RelationshipTypes.PartOf:
                        RelationshipName = typeof(PartOf).Name;
                        break;
                    case RelationshipTypes.PointOf:
                        RelationshipName = typeof(PointOf).Name; ;
                        break;
                    case RelationshipTypes.SubmeterOf:
                        RelationshipName = typeof(SubmeterOf).Name;
                        break;
                    case RelationshipTypes.TagOf:
                        RelationshipName = typeof(TagOf).Name;
                        break;
                }
                var relationship = entity.Relationships.FirstOrDefault(x => x.ParentId.Equals(parentId) && x.Type.Equals(RelationshipName));
                if (relationship == null)
                {
                    switch (type)
                    {
                        case RelationshipTypes.AssociatedWith:
                            entity.AddRelationshipAssociatedWith(parentId);
                            break;
                        case RelationshipTypes.Fedby:
                            entity.AddRelationshipFedBy(parentId);
                            break;
                        case RelationshipTypes.LocationOf:
                            entity.AddRelationshipLocationOf(parentId);
                            break;
                        case RelationshipTypes.MeterBy:
                            entity.AddRelationshipMeterBy(parentId);
                            break;
                        case RelationshipTypes.PartOf:
                            entity.AddRelationshipPartOf(parentId);
                            break;
                        case RelationshipTypes.PointOf:
                            entity.AddRelationshipPointOf(parentId);
                            break;
                        case RelationshipTypes.SubmeterOf:
                            entity.AddRelationshipSubmeterOf(parentId);
                            break;
                        case RelationshipTypes.TagOf:
                            entity.AddRelationshipTagOf(parentId);
                            break;
                    }
                    _saveBrick = true;
                }
            }
        }

        public void AddBehavior(string entityId, BrickBehavior behavior)
        {
            if (behavior == null) return;

            var entity = _graph.GetEntity(entityId);
            if (entity != null)
            {
                if (!entity.RegisteredBehaviors.ContainsKey(behavior?.Type??string.Empty))
                {
                    _saveBrick = true;
                }
                behavior.SetLogger(_logger);
                _behaviorManager.Subscribe(behavior.OnTimerTick);
                entity.AddBehavior(behavior);
            }

        }

        public void StartBehavior(string entityId, string type)
        {
            var entity = _graph.GetEntity(entityId);
            if (entity != null)
            {
                var behaviors = entity.GetBehaviors(type);
                foreach (var behavior in behaviors)
                {
                    behavior.Start();
                }
            }

        }

        public void StopBehavior(string entityId, string type)
        {
            var entity = _graph.GetEntity(entityId);
            if (entity != null)
            {
                var behaviors = entity.GetBehaviors(type);
                foreach (var behavior in behaviors)
                {
                    behavior.Stop();
                }
            }

        }

        #endregion Brick

        #region Code Processor

        private void LoadDefaultNameSpaces()
        {
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.AirFlow");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Chiller");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Pump");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.Efficiency.Temperature");
            _codeProcessor.AddNamespace("LisaCore.MachineLearning.DataModels");
        }

        public void AddReference(Assembly assembly)
        {
            _codeProcessor.AddReference(assembly);
        }

        /// <summary>
        /// Adds a namespace required by the C# code.
        /// </summary>
        /// <param name="namespaceName">The namespace to add.</param>
        public void AddNamespace(string namespaceName)
        {
            _codeProcessor.AddNamespace(namespaceName);
        }

        // <summary>
        /// Sets a global variable for the C# code.
        /// </summary>
        /// <param name="name">The name of the global variable.</param>
        /// <param name="value">The value of the global variable.</param>
        public void SetGlobalVariable(string name, object value)
        {
            _codeProcessor.SetGlobalVariable(name, value);
        }

        /// <summary>
        /// Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The C# code to execute.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ExecuteAsync(string code, object? objects = null)
        {
            return await _codeProcessor.ExecuteAsync(code, objects);
        }

        /// <summary>
        /// Compiles and executes C# code with callback asynchronously.
        /// </summary>
        /// <param name="code">The C# code to execute.</param>
        /// <param name="callback">An optional callback to invoke when the code is executed.</param>
        /// <param name="objects">Optional user objects to pass to the code.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ExecuteAsync(string code, Action<object?> callback, object? objects = null)
        {
            return await _codeProcessor.ExecuteAsync(code, callback, objects);
        }


        /// <summary>
        /// Continues previous code execution state. Compiles and executes C# code asynchronously.
        /// </summary>
        /// <param name="code">The code to execute.</param>
        /// <returns>A tuple containing the result of the code execution and any error that occurred.</returns>
        public async Task<(object? Result, Exception? Error)> ContinueExecuteAsync(string code)
        {
            return await _codeProcessor.ContinueExecuteAsync(code);
        }
        #endregion code processor

        #region Bot
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


        #endregion Bot
    }
}

