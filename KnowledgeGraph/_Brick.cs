using BrickSchema.Net.Classes;
using BrickSchema.Net.Relationships;
using BrickSchema.Net;
using LisaCore.KnowledgeGraph.Enums;
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
        public BrickSchemaManager Graph { get { return _graph; } }

        public BrickEntity GetEntity(string id, bool byRefrence = false)
        {
            var entity = Graph.GetEntity(id);
            return entity;
        }

        public List<BrickEntity> GetEntities(List<string>? entityIds = null, bool byReference = false)
        {
            List <BrickEntity>? entities= null;
            if (entityIds == null || entityIds?.Count == 0)
            {
                entities = Graph.GetEntities(byReference);
            }
            else
            {
                entities = Graph.GetEntities(byReference).Where(x=> entityIds?.Contains(x.Id)??false).ToList();
            }
            return entities;
        }
        public List<BrickEntity> GetEntities<T>(List<string>? entityIds = null, bool byReference = false)
        {
            List<BrickEntity>? entities = null;
            if (entityIds == null || entityIds?.Count == 0)
            {
                entities = Graph.GetEntities<T>(byReference);
            }
            else
            {
                entities = Graph.GetEntities<T>(byReference).Where(x => entityIds?.Contains(x.Id) ?? false).ToList();
            }
            return entities;
        }

        public List<BrickEntity> GetEquipmentEntities(List<string>? equipmentIds = null, bool byReference = false)
        {
            List<BrickEntity> equipments = _graph.GetEquipments(equipmentIds ?? new(), byReference); ;
            return equipments;
        }

        public List<BrickBehavior> GetEquipmentBehaviors(string equipmentId, bool byReference = false)
        {
            var brickBehaviors = _graph.GetEquipmentBehaviors(equipmentId);
            return brickBehaviors;
        }

        public Dictionary<string, string> GetRegisteredEquipmentBehaviors(string equipmentId, bool byReference = false)
        {
            var brickBehaviors = _graph.GetRegisteredEquipmentBehaviors(equipmentId, byReference);
            return brickBehaviors;
        }

        public List<BrickBehavior> GetBehaviors(List<string>? behaviorIds = null, bool byReference = false)
        {
            var brickBehaviors = _graph.GetBehaviors(behaviorIds ?? new(), byReference);


            return brickBehaviors;
        }

        public void SaveGraph()
        {

            _graph.SaveSchema();

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
            
        }

        public Tag? AddTag(string name)
        {
            bool save = !_graph.IsTag(name);
            var tag = _graph.AddTag(name);

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
                    
                }
            }
        }

        public void AddBehavior(string entityId, BrickBehavior behavior)
        {
            if (behavior == null) return;

            var entity = _graph.GetEntity(entityId);
            if (entity != null)
            {
                var behaviors = entity.GetBehaviors(behavior.Type);
                if (behaviors.Count >= 1)
                {
                    for (int i = 0; i < behaviors.Count - 1; i++)
                    {
                        _behaviorManager.Unsubscribe(behaviors[i].OnTimerTick);
                        entity.RemoveBehavior(behaviors[i]);
                    }
                    var foundBehavior = behaviors[behaviors.Count - 1];
                    if (foundBehavior.Parent == null) foundBehavior.Parent = entity;
                }
                else
                {
                    if (entity.RegisteredBehaviors.ContainsKey(behavior.Type))
                    {
                        behavior.Id = entity.RegisteredBehaviors[behavior.Type];
                    }
                    else
                    {
                        entity.RegisteredBehaviors.Add(behavior.Type, behavior.Id);
                    }
                    if (!behavior.IsLogger)
                    {
                        behavior.SetLogger(_logger);
                    }
                    behavior.Parent = entity; //must set this before start
                    behavior.Start();
                    entity.Behaviors.Add(behavior);

                    _behaviorManager.Subscribe(behavior.OnTimerTick);
                }
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


    }
}
