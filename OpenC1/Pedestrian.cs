using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;
using OpenC1.Physics;
using OneAmEngine;

namespace OpenC1
{
    class Pedestrian
    {
        public static float RunningSpeed = 5f;

        public int RefNumber;
        public int InitialInstruction;
        public PedestrianBehaviour Behaviour;
        public List<PedestrianInstruction> Instructions = new List<PedestrianInstruction>();
        public Vector3 Position;
        public bool IsHit;
        public float DistanceFromPlayer;

        PedestrianAction _currentAction;
        PedestrianSequence _currentSequence;

        private bool _inLoopingFrames;
        private int _frameIndex;
        private float _frameTime;
        private float _frameRate;        
        private int _currentInstruction, _instructionDirection = 1;
        private bool _isRunning;
        private Actor _physXActor;
        private Vector3 _direction;
        private float _groundHeight;        
        private float _hitSpeed, _hitSpinSpeed, _hitUpSpeed, _hitCurrentSpin;
        private bool _isFalling;
		private Vector3 _hitVelocity;
        
        public bool _stopUpdating;

        public Pedestrian()
        {
        }

        public void Initialize()
        {
            if (Behaviour.RefNumber == 99) //flag waving guy
            {
                SetAction(Behaviour.Actions[7], true);
            }
            else
            {
                SetAction(Behaviour.Standing, true);
            }
            Position = Instructions[InitialInstruction].Position;
            _currentInstruction = InitialInstruction;

            if (Instructions[_currentInstruction].AutoY)
            {
                StillDesign.PhysX.RaycastHit hit = PhysX.Instance.Scene.RaycastClosestShape(new StillDesign.PhysX.Ray(Position + new Vector3(0, 10, 0), Vector3.Down), StillDesign.PhysX.ShapesType.Static);
                Position = hit.WorldImpact;
            }

            ActorDescription actorDesc = new ActorDescription();
            actorDesc.BodyDescription = new BodyDescription(1);
            actorDesc.BodyDescription.MassSpaceInertia = new Vector3(1, 1, 1);
            actorDesc.BodyDescription.BodyFlags |= BodyFlag.Kinematic;

            BoxShapeDescription box = new BoxShapeDescription(1.6f, 2.5f, 1.6f);
            box.Flags = ShapeFlag.TriggerOnEnter;
            box.LocalPosition = new Vector3(0, 1, 0);
            actorDesc.Shapes.Add(box);
            _physXActor = PhysX.Instance.Scene.CreateActor(actorDesc);
            _physXActor.GlobalPosition = Position;
            _physXActor.UserData = this;
        }

        public void SetAction(PedestrianAction action, bool force)
        {
            if (_currentAction == action) return;
            if (_currentSequence != null && !_currentSequence.Collide && !force) return;
            if (action == null) return;
            _currentAction = action;
            SetSequence(Behaviour.Sequences[_currentAction.Sequences[0].SequenceIndex]);

            if (_currentAction.Sounds.Length > 0)
                SoundCache.Play(_currentAction.Sounds[Engine.Random.Next(_currentAction.Sounds.Length)], Race.Current.PlayerVehicle, true);
        }

        public void OnHit(Vehicle vehicle)
        {
            IsHit = true;
            _groundHeight = Position.Y;

            float speed = Math.Min(140, vehicle.Chassis.Speed);

            if (speed > 90)
            {
                _hitSpinSpeed = speed * Engine.Random.Next(0.07f, 0.13f);
                if (Engine.Random.Next() % 2 == 0) _hitSpinSpeed *= -1;
                _hitUpSpeed = speed * 0.10f;
                _hitSpeed = speed * Behaviour.Acceleration * 10000;
				if (!IsPowerup)
					PedestrianGibsController.AddGibs(Position + new Vector3(0, 1.2f, 0), vehicle.Chassis.Actor.LinearVelocity);
            }
            else
            {
				if (!IsPowerup && speed > 50)
					PedestrianGibsController.AddGibs(Position + new Vector3(0, 0.5f, 0), vehicle.Chassis.Actor.LinearVelocity);
                _hitSpeed = speed * Behaviour.Acceleration * 19000;
            }
			
			
            _direction = Vector3.Normalize(vehicle.Chassis.Actor.LinearVelocity);
            if (float.IsNaN(_direction.X))
            {
                _direction = Vector3.Zero;
            }
            SetAction(Behaviour.FatalImpact, true);
            if (Behaviour.ExplodingSounds.Length > 0)
                SoundCache.Play(Behaviour.ExplodingSounds[0], Race.Current.PlayerVehicle, true);
        }

        public void SetRunning(bool running)
        {
            if (_isRunning == running) return;
            if (Instructions.Count < 2) return;

            _isRunning = running;
            if (Instructions[_currentInstruction].Reverse)
                _currentInstruction--;
            else
                _currentInstruction++;

            SetAction(Behaviour.AfterNonFatalImpact, false);
        }

        private void SetSequence(PedestrianSequence seq)
        {
            if (_currentSequence == seq) return;

            _currentSequence = seq;
            _frameIndex = 0;
            _frameTime = 0;
            _inLoopingFrames = (_currentSequence.InitialFrames.Count == 0);  //if no initialframes, jump straight to looping frames

            UpdateFrameRate();
        }

        public void Update()
        {
            if (_stopUpdating) return;

            float angle = Helpers.GetSignedAngleBetweenVectors(Engine.Camera.Orientation, _direction, false);
            angle = MathHelper.ToDegrees(angle);
            if (angle < 0) angle += 360;

            if (float.IsNaN(angle))
                angle = 0;

            int seq = 0;
            foreach (PedestrianActionSequenceMap seqMap in _currentAction.Sequences)
            {
                if (angle < seqMap.MaxBearing)
                {
                    seq = seqMap.SequenceIndex;
                    SetSequence(Behaviour.Sequences[seqMap.SequenceIndex]);
                    break;
                }
            }

            _frameTime += Engine.ElapsedSeconds;
            if (_frameTime > _frameRate)
            {
                _frameIndex++;
                _frameTime = 0;

                if (!_inLoopingFrames)
                {
                    if (_frameIndex >= _currentSequence.InitialFrames.Count)
                    {
                        if (_currentSequence.LoopingFrames.Count > 0)
                        {
                            _inLoopingFrames = true;
                            _frameIndex = 0;
                        }
                        else
                        {
                            _frameIndex--;
                            _frameTime = -1;  //stop animating
                        }
                    }
                }
                else
                {
                    if (_frameIndex >= _currentSequence.LoopingFrames.Count)
                    {
                        _frameIndex = 0;
                        _frameTime = 0;
                        UpdateFrameRate();
                    }
                }
            }

            if (IsHit)
            {
                Position += _direction * _hitSpeed * Engine.ElapsedSeconds;

                if (_hitSpinSpeed != 0)
                {
                    Position.Y += _hitUpSpeed * Engine.ElapsedSeconds;
                    _hitUpSpeed -= Engine.ElapsedSeconds * 45;
                    _hitSpeed -= Engine.ElapsedSeconds * 10;
                    if (Position.Y <= _groundHeight)
                    {
                        Position.Y = _groundHeight;
                        _hitCurrentSpin = _hitSpinSpeed = _hitUpSpeed = 0;
                        _hitSpeed = 0;
                        _stopUpdating = true;
                    }
                    _hitCurrentSpin += _hitSpinSpeed * Engine.ElapsedSeconds;
                }
                else
                {
                    if (_frameTime == -1)
                        _stopUpdating = true;  //if were not spining the ped, stop them as soon as anim is finished
                }
                if (float.IsNaN(Position.X))
                {
                    
                }

                _physXActor.GlobalPosition = Position;
                
            }
            else if (_isRunning)
            {
                Vector3 target = Instructions[_currentInstruction].Position;
                
                _direction = target - Position;
                if (float.IsNaN(_direction.X))
                {
                }
                if (_direction != Vector3.Zero)
                {
                    _direction.Normalize();
                    Position += _direction * RunningSpeed * Engine.ElapsedSeconds;
                    if (float.IsNaN(_direction.X))
                    {
                    }

                    if (Instructions[_currentInstruction].AutoY)
                    {
                        StillDesign.PhysX.RaycastHit hit = PhysX.Instance.Scene.RaycastClosestShape(new StillDesign.PhysX.Ray(Position + new Vector3(0, 10, 0), Vector3.Down), StillDesign.PhysX.ShapesType.Static);
                        target.Y = hit.WorldImpact.Y;
                        float fallDist = Position.Y - hit.WorldImpact.Y;
                        if (fallDist > 10)
                        {
                            SetAction(Behaviour.NonFatalFalling, true);
                            if (!_isFalling) SoundCache.Play(Behaviour.FallingNoise, Race.Current.PlayerVehicle, true);
                            _isFalling = true;
                        }
                        if (fallDist > 0.5f)
                            Position.Y -= Engine.ElapsedSeconds * 20;
                        else
                        {
                            Position.Y = hit.WorldImpact.Y;
                            if (_isFalling)
                            {
                                OnHit(Race.Current.PlayerVehicle);
                                return;
                            }
                        }
                    }
                    _physXActor.GlobalPosition = Position;
                }

                if (Vector3.Distance(Position, target) < 0.5f)
                {
                    if (Instructions[_currentInstruction].Reverse)
                    {
                        _instructionDirection *= -1;
                    }
                    _currentInstruction += _instructionDirection;
                    if (_currentInstruction == -1) { _currentInstruction = 0; _instructionDirection *= -1; }
                }
            }
        }

        private void UpdateFrameRate()
        {
            switch (_currentSequence.FrameRateType)
            {
                case PedestrianSequenceFrameRate.Variable:
                    _frameRate = 1 / Engine.Random.Next(_currentSequence.MinFrameRate, _currentSequence.MaxFrameRate);
                    break;
                case PedestrianSequenceFrameRate.Speed:
                    _frameRate = 1 / _currentSequence.MaxFrameRate;
                    break;
                case PedestrianSequenceFrameRate.Fixed:
                    _frameRate = 0.1f;
                    break;
            }
        }

        public void Render()
        {
            Matrix world = Matrix.CreateConstrainedBillboard(Position, Engine.Camera.Position, Vector3.Up, null, null);

            PedestrianFrame frame = _inLoopingFrames ? _currentSequence.LoopingFrames[_frameIndex] : _currentSequence.InitialFrames[_frameIndex];
            Vector3 texSize = new Vector3(50, 70, 1);
            if (frame.Texture != null) texSize = new Vector3(frame.Texture.Width, frame.Texture.Height, 1);
            texSize /= Math.Max(texSize.X, texSize.Y);
            Vector3 scale = texSize * new Vector3(Behaviour.Height, Behaviour.Height, 1);

            if (frame.Flipped)
            {
                world = Matrix.CreateRotationY(MathHelper.Pi) * world;
            }

            world = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(_hitCurrentSpin) * world * Matrix.CreateTranslation(frame.Offset) *Matrix.CreateTranslation(0, scale.Y * 0.5f, 0);

            BasicEffect2 effect = GameVars.CurrentEffect;
            effect.World = world;
            effect.Texture = frame.Texture;
            effect.CommitChanges();
            Engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

		bool IsPowerup
		{
			get
			{
				return RefNumber >= 100;
			}
		}
    }

    class PedestrianInstruction
    {
        public Vector3 Position;
        public bool Reverse;
        public bool AutoY;
    }
}
