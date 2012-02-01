using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OneAmEngine;

namespace OpenC1
{
    enum CpuDriverState
    {
        Racing,
        Attacking,
        ReturningToTrack,
        Sleeping
    }

    class CpuDriver : IDriver
    {
        const int GIVEUP_ATTACK_DISTANCE = 200;
        const int MIN_CATCHUP_DISTANCE = 450;
        const int MAX_CATCHUP_DISTANCE = 750;
        const int GIVEUP_DISTANCE_FROM_TRACK = 50;

        public Vehicle Vehicle { get; set; }
        public bool InPlayersView;
        public float DistanceFromPlayer;
        public bool IsDead;
        public float LastPlayerTouchTime;

        protected CpuDriverState _state;
        OpponentPathNode _targetNode;
        OpponentPath _currentPath, _nextPath;
        Vector3 _lastPosition;
        float _lastPositionTime, _lastStateChangeTime;
        float _nextDirectionChangeTime;
        float _lastTargetChangeTime;
        float _reverseTurning;
        int _nbrFails = -1;
        float _lastDistance;
        float _maxSpeedAtEndOfPath = 0;
        bool _isReversing;
        Vector3 _closestPointOnPath;
        protected float _catchupDistance;
        protected bool _raceStarted = false;

        public CpuDriver()
        {

        }

        public bool ModerateSteeringAtSpeed { get { return false; } }

        public virtual void OnRaceStart()
        {
            _lastStateChangeTime = Engine.TotalSeconds;
            Vehicle.Chassis.Motor.Gearbox.CurrentGear = 1;
            LogPosition(Vehicle.Position);
            _raceStarted = true;
            _catchupDistance = Engine.Random.Next(MIN_CATCHUP_DISTANCE, MAX_CATCHUP_DISTANCE);
        }

        public virtual void Update()
        {
            if (!_raceStarted) return;
            Vector3 pos = Vehicle.Position;
            bool isBraking = false;

            if (IsDead || _state == CpuDriverState.Sleeping)
            {
                Vehicle.Chassis.Brake(0);
                Vehicle.Chassis.Steer(0);
                return;
            }

            if (DistanceFromPlayer > _catchupDistance)
            {
                _catchupDistance = Engine.Random.Next(MIN_CATCHUP_DISTANCE, MAX_CATCHUP_DISTANCE);
                Teleport(OpponentController.GetNodeCloseToPlayer());
            }

            // check position
            if (Helpers.HasTimePassed(1.5f, _lastPositionTime))
            {
                float distFromLastPosition = Vector3.Distance(_lastPosition, pos);
                if (distFromLastPosition < 2 && Vehicle.Chassis.Speed < 3)
                {
                    Escape(); //were stuck, try and escape
                }
                LogPosition(pos);
            }
            if (Vehicle.Chassis.Actor.GlobalPose.Up.Y < 0.002f && Vehicle.Chassis.Speed < 5 && !InPlayersView)
            {
                Vehicle.Chassis.Reset();
                return;
            }

            // check for state change
            if (_nextDirectionChangeTime < Engine.TotalSeconds)
            {
                if (_isReversing)
                {
                    _isReversing = false;
                    LogPosition(pos);
                }
            }

            float distanceFromNode=0;
            if (_state == CpuDriverState.Racing) distanceFromNode = Vector3.Distance(pos, _targetNode.Position);
            else if (_state == CpuDriverState.ReturningToTrack) distanceFromNode = Vector3.Distance(pos, _closestPointOnPath);

            // if we've been trying to get to the same target for 20 seconds, get a new one
            if (_state != CpuDriverState.Attacking && Helpers.HasTimePassed(20, _lastTargetChangeTime))
            {
                if (_lastDistance <= distanceFromNode) //only get another node if were not getting closer
                {
                    TargetClosestNode(pos);
                    return;
                }
            }

            if (_state == CpuDriverState.Racing)
            {
                //if (_currentPath != null)
                //{
                //    GameConsole.WriteLine("Limits " + _currentPath.Number + ", " + _currentPath.MinSpeedAtEnd + ", " + _maxSpeedAtEndOfPath);
                //}

                if (_currentPath != null && Vehicle.Chassis.Speed > _maxSpeedAtEndOfPath)
                {
                    float distToBrake = Vehicle.Chassis.Speed * 0.45f + ((Vehicle.Chassis.Speed - _maxSpeedAtEndOfPath) * 1.4f);
                    //GameConsole.WriteLine("brake: " + (int)distToBrake + ", " + (int)distanceFromNode);
                    //Matrix mat = Matrix.CreateTranslation(0, 0, distToBrake) * Vehicle.Chassis.Actor.GlobalPose;

                    if (distToBrake >= distanceFromNode)
                    {
                        Vehicle.Chassis.Brake(1);
                        isBraking = true;
                    }
                }
                if (_currentPath != null)
                {
                    _closestPointOnPath = Helpers.GetClosestPointOnLine(_currentPath.Start.Position, _currentPath.End.Position, pos);
                    _closestPointOnPath.Y = pos.Y; //ignore Y
                    if (Vector3.Distance(_closestPointOnPath, pos) > _currentPath.Width)
                    {
                        SetState(CpuDriverState.ReturningToTrack);
                    }
                }

                // now see if we're at the target ignoring height (if we jump over it for example)
                distanceFromNode = Vector2.Distance(new Vector2(pos.X, pos.Z), new Vector2(_targetNode.Position.X, _targetNode.Position.Z));
                if (distanceFromNode < 11 && pos.Y >= _targetNode.Position.Y)
                {
                    _nbrFails = 0; //reset fail counter

                    if (_currentPath != null)
                    {
                        if (Vehicle.Chassis.Speed < _currentPath.MinSpeedAtEnd)
                        {
                            Vehicle.Chassis.Boost();
                        }
                    }
                    _currentPath = _nextPath;

                    if (_currentPath != null && _currentPath.Type == PathType.Cheat)
                    {
                        Teleport(_currentPath.End);
                        SetState(CpuDriverState.Racing);
                        return;
                    }

                    if (_nextPath == null)  // the node didnt have any start paths
                    {
                        TargetClosestNode(pos);
                    }
                    else
                    {
                        TargetNode(_currentPath.End);
                    }

                    if (_currentPath == null && _nextPath == null)
                    {
                        Teleport(); //if the node we've just got to doesnt have any outgoing paths, teleport randomly
                    }
                }
            }
            else if (_state == CpuDriverState.ReturningToTrack)
            {
                _closestPointOnPath = Helpers.GetClosestPointOnLine(_currentPath.Start.Position, _currentPath.End.Position, pos);

                GameConsole.WriteLine("dist from track", Vector3.Distance(_closestPointOnPath, pos));
                if (Vector3.Distance(_closestPointOnPath, pos) > GIVEUP_DISTANCE_FROM_TRACK)
                {
                    TargetClosestNode(pos);
                    return;
                }

                _closestPointOnPath.Y = pos.Y; //ignore Y
                
                float dist = Vector3.Distance(_closestPointOnPath, pos);
                if (dist < _currentPath.Width)
                {
                    _state = CpuDriverState.Racing;
                }
                else if (dist < _currentPath.Width * 1.5f)
                {
                    _closestPointOnPath = Vector3.Lerp(_currentPath.End.Position, _closestPointOnPath, dist / (_currentPath.Width * 1.5f));
                }
                //Engine.DebugRenderer.AddCube(Matrix.CreateTranslation(_closestPointOnPath), Color.Blue);
            }
            else if (_state == CpuDriverState.Attacking)
            {
                if (DistanceFromPlayer > GIVEUP_ATTACK_DISTANCE)
                    TargetClosestNode(pos);
            }

            Vector3 towardsNode = Vector3.Zero;
            if (_state == CpuDriverState.Racing)
            {
                towardsNode = _targetNode.Position - pos;
            }
            else if (_state == CpuDriverState.Attacking)
            {
                towardsNode = Race.Current.PlayerVehicle.Position - pos;
            }
            else if (_state == CpuDriverState.ReturningToTrack)
            {
                towardsNode = _closestPointOnPath - pos;
            }

            GameConsole.WriteLine("state: " + _state);

            float angle = Helpers.GetSignedAngleBetweenVectors(Vehicle.Chassis.Actor.GlobalOrientation.Forward, towardsNode, true);
            angle *= 1.5f;
            if (angle > 1) angle = 1;
            else if (angle < -1) angle = -1;

            if (Math.Abs(angle) > 0.003f) Vehicle.Chassis.Steer(angle);

            if (!isBraking)
            {
                if (_state == CpuDriverState.ReturningToTrack)
                {
                    if (Math.Abs(angle) > 0.3f)
                    {
                        float speed = 20; // 20 + ((1 - Math.Abs(angle)) * 30);
                        if (Vehicle.Chassis.Speed > speed)
                            Vehicle.Chassis.Brake(1);
                        else
                            Vehicle.Chassis.Accelerate(0.4f);

                        GameConsole.WriteLine("rtt", angle);
                        GameConsole.WriteLine("rttspeed", speed);
                    }
                    else
                        Vehicle.Chassis.Accelerate(0.5f);
                }
                else
                {
                    if (Math.Abs(angle) > 0.7f)
                        Vehicle.Chassis.Accelerate(0.5f); //if were turning hard, go easy on the gas pedal
                    else
                        Vehicle.Chassis.Accelerate(1.0f);
                }
            }

            _lastDistance = distanceFromNode;

            if (_isReversing)
            {
                Vehicle.Chassis.Brake(0.3f);
                Vehicle.Chassis.Steer(_reverseTurning);
            }

            if (Vehicle.Chassis.Motor.Gearbox.CurrentGear == 0)
                Vehicle.Chassis.Motor.Gearbox.CurrentGear = 1;

            //Engine.DebugRenderer.AddWireframeCube(Matrix.CreateScale(2) * Matrix.CreateTranslation(_targetNode.Position), Color.Green);
        }

        public void SetState(CpuDriverState state)
        {
            _state = state;
            _lastStateChangeTime = Engine.TotalSeconds;
        }

        private void LogPosition(Vector3 pos)
        {
            _lastPosition = pos;
            _lastPositionTime = Engine.TotalSeconds;
        }

        private void TargetClosestNode(Vector3 pos)
        {
            OpponentPathNode curNode = _targetNode;
            TargetNode(OpponentController.GetClosestNode(pos));
            _currentPath = null;

            if (curNode == _targetNode) //if the closest node is the one we've failed to get to
            {
                // if we've failed to get to the target twice we're really stuck, teleport straight to node :)
                Teleport(_targetNode);
                return;
            }
            GameConsole.WriteEvent("ClosestNode");
            _nbrFails++;
        }

        public void TargetNode(OpponentPathNode node)
        {
            _targetNode = node;
            SetState(CpuDriverState.Racing);
            _lastTargetChangeTime = Engine.TotalSeconds;
            GetNextPath();
        }

        private void Escape()
        {
            _isReversing = !_isReversing;
            _nextDirectionChangeTime = Engine.TotalSeconds + Engine.Random.Next(1.5f, 5f);
            _reverseTurning = Engine.Random.Next(-1f, 1f);
        }

        private void Teleport()
        {
            Teleport(OpponentController.GetRandomNode());
        }

        private void Teleport(OpponentPathNode node)
        {
            TargetNode(node);
            Vehicle.Teleport(_targetNode.Position);
            Vehicle.Chassis.Reset();

            if (_nextPath != null)
            {
                Matrix m = Vehicle.Chassis.Actor.GlobalOrientation;
                m.Forward = _nextPath.End.Position - _nextPath.Start.Position;
                m.Forward = Vector3.Normalize(m.Forward);
                Vehicle.Chassis.Actor.GlobalOrientation *= Matrix.CreateRotationY(Helpers.GetSignedAngleBetweenVectors(Vehicle.Chassis.Actor.GlobalOrientation.Forward, _nextPath.End.Position - _nextPath.Start.Position, true));
            }            
        }

        private void GetNextPath()
        {
            // for the next node, look at direction we will be turning and decide if we will need to slow down before we get there
            _nextPath = OpponentController.GetNextPath(_targetNode);
            
            if (_nextPath != null && _currentPath != null)
            {
                float nextPathAngle = MathHelper.ToDegrees(Helpers.GetUnsignedAngleBetweenVectors(_currentPath.End.Position - _currentPath.Start.Position, _nextPath.End.Position - _nextPath.Start.Position, false));

                if (nextPathAngle > 5)
                {
                    float newspeed = (180 - nextPathAngle) * 0.50f;
                    _maxSpeedAtEndOfPath = newspeed;
                }
                else
                {
                    _maxSpeedAtEndOfPath = 255;
                }
            }
        }

        internal void OnPlayerHit(float force)
        {
            LastPlayerTouchTime = Engine.TotalSeconds;
            SetState(CpuDriverState.Attacking);
        }
    }
}
