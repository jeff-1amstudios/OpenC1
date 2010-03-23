using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using NFSEngine;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    enum CpuDriverState
    {
        Racing,
        Attacking,
        Reversing
    }

    class CpuDriver : IDriver
    {
        public Vehicle Vehicle { get; set; }
        public bool InPlayersView;

        CpuDriverState _state = CpuDriverState.Racing;
        OpponentPathNode _targetNode;
        OpponentPath _currentPath, _nextPath;
        Vector3 _lastPosition;
        float _lastPositionTime;
        float _nextStateChangeTime;
        float _lastTargetChangeTime;
        float _reverseTurning;
        int _nbrFails = -1;
        float _lastDistance;
        
        bool _raceStarted = false;

        public CpuDriver()
        {

        }

        public void OnRaceStart()
        {
            //Vehicle.Chassis.Motor.Gearbox.CurrentGear = 1;
            LogPosition(Vehicle.Position);
            SetTarget(OpponentController.GetClosestNode(_lastPosition));
            
            _raceStarted = true;
        }

        public void Update()
        {
            if (!_raceStarted) return;

            Vector3 pos = Vehicle.Position;
            bool isBraking = false;

            // check position
            if (_lastPositionTime + 1.5f < Engine.TotalSeconds)
            {
                float distFromLastPosition = Vector3.Distance(_lastPosition, pos);
                if (distFromLastPosition < 2)
                {
                    Escape(); //were stuck, try and escape
                }
                LogPosition(pos);
            }

            // check for state change
            if (_nextStateChangeTime < Engine.TotalSeconds)
            {
                if (_state == CpuDriverState.Reversing)
                {
                    _state = CpuDriverState.Racing;
                    LogPosition(pos);
                }
            }

            if (_state == CpuDriverState.Racing)
            {
                float distanceFromNode = Vector3.Distance(pos, _targetNode.Position);

                // if we've been trying to get to the same target for 20 seconds, get a new one
                if (_lastTargetChangeTime + 20 < Engine.TotalSeconds)
                {
                    if (_lastDistance < distanceFromNode) //only get another node if were not getting closer
                    {
                        GotoClosestNode(pos);
                        distanceFromNode = Vector3.Distance(pos, _targetNode.Position);
                    }
                }

                if (_currentPath != null)
                {
                    GameConsole.WriteLine("Limits " + _currentPath.MinSpeedAtEnd + ", " + _currentPath.MaxSpeedAtEnd);
                }


                if (_currentPath != null && Vehicle.Chassis.Speed > _currentPath.MaxSpeedAtEnd)
                {
                    float distToBrake = Vehicle.Chassis.Speed * 0.4f + ((Vehicle.Chassis.Speed - _currentPath.MaxSpeedAtEnd) * 1.4f);
                    //GameConsole.WriteLine("brake: " + (int)distToBrake + ", " + (int)distanceFromNode);
                    Matrix mat = Matrix.CreateTranslation(0, 0, distToBrake) * Vehicle.Chassis.Actor.GlobalPose;

                    if (distToBrake >= distanceFromNode)
                    {
                        Vehicle.Chassis.Brake(1);
                        isBraking = true;
                    }
                }

                // now see if we're at the target ignoring height (if we jump over it for example)
                distanceFromNode = Vector2.Distance(new Vector2(pos.X, pos.Z), new Vector2(_targetNode.Position.X, _targetNode.Position.Z));
                if (distanceFromNode < 17)
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

                    if (_nextPath == null)  // the node didnt have any start paths
                    {
                        GotoClosestNode(pos);
                    }
                    else
                    {
                        SetTarget(_currentPath.End);
                        //GameConsole.WriteEvent("NextPath " + _currentPath.End.Number);
                    }

                    //GetNextPath();
                    if (_currentPath == null && _nextPath == null)
                    {
                        Teleport(); //if the node we've just got to doesnt have any outgoing paths, teleport randomly
                    }
                }

                Vector3 towardsNode = _targetNode.Position - pos;

                float angle = GetSignedAngleBetweenVectors(Vehicle.Chassis.Actor.GlobalOrientation.Forward, towardsNode);
                angle *= 2;
                if (angle > 1) angle = 1;
                else if (angle < -1) angle = -1;

                if (Math.Abs(angle) > 0.003f) Vehicle.Chassis.Steer(angle);

                if (!isBraking)
                {
                    if (Math.Abs(angle) > 0.7f)
                        Vehicle.Chassis.Accelerate(0.5f); //if were turning hard, go easy on the gas pedal
                    else
                        Vehicle.Chassis.Accelerate(1.0f);
                }

                _lastDistance = distanceFromNode;
            }
            else if (_state == CpuDriverState.Reversing)
            {
                Vehicle.Chassis.Brake(0.5f);
                Vehicle.Chassis.Steer(_reverseTurning);
            }            

            Engine.DebugRenderer.AddWireframeCube(Matrix.CreateScale(2) * Matrix.CreateTranslation(_targetNode.Position), Color.Green);
        }

        private void LogPosition(Vector3 pos)
        {
            _lastPosition = pos;
            _lastPositionTime = Engine.TotalSeconds;
        }

        private void GotoClosestNode(Vector3 pos)
        {
            OpponentPathNode curNode = _targetNode;
            SetTarget(OpponentController.GetClosestNode(pos));

            if (curNode == _targetNode) //if the closest node is the one we've failed to get to
            {
                // if we've failed to get to the target twice we're really stuck, teleport straight to node :)
                Teleport(_targetNode);
                return;
            }
            GameConsole.WriteEvent("ClosestNode");
            _nbrFails++;
            //_currentPath = _nextPath = null;
        }

        private void SetTarget(OpponentPathNode node)
        {
            _targetNode = node;
            _lastTargetChangeTime = Engine.TotalSeconds;
            GetNextPath();
        }

        private void Escape()
        {
            _state = _state == CpuDriverState.Reversing ? CpuDriverState.Racing : CpuDriverState.Reversing;
            _nextStateChangeTime = Engine.TotalSeconds + Engine.Random.Next(1f, 3f);
            _reverseTurning = Engine.Random.Next(-1f, 0f);
        }

        private void Teleport()
        {
            Teleport(OpponentController.GetRandomNode());
        }
        private void Teleport(OpponentPathNode node)
        {
            SetTarget(node);
            Vehicle.Chassis.Actor.GlobalPosition = _targetNode.Position;
            Vehicle.Reset();
        }

        private void GetNextPath()
        {
            // for the next node, look at direction we will be turning and decide if we will need to slow down before we get there
            _nextPath = OpponentController.GetNextPath(_targetNode);

            if (_nextPath != null && _currentPath != null)
            {
                float nextPathAngle = MathHelper.ToDegrees(GetUnsignedAngleBetweenVectors(_currentPath.End.Position - _currentPath.Start.Position, _nextPath.End.Position - _nextPath.Start.Position));
                //GameConsole.WriteEvent("next path angle " + nextPathAngle);

                if (nextPathAngle > 5)
                {
                    float newspeed = (180 - nextPathAngle) * 0.6f;
                    if (newspeed < _currentPath.MaxSpeedAtEnd)
                    {
                        _currentPath.MaxSpeedAtEnd = newspeed;
                        _currentPath.UserSet = true;
                    }
                }
            }
        }


        public static float GetSignedAngleBetweenVectors(Vector3 from, Vector3 to)
        {

            from.Y = to.Y = 0;
            from.Normalize();
            to.Normalize();
            Vector3 toRight = Vector3.Cross(to, Vector3.Up);
            toRight.Normalize();

            float forwardDot = Vector3.Dot(from, to);
            float rightDot = Vector3.Dot(from, toRight);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return (float)angleBetween;
        }

        public float GetUnsignedAngleBetweenVectors(Vector3 from, Vector3 to)
        {
            from.Y = to.Y = 0;
            from.Normalize();
            to.Normalize();

            Vector2 a = new Vector2(from.X, from.Z);
            a.Normalize();
            Vector2 b = new Vector2(to.X, to.Z);
            b.Normalize();
            return (float)Math.Acos(Vector2.Dot(a, b));
            //return (float)Math.Acos(Vector3.Dot(from, to));
        }
    }
}
