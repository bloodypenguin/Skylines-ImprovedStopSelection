using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UndergroundStopsEnabler.RedirectionFramework.Attributes;
using UnityEngine;

namespace ImprovedStopSelection.Detour
{
    [TargetType(typeof(TransportTool))]
    public class TransportToolDetour : TransportTool
    {
        [RedirectMethod]
        private bool GetStopPosition(TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            //begin mod(+): detect key
            bool alternateMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            //end mod

            NetManager instance1 = Singleton<NetManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            TransportManager instance3 = Singleton<TransportManager>.instance;
            fixedPlatform = false;
            if (info.m_transportType == TransportInfo.TransportType.Pedestrian)
            {
                Vector3 position = Vector3.zero;
                float laneOffset = 0.0f;
                uint laneID = 0;
                int laneIndex;
                if ((int)segment != 0 && !instance1.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, VehicleInfo.VehicleType.None, out position, out laneID, out laneIndex, out laneOffset))
                {
                    laneID = 0U;
                    if ((instance1.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None && (int)building == 0)
                        building = NetSegment.FindOwnerBuilding(segment, 363f);
                }
                if ((int)building != 0)
                {
                    if (instance2.m_buildings.m_buffer[(int)building].Info.m_hasPedestrianPaths)
                        laneID = instance2.m_buildings.m_buffer[(int)building].FindLane(NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, hitPos, out position, out laneOffset);
                    if ((int)laneID == 0)
                    {
                        Vector3 sidewalkPosition = instance2.m_buildings.m_buffer[(int)building].CalculateSidewalkPosition();
                        laneID = instance2.m_buildings.m_buffer[(int)building].FindAccessLane(NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, sidewalkPosition, out position, out laneOffset);
                    }
                }
                if ((int)laneID == 0)
                    return false;
                if ((double)laneOffset < 0.00392156885936856)
                {
                    laneOffset = 0.003921569f;
                    position = instance1.m_lanes.m_buffer[laneID].CalculatePosition(laneOffset);
                }
                else if ((double)laneOffset > 0.996078431606293)
                {
                    laneOffset = 0.9960784f;
                    position = instance1.m_lanes.m_buffer[laneID].CalculatePosition(laneOffset);
                }
                if ((int)this.m_line != 0)
                {
                    firstStop = instance3.m_lines.m_buffer[(int)this.m_line].m_stops;
                    ushort stop = firstStop;
                    int num = 0;
                    while ((int)stop != 0)
                    {
                        if ((int)instance1.m_nodes.m_buffer[(int)stop].m_lane == (int)laneID)
                        {
                            hitPos = instance1.m_nodes.m_buffer[(int)stop].m_position;
                            fixedPlatform = true;
                            return true;
                        }
                        stop = TransportLine.GetNextStop(stop);
                        if ((int)stop != (int)firstStop)
                        {
                            if (++num >= 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                        else
                            break;
                    }
                }
                hitPos = position;
                fixedPlatform = true;
                return true;
            }
            if ((int)segment != 0)
            {
                if ((instance1.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                {
                    building = NetSegment.FindOwnerBuilding(segment, 363f);
                    if ((int)building != 0)
                    {
                        BuildingInfo info1 = instance2.m_buildings.m_buffer[(int)building].Info;
                        TransportInfo transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                        TransportInfo transportLineInfo2 = info1.m_buildingAI.GetSecondaryTransportLineInfo();
                        //begin mod(*): check for !alternateMode
                        if (!alternateMode && transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType || !alternateMode && transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                            //end mod
                            segment = (ushort)0;
                        else
                            building = (ushort)0;
                    }
                }
                Vector3 position1;
                uint laneID1;
                int laneIndex1;
                float laneOffset1;
                if ((int)segment != 0 && instance1.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out position1, out laneID1, out laneIndex1, out laneOffset1))
                {
                    if (info.m_vehicleType == VehicleInfo.VehicleType.None)
                    {
                        NetLane.Flags flags1 = (NetLane.Flags)((int)instance1.m_lanes.m_buffer[laneID1].m_flags & 768);
                        NetLane.Flags flags2 = info.m_stopFlag;
                        NetInfo info1 = instance1.m_segments.m_buffer[(int)segment].Info;
                        if (info1.m_vehicleTypes != VehicleInfo.VehicleType.None)
                            flags2 = NetLane.Flags.None;
                        if (flags1 != NetLane.Flags.None && flags2 != NetLane.Flags.None && flags1 != flags2)
                            return false;
                        float stopOffset = info1.m_lanes[laneIndex1].m_stopOffset;
                        if ((instance1.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                            stopOffset = -stopOffset;
                        Vector3 direction;
                        instance1.m_lanes.m_buffer[laneID1].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out direction);
                        fixedPlatform = true;
                        return true;
                    }
                    Vector3 position2;
                    uint laneID2;
                    int laneIndex2;
                    float laneOffset2;
                    if (instance1.m_segments.m_buffer[(int)segment].GetClosestLanePosition(position1, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out position2, out laneID2, out laneIndex2, out laneOffset2))
                    {
                        NetLane.Flags flags = (NetLane.Flags)((int)instance1.m_lanes.m_buffer[laneID1].m_flags & 768);
                        if (flags != NetLane.Flags.None && info.m_stopFlag != NetLane.Flags.None && flags != info.m_stopFlag)
                            return false;
                        float stopOffset = instance1.m_segments.m_buffer[(int)segment].Info.m_lanes[laneIndex2].m_stopOffset;
                        if ((instance1.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                            stopOffset = -stopOffset;
                        Vector3 direction;
                        instance1.m_lanes.m_buffer[laneID2].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out direction);
                        fixedPlatform = true;
                        return true;
                    }
                }
            }
            //begin mod(*): check for !alternateMode
            if (!alternateMode && (int)building != 0)
            {
                //end mod
                ushort num1 = 0;
                if ((instance2.m_buildings.m_buffer[(int)building].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
                    num1 = Building.FindParentBuilding(building);
                if (this.m_building != 0 && (int)firstStop != 0 && (this.m_building == (int)building || this.m_building == (int)num1))
                {
                    hitPos = instance1.m_nodes.m_buffer[(int)firstStop].m_position;
                    return true;
                }
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                if (randomVehicleInfo != null)
                {
                    BuildingInfo info1 = instance2.m_buildings.m_buffer[(int)building].Info;
                    TransportInfo transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                    if (transportLineInfo1 == null && (int)num1 != 0)
                    {
                        building = num1;
                        info1 = instance2.m_buildings.m_buffer[(int)building].Info;
                        transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                    }
                    TransportInfo transportLineInfo2 = info1.m_buildingAI.GetSecondaryTransportLineInfo();
                    if (transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType || transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                    {
                        Vector3 vector3 = Vector3.zero;
                        int num2 = 1000000;
                        for (int index = 0; index < 12; ++index)
                        {
                            Randomizer randomizer = new Randomizer((ulong)index);
                            Vector3 position;
                            Vector3 target;
                            info1.m_buildingAI.CalculateSpawnPosition(building, ref instance2.m_buildings.m_buffer[(int)building], ref randomizer, randomVehicleInfo, out position, out target);
                            int num3 = 0;
                            if (info.m_avoidSameStopPlatform)
                                num3 = this.GetLineCount(position, target - position, info.m_transportType);
                            if (num3 < num2)
                            {
                                vector3 = position;
                                num2 = num3;
                            }
                            else if (num3 == num2 && (double)Vector3.SqrMagnitude(position - hitPos) < (double)Vector3.SqrMagnitude(vector3 - hitPos))
                                vector3 = position;
                        }
                        if ((int)firstStop != 0)
                        {
                            Vector3 position = instance1.m_nodes.m_buffer[(int)firstStop].m_position;
                            if ((double)Vector3.SqrMagnitude(position - vector3) < 16384.0)
                            {
                                uint lane = instance1.m_nodes.m_buffer[(int)firstStop].m_lane;
                                if ((int)lane != 0)
                                {
                                    ushort segment1 = instance1.m_lanes.m_buffer[lane].m_segment;
                                    if ((int)segment1 != 0 && (instance1.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                    {
                                        ushort ownerBuilding = NetSegment.FindOwnerBuilding(segment1, 363f);
                                        if ((int)building == (int)ownerBuilding)
                                        {
                                            hitPos = position;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        hitPos = vector3;
                        return num2 != 1000000;
                    }
                }
            }
            return false;
        }

        [RedirectReverse]
        private int GetLineCount(Vector3 stopPosition, Vector3 stopDirection, TransportInfo.TransportType transportType)
        {
            UnityEngine.Debug.Log("GetLineCount");
            return 0;
        }

        private ushort m_line => (ushort)typeof(TransportTool).GetField("m_line",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
    }
}
